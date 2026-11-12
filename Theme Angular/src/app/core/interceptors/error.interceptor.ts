import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../services/notification.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notifications = inject(NotificationService);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse) {
        const message =
          (typeof error.error === 'object' && error.error?.message) ||
          error.message ||
          'An unexpected network error occurred.';
        notifications.notify(message, 'error');
      }
      return throwError(() => error);
    })
  );
};
