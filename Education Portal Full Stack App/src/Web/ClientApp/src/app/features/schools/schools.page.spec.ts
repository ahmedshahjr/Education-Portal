import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting, TestRequest } from '@angular/common/http/testing';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { TestBed } from '@angular/core/testing';
import { MatSelectHarness } from '@angular/material/select/testing';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { API_BASE_URL } from '../../web-api-client';
import { SchoolsPage } from './schools.page';

const blob = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

const pageOf = (items: { id: string; name: string; countryId?: string }[]) =>
  blob({
    items: items.map((s) => ({ ...s, domain: `${s.name.toLowerCase().replace(/ /g, '')}.localhost`, isMaster: false })),
    pageNumber: 1,
    totalPages: 1,
    totalCount: items.length,
  });

const countries = blob({
  items: [
    { id: '0190f3a0-0000-7000-8000-000000000001', code: 'PK', name: 'Pakistan', languages: ['en'], defaultLanguage: 'en', timeZoneId: 'Asia/Karachi', currencyCode: 'PKR' },
    { id: '0190f3a0-0000-7000-8000-000000000002', code: 'AE', name: 'United Arab Emirates', languages: ['en'], defaultLanguage: 'en', timeZoneId: 'Asia/Dubai', currencyCode: 'AED' },
  ],
  pageNumber: 1,
  totalPages: 1,
  totalCount: 2,
});

const isSchoolsRequest = (r: { url: string }) => r.url.startsWith('/api/Schools?');

async function render() {
  TestBed.configureTestingModule({
    imports: [SchoolsPage],
    providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_BASE_URL, useValue: '' }],
  });
  const fixture = TestBed.createComponent(SchoolsPage);
  const http = TestBed.inject(HttpTestingController);
  http.expectOne(isSchoolsRequest).flush(pageOf([{ id: '0190f3a0-0000-7000-8000-000000000007', name: 'Alpha Academy', countryId: '0190f3a0-0000-7000-8000-000000000001' }, { id: '0190f3a0-0000-7000-8000-000000000008', name: 'Beta School' }]));
  http.expectOne((r) => r.url.startsWith('/api/Countries?')).flush(countries);
  const element = fixture.nativeElement as HTMLElement;
  await rowsShouldBe(fixture, ['Alpha Academy', 'Beta School']);
  return { fixture, http, element };
}

async function rowsShouldBe(fixture: { whenStable(): Promise<unknown>; nativeElement: HTMLElement }, names: string[]) {
  // The generated client reads every response as a Blob, which is read asynchronously.
  await vi.waitFor(async () => {
    await fixture.whenStable();
    const rows = [...fixture.nativeElement.querySelectorAll('tr.mat-mdc-row td:first-child')];
    expect(rows.map((r) => r.textContent?.trim())).toEqual(names);
  });
}

/** Waits for the (debounced) request for schools and returns it. */
async function nextSchoolsRequest(http: HttpTestingController): Promise<TestRequest> {
  let request: TestRequest | undefined;
  await vi.waitFor(() => {
    request = http.expectOne(isSchoolsRequest);
  });
  return request!;
}

function type(element: HTMLElement, text: string) {
  const input = element.querySelector<HTMLInputElement>('input[type="search"]')!;
  input.value = text;
  input.dispatchEvent(new Event('input'));
}

