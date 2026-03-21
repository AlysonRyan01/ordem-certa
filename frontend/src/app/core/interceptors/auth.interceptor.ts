import { HttpInterceptorFn, HttpErrorResponse, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

let isRefreshing = false;
const refreshSubject = new BehaviorSubject<string | null>(null);

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const authService = inject(AuthService);

  const isAdminRoute = req.url.includes('/api/admin');
  const isRefreshRoute = req.url.includes('/api/auth/refresh');

  const token = isAdminRoute
    ? localStorage.getItem('admin_token')
    : localStorage.getItem('token');

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && isAdminRoute) {
        localStorage.removeItem('admin_token');
        router.navigate(['/admin/login']);
        return throwError(() => error);
      }

      if (error.status === 401 && !isRefreshRoute) {
        return handleRefresh(req, next, authService, router);
      }

      return throwError(() => error);
    })
  );
};

function handleRefresh(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
  authService: AuthService,
  router: Router
) {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshSubject.next(null);

    return authService.refresh().pipe(
      switchMap((response) => {
        isRefreshing = false;
        refreshSubject.next(response.token);
        const retryReq = req.clone({ setHeaders: { Authorization: `Bearer ${response.token}` } });
        return next(retryReq);
      }),
      catchError((err) => {
        isRefreshing = false;
        refreshSubject.next(null);
        authService.logout();
        return throwError(() => err);
      })
    );
  }

  return refreshSubject.pipe(
    filter((token) => token !== null),
    take(1),
    switchMap((token) => {
      const retryReq = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
      return next(retryReq);
    })
  );
}
