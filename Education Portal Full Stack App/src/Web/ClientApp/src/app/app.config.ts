import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, inject, provideAppInitializer, provideBrowserGlobalErrorListeners } from '@angular/core';
import { MatPaginatorIntl } from '@angular/material/paginator';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { forkJoin } from 'rxjs';
import { routes } from './app.routes';
import { AuthService } from './core/auth/auth.service';
import { BrandingService } from './core/branding/branding.service';
import { LocalizedPaginatorIntl } from './core/i18n/paginator-intl';
import { timeZoneInterceptor } from './core/i18n/time-zone';
import { antiforgeryInterceptor } from './core/http/antiforgery.interceptor';
import { sessionInterceptor } from './core/http/session.interceptor';
import { API_BASE_URL } from './web-api-client';

export function getApiBaseUrl(): string {
  const url = document.getElementsByTagName('base')[0].href;
  return url.endsWith('/') ? url.slice(0, -1) : url;
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withInterceptors([sessionInterceptor, antiforgeryInterceptor, timeZoneInterceptor])),
    { provide: API_BASE_URL, useFactory: getApiBaseUrl },
    { provide: MatPaginatorIntl, useClass: LocalizedPaginatorIntl },
    // The school's branding (picked by the server from this domain) and the session are known before the first page
    // shows, so even the sign-in page is in the school's colours.
    provideAppInitializer(() => forkJoin([inject(BrandingService).load(), inject(AuthService).initialize()])),
  ],
};
