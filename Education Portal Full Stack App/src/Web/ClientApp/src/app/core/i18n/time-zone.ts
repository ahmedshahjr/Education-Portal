import { HttpInterceptorFn } from '@angular/common/http';

/** The header that tells the server which time zone the caller reads dates in (RequestTimeZone.cs). */
export const TIME_ZONE_HEADER = 'X-Time-Zone';

/**
 * The IANA time zone of this device, for example `Asia/Dubai`, or null when the browser does not say. This is the
 * user's location: a teacher of a Pakistani school who travels to Dubai sees times in Dubai time.
 */
export function browserTimeZone(): string | null {
  try {
    return Intl.DateTimeFormat().resolvedOptions().timeZone || null;
  } catch {
    return null;
  }
}

/**
 * Sends the device's time zone with every request to this app's own server, so dates the server turns into text
 * (exports, emails) use the same zone as the screen. Never sent to another site.
 */
export const timeZoneInterceptor: HttpInterceptorFn = (req, next) => {
  const zone = browserTimeZone();
  return zone && isSameOrigin(req.url) ? next(req.clone({ setHeaders: { [TIME_ZONE_HEADER]: zone } })) : next(req);
};

function isSameOrigin(url: string): boolean {
  try {
    return new URL(url, document.baseURI).origin === document.location.origin;
  } catch {
    return false;
  }
}
