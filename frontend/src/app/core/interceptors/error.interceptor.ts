import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401) {
        const messages: string[] = error.error?.errors ?? ['Erro inesperado. Tente novamente.'];
        messages.forEach(msg =>
          snackBar.open(msg, 'Fechar', { duration: 5000, panelClass: ['snack-error'] })
        );
      }
      return throwError(() => error);
    })
  );
};
