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

async function rowsShouldBe(fixture: { whenStable(): Promise<unknown>; detectChanges(): void; nativeElement: HTMLElement }, names: string[]) {
  // The generated client reads every response as a Blob, which is read asynchronously.
  await vi.waitFor(async () => {
    await fixture.whenStable();
    fixture.detectChanges();
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

  it('filters the loaded schools by name on the page, without another request', async () => {
    const { fixture, http, element } = await render();

    type(element, 'alp');
    await rowsShouldBe(fixture, ['Alpha Academy']);
    expect(http.match(isSchoolsRequest)).toEqual([]);
  });

  it('shows each school\'s plan: days left rounded up, a trial ending today or already ended, a paid plan, a lapsed one and no plan', async () => {
    TestBed.configureTestingModule({
      imports: [SchoolsPage],
      providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_BASE_URL, useValue: '' }],
    });
    const fixture = TestBed.createComponent(SchoolsPage);
    const http = TestBed.inject(HttpTestingController);
    const inDays = (days: number) => new Date(Date.now() + days * 24 * 60 * 60 * 1000).toISOString();
    const school = (id: number, name: string, extra: object) => ({ id: `0190f3a0-0000-7000-8000-0000000001${id}`, name, isMaster: false, ...extra });
    http.expectOne(isSchoolsRequest).flush(
      pageOf([
        school(1, 'Trial School', { subscriptionStatus: 'Trial', trialEndDate: inDays(88) }),
        school(2, 'Ending School', { subscriptionStatus: 'Trial', trialEndDate: inDays(-1) }),
        school(3, 'Paid School', { subscriptionStatus: 'Active' }),
        school(4, 'Late School', { subscriptionStatus: 'PastDue' }),
        school(5, 'New School', {}),
        school(6, 'Hour School', { subscriptionStatus: 'Trial', trialEndDate: inDays(1 / 24) }),
        school(7, 'Past School', { subscriptionStatus: 'Trial', trialEndDate: inDays(-3) }),
      ])
    );
    http.expectOne((r) => r.url.startsWith('/api/Countries?')).flush(countries);
    const element = fixture.nativeElement as HTMLElement;

    await vi.waitFor(() => {
      fixture.detectChanges();
      const plans = [...element.querySelectorAll('tr.mat-mdc-row')].map((row) => row.textContent ?? '');
      expect(plans).toHaveLength(7);
      expect(plans[5]).toContain('Trial (1 day left)');
      expect(plans[6]).toContain('Trial ends today');
      expect(plans[0]).toContain('Trial (');
      expect(plans[0]).toContain('88');
      expect(plans[1]).toContain('Trial ends today');
      expect(plans[2]).toContain('Paid');
      expect(plans[3]).toContain('Payment overdue');
      expect(plans[4]).toContain('No plan');
    });
  });

  it('limits the table to one country on the page', async () => {
    const { fixture, element } = await render();
    const loader = TestbedHarnessEnvironment.loader(fixture);
    const filter = await loader.getHarness(MatSelectHarness.with({ selector: '.mm-schools__country-filter mat-select' }));

    await filter.open();
    await filter.clickOptions({ text: 'Pakistan' });

    await rowsShouldBe(fixture, ['Alpha Academy']);
    expect(element.querySelector('input[type="search"]')).not.toBeNull();
  });

  it('clears the search and the country from the empty state', async () => {
    const { fixture, element } = await render();

    type(element, 'zzz');
    await vi.waitFor(() => {
      fixture.detectChanges();
      expect(element.textContent).toContain('Clear search');
    });

    [...element.querySelectorAll<HTMLButtonElement>('button')].find((b) => b.textContent?.includes('Clear search'))!.click();

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

    // Saving reloads the whole list, so the new school is in it.
    const request = await nextSchoolsRequest(http);
    expect(request.request.urlWithParams).toBe('/api/Schools?pageNumber=1&pageSize=100');
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
