import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';
import { API_BASE_URL } from '../../web-api-client';
import { CountriesPage } from './countries.page';

const blob = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

describe('CountriesPage', () => {
  it('lists each country with its languages, time zone and currency', async () => {
    TestBed.configureTestingModule({
      imports: [CountriesPage],
      providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_BASE_URL, useValue: '' }],
    });
    const fixture = TestBed.createComponent(CountriesPage);
    const http = TestBed.inject(HttpTestingController);

    http.expectOne((r) => r.url.startsWith('/api/Countries?')).flush(
      blob({
        items: [{ id: '0190f3a0-0000-7000-8000-000000000001', code: 'PK', name: 'Pakistan', languages: ['en', 'ur'], defaultLanguage: 'en', timeZoneId: 'Asia/Karachi', currencyCode: 'PKR' }],
        pageNumber: 1,
        totalPages: 1,
        totalCount: 1,
      })
    );
    http.expectOne('/api/Countries/languages').flush(
      blob([
        { code: 'en', englishName: 'English', nativeName: 'English', isRightToLeft: false },
        { code: 'ur', englishName: 'Urdu', nativeName: 'اردو', isRightToLeft: true },
      ])
    );

    await vi.waitFor(async () => {
      await fixture.whenStable();
      const row: HTMLElement | null = fixture.nativeElement.querySelector('tr.mat-mdc-row');
      expect(row?.textContent).toContain('Pakistan');
      expect(row?.textContent).toContain('English, Urdu');
      expect(row?.textContent).toContain('Asia/Karachi · PKR');
    });
  });
});