describe('SchoolsPage', () => {
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('searches schools by name on the server from the first page', async () => {
    const { fixture, http, element } = await render();

    type(element, 'alp');
    type(element, 'alpha ');

    const request = await nextSchoolsRequest(http);
    expect(request.request.urlWithParams).toBe('/api/Schools?pageNumber=1&pageSize=25&search=alpha');
    request.flush(pageOf([{ id: '0190f3a0-0000-7000-8000-000000000007', name: 'Alpha Academy', countryId: '0190f3a0-0000-7000-8000-000000000001' }]));

    await rowsShouldBe(fixture, ['Alpha Academy']);
  });

  it('limits the table to one country', async () => {
    const { fixture, http } = await render();
    const loader = TestbedHarnessEnvironment.loader(fixture);
    const filter = await loader.getHarness(MatSelectHarness.with({ selector: '.mm-schools__country-filter mat-select' }));

    await filter.open();
    await filter.clickOptions({ text: 'United Arab Emirates' });

    const request = await nextSchoolsRequest(http);
    expect(request.request.urlWithParams).toBe('/api/Schools?pageNumber=1&pageSize=25&countryId=0190f3a0-0000-7000-8000-000000000002');
    request.flush(pageOf([]));

    await vi.waitFor(async () => {
      await fixture.whenStable();
      expect(fixture.nativeElement.textContent).toContain('No schools match');
    });
  });

  it('clears the search and the country from the empty state', async () => {
    const { fixture, http, element } = await render();

    type(element, 'zzz');
    (await nextSchoolsRequest(http)).flush(pageOf([]));
    await vi.waitFor(async () => {
      await fixture.whenStable();
      expect(element.textContent).toContain('No schools match');
    });

    [...element.querySelectorAll<HTMLButtonElement>('button')].find((b) => b.textContent?.includes('Clear filters'))!.click();

    const request = await nextSchoolsRequest(http);
    expect(request.request.urlWithParams).toBe('/api/Schools?pageNumber=1&pageSize=25');
    request.flush(pageOf([{ id: '0190f3a0-0000-7000-8000-000000000007', name: 'Alpha Academy', countryId: '0190f3a0-0000-7000-8000-000000000001' }, { id: '0190f3a0-0000-7000-8000-000000000008', name: 'Beta School' }]));

    await rowsShouldBe(fixture, ['Alpha Academy', 'Beta School']);
    expect(element.querySelector<HTMLInputElement>('input[type="search"]')!.value).toBe('');
  });

  it('adds a school with its first administrator and then shows it', async () => {
    const { fixture, http, element } = await render();

    [...element.querySelectorAll<HTMLButtonElement>('button')].find((b) => b.textContent?.trim() === 'Add school')!.click();
    await fixture.whenStable();
    fillField(element, 'name', 'New School');
    fillField(element, 'domain', 'new.localhost');
    fillField(element, 'administratorEmail', 'head@new.local');
    fillField(element, 'administratorPassword', 'violet harbour lantern 47');
    element.querySelector<HTMLButtonElement>('app-school-form button[type="submit"]')!.click();

    const create = http.expectOne((r) => r.method === 'POST' && r.url === '/api/Schools');
    expect(JSON.parse(create.request.body as string)).toEqual({
      name: 'New School',
      domain: 'new.localhost',
      administratorEmail: 'head@new.local',
      administratorPassword: 'violet harbour lantern 47',
    });
    create.flush(blob('0190f3a0-0000-7000-8000-000000000009'), { status: 201, statusText: 'Created' });

    const request = await nextSchoolsRequest(http);
    expect(request.request.urlWithParams).toBe('/api/Schools?pageNumber=1&pageSize=25&search=New%20School');
    request.flush(pageOf([{ id: '0190f3a0-0000-7000-8000-000000000009', name: 'New School' }]));

    await rowsShouldBe(fixture, ['New School']);
    expect(element.querySelector('app-school-form')).toBeNull();
  });

  it('keeps the form open and shows the server\'s message when the school is refused', async () => {
    const { fixture, http, element } = await render();

    [...element.querySelectorAll<HTMLButtonElement>('button')].find((b) => b.textContent?.trim() === 'Add school')!.click();
    await fixture.whenStable();
    fillField(element, 'name', 'Copy');
    fillField(element, 'domain', 'alphaacademy.localhost');
    fillField(element, 'administratorEmail', 'head@new.local');
    fillField(element, 'administratorPassword', 'violet harbour lantern 47');
    element.querySelector<HTMLButtonElement>('app-school-form button[type="submit"]')!.click();

    http
      .expectOne((r) => r.method === 'POST' && r.url === '/api/Schools')
      .flush(blob({ status: 400, title: 'Bad Request', errors: { Domain: ['Another school already uses this domain.'] } }), {
        status: 400,
        statusText: 'Bad Request',
      });

    await vi.waitFor(async () => {
      await fixture.whenStable();
      expect(element.querySelector('app-school-form')?.textContent).toContain('Another school already uses this domain.');
    });
  });
});

function fillField(element: HTMLElement, name: string, value: string) {
  const input = element.querySelector<HTMLInputElement>(`app-school-form input[formcontrolname="${name}"]`)!;
  input.value = value;
  input.dispatchEvent(new Event('input'));
}
