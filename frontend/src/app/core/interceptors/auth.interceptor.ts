import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const isAdminRoute = req.url.includes('/api/admin');
  const token = isAdminRoute
    ? localStorage.getItem('admin_token')
    : localStorage.getItem('token');

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        if (isAdminRoute) {
          localStorage.removeItem('admin_token');
          router.navigate(['/admin/login']);
        } else {
          localStorage.removeItem('token');
          router.navigate(['/login']);
        }
      }
      return throwError(() => error);
    })
  );
};
