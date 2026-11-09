import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';

/** These calls answer 401 as a normal result, so that answer must not send the visitor to the login page. */
const ANONYMOUS_ANSWER_PATHS = ['/api/Users/me', '/api/Users/login'];

@Injectable({
  providedIn: 'root'
})
export class AuthorizeInterceptor implements HttpInterceptor {
  constructor(private router: Router) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const authReq = req.clone({ withCredentials: true });
    return next.handle(authReq).pipe(
      catchError(error => {
        if (error instanceof HttpErrorResponse
          && error.status === 401
          && !isAnonymousAnswer(error.url)
          && !this.router.url.startsWith('/login')) {
          this.router.navigate(['/login'], { queryParams: { returnUrl: window.location.pathname } });
        }
        return throwError(() => error);
      })
    );
  }
}

function isAnonymousAnswer(url: string | null): boolean {
  if (!url) {
    return false;
  }
  const path = new URL(url, window.location.origin).pathname;
  return ANONYMOUS_ANSWER_PATHS.some(answerPath => path.endsWith(answerPath));
}
