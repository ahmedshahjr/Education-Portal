import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { API_BASE_URL } from '../../web-api-client';
import { APP_LOCALE, DEFAULT_LOCALE } from './app-locale';
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
