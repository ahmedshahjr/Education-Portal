import { DOCUMENT } from '@angular/common';
import { inject, Injectable, InjectionToken, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';
import { ChangeLanguageCommand, LocalizationClient } from '../../web-api-client';
import { APP_LOCALE } from './app-locale';

/** Reloads the page. A token, so tests can check the reload without leaving the page. */
export const PAGE_RELOAD = new InjectionToken<() => void>('PAGE_RELOAD', {
  providedIn: 'root',
  factory: () => {
    const document = inject(DOCUMENT);
    return () => document.defaultView?.location.reload();
  },
});

/**
 * The page's language and the school's other languages. Switching saves the choice on the server (a cookie on this
 * school's domain) and reloads, because Angular's translations are loaded once, before the app starts.
 */
@Injectable({ providedIn: 'root' })
export class LocaleService {
  private readonly locale = inject(APP_LOCALE);
  private readonly client = inject(LocalizationClient);
  private readonly reload = inject(PAGE_RELOAD);

  readonly language = this.locale.language;
  readonly culture = this.locale.culture;
  readonly languages = this.locale.languages;
  readonly direction: 'ltr' | 'rtl' = this.locale.isRightToLeft ? 'rtl' : 'ltr';
  readonly timeZone = this.locale.timeZone;
  readonly currencyCode = this.locale.currencyCode;

  /** True while a switch is being saved, so the menu can't be used twice. */
  readonly changing = signal(false);

  changeLanguage(code: string): Observable<void> {
    this.changing.set(true);
    return this.client.changeLanguage(new ChangeLanguageCommand({ language: code })).pipe(
      tap(() => this.reload()),
      finalize(() => this.changing.set(false))
    );
  }
}
