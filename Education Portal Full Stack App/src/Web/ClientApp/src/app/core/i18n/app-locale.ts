import { registerLocaleData } from '@angular/common';
import { DEFAULT_CURRENCY_CODE, InjectionToken, LOCALE_ID, Provider } from '@angular/core';
import { loadTranslations } from '@angular/localize';
import { browserTimeZone, TIME_ZONE_HEADER } from './time-zone';

/** A language the school offers, as the language menu shows it. */
export interface LanguageOption {
  readonly code: string;
  readonly nativeName: string;
  readonly isRightToLeft: boolean;
}

/**
 * The language and regional settings this page was started with. The server picks them from the visitor's choice,
 * their browser and the school's country (`GET /api/Localization`), before the app starts. Changing the language
 * reloads the page, so these values never change while the app runs.
 */
export interface AppLocale {
  /** ISO 639-1 code, for example `ur`. Angular's LOCALE_ID. */
  readonly language: string;
  /** BCP 47 culture for dates and numbers, for example `ur-PK`. */
  readonly culture: string;
  readonly isRightToLeft: boolean;
  readonly languages: readonly LanguageOption[];
  /** IANA time zone of the school, for example `Asia/Karachi`. */
  readonly timeZone: string;
  /**
   * IANA time zone dates are shown in: this device's own (the user's location), else the school's. The server uses the
   * same one, because the app sends it with every request.
   */
  readonly userTimeZone: string;
  /** ISO 4217 code of the school's currency, or null without a country. */
  readonly currencyCode: string | null;
}

export const DEFAULT_LOCALE: AppLocale = {
  language: 'en',
  culture: 'en',
  isRightToLeft: false,
  languages: [{ code: 'en', nativeName: 'English', isRightToLeft: false }],
  timeZone: 'UTC',
  userTimeZone: browserTimeZone() ?? 'UTC',
  currencyCode: null,
};

export const APP_LOCALE = new InjectionToken<AppLocale>('APP_LOCALE', {
  providedIn: 'root',
  factory: () => DEFAULT_LOCALE,
});

/**
 * Angular's number and date data for each language with translations. Imported only for the current language. English
 * is built into Angular.
 */
const LOCALE_DATA: Readonly<Record<string, () => Promise<{ default: unknown }>>> = {
  ar: () => import('@angular/common/locales/ar'),
  ur: () => import('@angular/common/locales/ur'),
};

/** Where the translations of a language are served from (built from src/locale/messages.<code>.json). */
export function translationsUrl(language: string): string {
  return `i18n/messages.${language}.json`;
}

/**
 * Asks the server which language to use, loads its translations and number/date data, and sets `lang` and `dir` on
 * the page. It must finish before the app's code is imported, so every `$localize` message is translated
 * (Angular's runtime translation loading). Any failure falls back to English, so the app still starts.
 */
export async function loadAppLocale(fetchFn: typeof fetch = fetch, document: Document = globalThis.document): Promise<AppLocale> {
  const locale = await fetchLocale(fetchFn);
  const applied = locale.language === 'en' ? locale : await loadLanguage(locale, fetchFn);

  document.documentElement.lang = applied.language;
  document.documentElement.dir = applied.isRightToLeft ? 'rtl' : 'ltr';

  return applied;
}

/** The providers that hand the started-with locale to Angular (pipes, Material, the app's own services). */
export function provideAppLocale(locale: AppLocale): Provider[] {
  return [
    { provide: APP_LOCALE, useValue: locale },
    { provide: LOCALE_ID, useValue: locale.language },
    ...(locale.currencyCode ? [{ provide: DEFAULT_CURRENCY_CODE, useValue: locale.currencyCode }] : []),
  ];
}

async function fetchLocale(fetchFn: typeof fetch): Promise<AppLocale> {
  try {
    const zone = browserTimeZone();
    const headers: Record<string, string> = { Accept: 'application/json', ...(zone ? { [TIME_ZONE_HEADER]: zone } : {}) };
    const response = await fetchFn('api/Localization', { credentials: 'same-origin', headers });
    return response.ok ? toAppLocale(await response.json()) : DEFAULT_LOCALE;
  } catch {
    // Offline or the server is restarting: the page still starts, in English.
    return DEFAULT_LOCALE;
  }
}

async function loadLanguage(locale: AppLocale, fetchFn: typeof fetch): Promise<AppLocale> {
  const localeData = LOCALE_DATA[locale.language];

  try {
    const response = await fetchFn(translationsUrl(locale.language), { credentials: 'same-origin' });
    if (!response.ok || !localeData) {
      return { ...DEFAULT_LOCALE, languages: locale.languages, userTimeZone: locale.userTimeZone };
    }

    const file = (await response.json()) as { translations?: Record<string, string> };
    loadTranslations(file.translations ?? {});
    registerLocaleData((await localeData()).default, locale.language);
    return locale;
  } catch {
    // Without its translations the language cannot be shown; English keeps the page usable.
    return { ...DEFAULT_LOCALE, languages: locale.languages, userTimeZone: locale.userTimeZone };
  }
}

/** Reads the API's answer defensively: anything unexpected becomes the English default. */
export function toAppLocale(value: unknown): AppLocale {
  if (typeof value !== 'object' || value === null) {
    return DEFAULT_LOCALE;
  }

  const data = value as Record<string, unknown>;
  const languages = Array.isArray(data['languages'])
    ? data['languages']
        .filter((l): l is Record<string, unknown> => typeof l === 'object' && l !== null && typeof l['code'] === 'string')
        .map((l) => ({
          code: String(l['code']),
          nativeName: typeof l['nativeName'] === 'string' ? l['nativeName'] : String(l['code']),
          isRightToLeft: l['isRightToLeft'] === true,
        }))
    : [];

  if (typeof data['language'] !== 'string' || languages.length === 0) {
    return DEFAULT_LOCALE;
  }

  const timeZone = typeof data['timeZone'] === 'string' ? data['timeZone'] : 'UTC';

  return {
    language: data['language'],
    culture: typeof data['culture'] === 'string' ? data['culture'] : data['language'],
    isRightToLeft: data['isRightToLeft'] === true,
    languages,
    timeZone,
    userTimeZone: typeof data['userTimeZone'] === 'string' ? data['userTimeZone'] : timeZone,
    currencyCode: typeof data['currencyCode'] === 'string' ? data['currencyCode'] : null,
  };
}
