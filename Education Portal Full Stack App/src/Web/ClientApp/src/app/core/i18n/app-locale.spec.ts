import { clearTranslations } from '@angular/localize';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { DEFAULT_LOCALE, loadAppLocale, toAppLocale, translationsUrl } from './app-locale';

const urdu = {
  language: 'ur',
  culture: 'ur-PK',
  isRightToLeft: true,
  languages: [
    { code: 'en', englishName: 'English', nativeName: 'English', isRightToLeft: false },
    { code: 'ur', englishName: 'Urdu', nativeName: 'اردو', isRightToLeft: true },
  ],
  timeZone: 'Asia/Karachi',
  currencyCode: 'PKR',
};

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } });

function fakeFetch(routes: Record<string, () => Response | Promise<Response>>): typeof fetch {
  return vi.fn(async (input: RequestInfo | URL) => {
    const route = routes[String(input)];
    if (!route) {
      throw new TypeError(`No route for ${String(input)}`);
    }
    return route();
  }) as unknown as typeof fetch;
}

describe('loadAppLocale', () => {
  afterEach(() => {
    clearTranslations();
    document.documentElement.removeAttribute('dir');
    document.documentElement.lang = 'en';
  });

  it('lays out Urdu right to left once its translations are loaded', async () => {
    const fetchFn = fakeFetch({
      'api/Localization': () => json(urdu),
      [translationsUrl('ur')]: () => json({ locale: 'ur', translations: { 'nav.home': 'ہوم' } }),
    });

    const locale = await loadAppLocale(fetchFn, document);

    const sent = new Headers((fetchFn as unknown as ReturnType<typeof vi.fn>).mock.calls[0][1]?.headers as HeadersInit);
    expect(sent.get('X-Time-Zone')).toBe(Intl.DateTimeFormat().resolvedOptions().timeZone);
    expect(locale.language).toBe('ur');
    expect(locale.culture).toBe('ur-PK');
    expect(document.documentElement.dir).toBe('rtl');
    expect(document.documentElement.lang).toBe('ur');
    expect($localize`:@@nav.home:Home`).toBe('ہوم');
  });

  it('falls back to English, left to right, when the translations cannot be loaded', async () => {
    const fetchFn = fakeFetch({
      'api/Localization': () => json(urdu),
      [translationsUrl('ur')]: () => json({}, 404),
    });

    const locale = await loadAppLocale(fetchFn, document);

    expect(locale.language).toBe('en');
    expect(locale.languages.map((l) => l.code)).toEqual(['en', 'ur']); // the visitor can still pick Urdu again
    expect(document.documentElement.dir).toBe('ltr');
  });

  it('starts in English when the server cannot be reached', async () => {
    const locale = await loadAppLocale(fakeFetch({}), document);

    expect(locale).toEqual(DEFAULT_LOCALE);
  });
});

describe('toAppLocale', () => {
  it('shows dates in the user\u2019s own time zone, else the school\u2019s', () => {
    expect(toAppLocale({ ...urdu, userTimeZone: 'Asia/Dubai' }).userTimeZone).toBe('Asia/Dubai');
    expect(toAppLocale(urdu).userTimeZone).toBe('Asia/Karachi');
  });

  it('keeps only well-formed languages', () => {
    const locale = toAppLocale({ ...urdu, languages: [...urdu.languages, { nativeName: 'no code' }, null] });

    expect(locale.languages.map((l) => l.code)).toEqual(['en', 'ur']);
  });

  it('uses the English default for anything unexpected', () => {
    expect(toAppLocale('nonsense')).toEqual(DEFAULT_LOCALE);
    expect(toAppLocale({ language: 'ur', languages: [] })).toEqual(DEFAULT_LOCALE);
  });
});
