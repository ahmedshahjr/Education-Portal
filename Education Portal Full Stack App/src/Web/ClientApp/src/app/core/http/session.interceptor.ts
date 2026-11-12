import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

/** These calls answer 401 as a normal result, so that answer must not send the visitor to the login page. */
const ANONYMOUS_ANSWER_PATHS = ['/api/Users/me', '/api/Users/login'];

/**
 * Sends the session cookie with every API call, and sends the visitor to the login page when the session has ended
 * (for example after the 15-minute idle timeout).
 */
export const sessionInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  return next(req.clone({ withCredentials: true })).pipe(
    catchError((error: unknown) => {
      if (
        error instanceof HttpErrorResponse &&
        error.status === 401 &&
        !isAnonymousAnswer(error.url) &&
        !router.url.startsWith('/login')
      ) {
        void router.navigate(['/login'], { queryParams: { returnUrl: router.url } });
      }
      return throwError(() => error);
    })
  );
};

function isAnonymousAnswer(url: string | null): boolean {
  if (!url) {
    return false;
  }
  const path = new URL(url, window.location.origin).pathname;
  return ANONYMOUS_ANSWER_PATHS.some((answerPath) => path.endsWith(answerPath));
}
