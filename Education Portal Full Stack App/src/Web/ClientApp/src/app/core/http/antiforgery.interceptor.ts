import { HttpInterceptorFn } from '@angular/common/http';

/** The names the server uses for the antiforgery token (AntiforgeryTokenCookie.cs). */
export const ANTIFORGERY_COOKIE_NAME = '__Host-XSRF-TOKEN';
export const ANTIFORGERY_HEADER_NAME = 'X-XSRF-TOKEN';

const SAFE_METHODS = ['GET', 'HEAD', 'OPTIONS', 'TRACE'];

/**
 * Sends the antiforgery token in a header with every unsafe request (SEC-WEB-03). Angular's built-in XSRF support
 * skips absolute URLs, and this app calls the API with absolute URLs, so the token is copied here instead.
 */
export const antiforgeryInterceptor: HttpInterceptorFn = (req, next) => {
  if (SAFE_METHODS.includes(req.method.toUpperCase())) {
    return next(req);
  }

  const token = readCookie(document.cookie, ANTIFORGERY_COOKIE_NAME);
  return next(token ? req.clone({ setHeaders: { [ANTIFORGERY_HEADER_NAME]: token } }) : req);
};

/** Returns the value of one cookie from a document.cookie string, or undefined when the cookie is absent. */
export function readCookie(cookies: string, name: string): string | undefined {
  const prefix = `${name}=`;
  const entry = cookies
    .split(';')
    .map((part) => part.trim())
    .find((part) => part.startsWith(prefix));
  return entry ? entry.substring(prefix.length) || undefined : undefined;
}
