import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { API_BASE_URL } from '../../web-api-client';
import { clearTranslations } from '@angular/localize';
import { APP_LOCALE, DEFAULT_LOCALE, loadAppLocale, provideAppLocale, translationsUrl } from './app-locale';
import { CalendarDatePipe } from './calendar-date.pipe';
import { LocalDatePipe } from './local-date.pipe';
import { LocaleService, PAGE_RELOAD } from './locale.service';

describe('LocaleService', () => {
  const reload = vi.fn();

  beforeEach(() => {
    reload.mockReset();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: '' },
        { provide: PAGE_RELOAD, useValue: reload },
        {
          provide: APP_LOCALE,
          useValue: {
            ...DEFAULT_LOCALE,
            languages: [...DEFAULT_LOCALE.languages, { code: 'ur', nativeName: 'اردو', isRightToLeft: true }],
          },
        },
      ],
    });
  });

  it('saves the new language and reloads the page in it', async () => {
    const service = TestBed.inject(LocaleService);
    const done = vi.fn();

    service.changeLanguage('ur').subscribe({ complete: done });
    const request = TestBed.inject(HttpTestingController).expectOne('/api/Localization/language');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toContain('"language":"ur"');
    request.flush(new Blob(), { status: 204, statusText: 'No Content' });

    await vi.waitFor(() => expect(done).toHaveBeenCalled());
    expect(reload).toHaveBeenCalledOnce();
    expect(service.changing()).toBe(false);
  });

  it('does not reload when the server refuses the language', async () => {
    const service = TestBed.inject(LocaleService);
    const failed = vi.fn();

    service.changeLanguage('ar').subscribe({ error: failed });
    TestBed.inject(HttpTestingController)
      .expectOne('/api/Localization/language')
      .flush(new Blob([JSON.stringify({ errors: { Language: ['This school does not offer that language.'] } })]), {
        status: 400,
        statusText: 'Bad Request',
      });

    await vi.waitFor(() => expect(failed).toHaveBeenCalled());
    expect(reload).not.toHaveBeenCalled();
  });

  it('reports when the user\u2019s saved language differs from the page after sign-in', async () => {
    const service = TestBed.inject(LocaleService);
    const result = vi.fn();

    service.languageChangedBySignIn().subscribe(result);
    TestBed.inject(HttpTestingController)
      .expectOne('/api/Localization')
      .flush(new Blob([JSON.stringify({ language: 'ur', culture: 'ur-PK', languages: [] })], { type: 'application/json' }));

    await vi.waitFor(() => expect(result).toHaveBeenCalledWith(true));
  });

  it('reports no change when the saved language is the page\u2019s', async () => {
    const service = TestBed.inject(LocaleService);
    const result = vi.fn();

    service.languageChangedBySignIn().subscribe(result);
    TestBed.inject(HttpTestingController)
      .expectOne('/api/Localization')
      .flush(new Blob([JSON.stringify({ language: 'en', culture: 'en', languages: [] })], { type: 'application/json' }));

    await vi.waitFor(() => expect(result).toHaveBeenCalledWith(false));
  });
});

describe('LocalDatePipe and CalendarDatePipe, through the real LocaleService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: '' },
        { provide: APP_LOCALE, useValue: { ...DEFAULT_LOCALE, culture: 'en-GB', timeZone: 'Europe/London', userTimeZone: 'Europe/London' } },
      ],
    });
  });

  it('shows an instant in the user\'s zone and follows daylight saving', () => {
    const pipe = TestBed.runInInjectionContext(() => new LocalDatePipe());

    expect(pipe.transform('2026-01-15T12:00:00Z', 'time')).toBe('12:00');
    expect(pipe.transform('2026-07-15T12:00:00Z', 'time')).toBe('13:00');
  });

  it('shows nothing for a missing or invalid instant', () => {
    const pipe = TestBed.runInInjectionContext(() => new LocalDatePipe());

    expect(pipe.transform(null)).toBe('');
    expect(pipe.transform(undefined)).toBe('');
    expect(pipe.transform('')).toBe('');
    expect(pipe.transform('not a date')).toBe('');
  });

  it('shows a calendar day as the day it is, in the page culture, without a time zone shift', () => {
    const pipe = TestBed.runInInjectionContext(() => new CalendarDatePipe());

    expect(pipe.transform(new Date(2026, 7, 1))).toMatch(/1 Aug 2026/);
    expect(pipe.transform(null)).toBe('');
  });
});

describe('start-up locale, then the real LocaleService', () => {
  const urdu = {
    language: 'ur',
    culture: 'ur-PK',
    isRightToLeft: true,
    languages: [
      { code: 'en', englishName: 'English', nativeName: 'English', isRightToLeft: false },
      { code: 'ur', englishName: 'Urdu', nativeName: 'اردو', isRightToLeft: true },
      { englishName: 'No code', nativeName: 'x', isRightToLeft: false },
    ],
    timeZone: 'Asia/Karachi',
    currencyCode: 'PKR',
  };
  const json = (body: unknown, status = 200) => new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } });
  const fetchWith = (routes: Record<string, () => Response>) =>
    vi.fn(async (input: RequestInfo | URL) => {
      const route = routes[String(input)];
      if (!route) {
        throw new TypeError(`No route for ${String(input)}`);
      }
      return route();
    }) as unknown as typeof fetch;

  afterEach(() => {
    clearTranslations();
    document.documentElement.removeAttribute('dir');
    document.documentElement.lang = 'en';
  });

  it('starts in Urdu, right to left, with the translations and the languages the server offers', async () => {
    const locale = await loadAppLocale(
      fetchWith({
        'api/Localization': () => json(urdu),
        [translationsUrl('ur')]: () => json({ locale: 'ur', translations: { 'nav.home': 'ہوم' } }),
      }),
      document
    );
    TestBed.configureTestingModule({ providers: [provideAppLocale(locale), provideHttpClient(), provideHttpClientTesting(), { provide: API_BASE_URL, useValue: '' }] });
    const service = TestBed.inject(LocaleService);

    expect(service.culture).toBe('ur-PK');
    expect(service.userTimeZone).toBe('Asia/Karachi');
    expect(document.documentElement.dir).toBe('rtl');
    expect(document.documentElement.lang).toBe('ur');
    expect(service.languages.map((l) => l.code)).toEqual(['en', 'ur']); // a language with no code is dropped
  });

  it('falls back to English, left to right, when the Urdu translations are missing', async () => {
    const locale = await loadAppLocale(
      fetchWith({ 'api/Localization': () => json(urdu), [translationsUrl('ur')]: () => json({}, 404) }),
      document
    );
    TestBed.configureTestingModule({ providers: [provideAppLocale(locale), provideHttpClient(), provideHttpClientTesting(), { provide: API_BASE_URL, useValue: '' }] });

    expect(TestBed.inject(LocaleService).culture).toBe('en');
    expect(document.documentElement.dir).toBe('ltr');
  });

  it('starts in English when the server cannot be reached', async () => {
    const locale = await loadAppLocale(fetchWith({}), document);

    expect(locale).toEqual(DEFAULT_LOCALE);
  });
});
