import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../services/notification.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notifications = inject(NotificationService);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse) {
        const body: unknown = error.error;
        const bodyMessage =
          typeof body === 'object' &&
          body !== null &&
          'message' in body &&
          typeof body.message === 'string'
            ? body.message
            : undefined;
        const message = bodyMessage || error.message || 'An unexpected network error occurred.';
        notifications.notify(message, 'error');
      }
      return throwError(() => error);
    }),
  );
};
