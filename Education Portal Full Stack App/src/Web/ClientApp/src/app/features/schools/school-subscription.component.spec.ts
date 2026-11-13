import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { SchoolSummaryDto } from '../../web-api-client';
import { SchoolSubscriptionComponent } from './school-subscription.component';

const json = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });
const SCHOOL = '0190f3a0-0000-7000-8000-000000000005';

const subscription = {
  id: '0190f3a0-0000-7000-8000-000000000009',
  planCode: 'BASIC',
  countryCode: 'PK',
  currencyCode: 'PKR',
  billingCycle: 'Monthly',
  amount: 25500,
  status: 'Active',
  startDate: '2026-11-01T00:00:00Z',
  endDate: '2026-12-01T00:00:00Z',
  modules: [
    { code: 'STUDENTS', status: 'Active', validFrom: '2026-11-01T00:00:00Z', validTo: '2026-12-01T00:00:00Z' },
    { code: 'EXAMS', status: 'Active', validFrom: '2026-11-01T00:00:00Z', validTo: '2026-12-01T00:00:00Z' },
    { code: 'LIBRARY', status: 'Active', validFrom: '2026-11-01T00:00:00Z', validTo: '2026-12-01T00:00:00Z' },
  ],
};
const plans = [{ id: 'p1', code: 'BASIC', name: 'Basic', description: '', isActive: true, modules: ['EXAMS'] }];
const modules = [
  { code: 'STUDENTS', name: 'Students', description: '', isCore: true, isActive: true, sortOrder: 1, requires: [] },
  { code: 'EXAMS', name: 'Exams', description: '', isCore: false, isActive: true, sortOrder: 2, requires: [] },
  { code: 'LIBRARY', name: 'Library', description: '', isCore: false, isActive: true, sortOrder: 3, requires: [] },
];
const invoice = {
  id: '0190f3a0-0000-7000-8000-000000000021',
  schoolId: SCHOOL,
  number: 'INV-000001',
  status: 'Open',
  currencyCode: 'PKR',
  subtotal: 25500,
  taxRate: 0,
  taxAmount: 0,
  total: 25500,
  issueDate: '2026-11-01T00:00:00Z',
  dueDate: '2026-12-01T00:00:00Z',
  periodStart: '2026-11-01T00:00:00Z',
  periodEnd: '2026-12-01T00:00:00Z',
  lines: [],
  payments: [],
};

async function render(subscriptionResponse: { body?: unknown; status?: number }) {
  TestBed.configureTestingModule({ imports: [SchoolSubscriptionComponent], providers: [provideHttpClient(), provideHttpClientTesting()] });
  const http = TestBed.inject(HttpTestingController);
  const fixture = TestBed.createComponent(SchoolSubscriptionComponent);
  fixture.componentRef.setInput('school', new SchoolSummaryDto({ id: SCHOOL, name: 'School One', domain: 'school1.com', isMaster: false, countryId: undefined }));
  fixture.detectChanges();
  http.expectOne('/api/Plans').flush(json(plans));
  http.expectOne('/api/Modules').flush(json(modules));
  const response = http.expectOne(`/api/Schools/${SCHOOL}/subscription`);
  if (subscriptionResponse.status === 404) {
    response.flush(json({}), { status: 404, statusText: 'Not Found' });
  } else {
    response.flush(json(subscriptionResponse.body));
    await vi.waitFor(() => http.expectOne(`/api/Invoices?schoolId=${SCHOOL}`).flush(json([invoice])));
  }
  const element = fixture.nativeElement as HTMLElement;
  await vi.waitFor(() => {
    fixture.detectChanges();
    expect(element.querySelector('app-loading-state')).toBeNull();
  });
  return { fixture, http, element };
}

const button = (element: HTMLElement, text: string) =>
  [...element.querySelectorAll<HTMLButtonElement>('button')].find((b) => b.textContent?.trim().startsWith(text))!;

describe('SchoolSubscriptionComponent', () => {
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('offers to start a subscription when the school has none', async () => {
    const { element } = await render({ status: 404 });

    expect(element.querySelector('form')).not.toBeNull();
    expect(button(element, 'Start subscription').disabled).toBe(true); // a plan must be chosen first
  });

  it('lets the master remove an add-on but not a core module or one of the plan', async () => {
    const { element } = await render({ body: subscription });

    const rows = [...element.querySelectorAll('ul')[0].querySelectorAll('li')];
    expect(rows.map((r) => r.querySelector('button') !== null)).toEqual([false, false, true]); // STUDENTS core, EXAMS in the plan, LIBRARY add-on
  });

  it('suspends the subscription and reloads it', async () => {
    const { http, element } = await render({ body: subscription });

    button(element, 'Suspend').click();

    const request = http.expectOne(`/api/Schools/${SCHOOL}/subscription/suspend`);
    expect(request.request.method).toBe('POST');
    request.flush(null, { status: 204, statusText: 'No Content' });
    await vi.waitFor(() => http.expectOne(`/api/Schools/${SCHOOL}/subscription`).flush(json({ ...subscription, status: 'Suspended' })));
    await vi.waitFor(() => http.expectOne(`/api/Invoices?schoolId=${SCHOOL}`).flush(json([invoice])));
  });

  it('records a payment for the whole total of an open invoice', async () => {
    const { fixture, http, element } = await render({ body: subscription });
    await vi.waitFor(() => {
      fixture.detectChanges();
      expect(button(element, 'Record payment')).toBeDefined();
    });

    button(element, 'Record payment').click();
    fixture.detectChanges();
    const reference = element.querySelector<HTMLInputElement>('input[formcontrolname="reference"]')!;
    reference.value = 'TRX-1';
    reference.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    button(element, 'Record payment of').click();

    const request = http.expectOne(`/api/Invoices/${invoice.id}/payments`);
    expect(request.request.method).toBe('POST');
    expect(JSON.parse(request.request.body as string)).toMatchObject({ invoiceId: invoice.id, amount: 25500, method: 'BankTransfer', reference: 'TRX-1' });
    request.flush(json('0190f3a0-0000-7000-8000-000000000031'));
    await vi.waitFor(() => http.expectOne(`/api/Schools/${SCHOOL}/subscription`).flush(json(subscription)));
    await vi.waitFor(() => http.expectOne(`/api/Invoices?schoolId=${SCHOOL}`).flush(json([])));
  });
});
