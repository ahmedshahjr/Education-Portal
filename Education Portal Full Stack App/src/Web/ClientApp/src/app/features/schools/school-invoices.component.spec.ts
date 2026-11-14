import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { SchoolSummaryDto } from '../../web-api-client';
import { SchoolInvoicesComponent } from './school-invoices.component';

const json = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });
const SCHOOL = '0190f3a0-0000-7000-8000-000000000005';

const openInvoice = {
  id: '0190f3a0-0000-7000-8000-000000000021',
  schoolId: SCHOOL,
  number: 'INV-000001',
  status: 'Open',
  currencyCode: 'PKR',
  subtotal: 25000,
  taxRate: 15,
  taxAmount: 3750,
  total: 28750,
  issueDate: '2026-11-01T00:00:00Z',
  dueDate: '2026-12-01T00:00:00Z',
  periodStart: '2026-11-01T00:00:00Z',
  periodEnd: '2026-12-01T00:00:00Z',
  paidDate: undefined,
  overdueSince: undefined,
  lines: [{ description: 'BASIC plan', amount: 25000 }],
  payments: [
    { id: '0190f3a0-0000-7000-8000-000000000031', amount: 8750, currencyCode: 'PKR', method: 'Cash', reference: 'R1', receivedDate: '2026-11-05T00:00:00Z', provider: 'Manual' },
  ],
};

async function render(invoices: unknown[]) {
  TestBed.configureTestingModule({ imports: [SchoolInvoicesComponent], providers: [provideHttpClient(), provideHttpClientTesting()] });
  const http = TestBed.inject(HttpTestingController);
  const fixture = TestBed.createComponent(SchoolInvoicesComponent);
  fixture.componentRef.setInput('school', new SchoolSummaryDto({ id: SCHOOL, name: 'School One', domain: 'school1.com', isMaster: false, countryId: undefined }));
  fixture.detectChanges();
  http.expectOne(`/api/Invoices?schoolId=${SCHOOL}`).flush(json(invoices));
  const element = fixture.nativeElement as HTMLElement;
  // The generated client reads every response as a Blob, which is read asynchronously.
  await vi.waitFor(() => {
    fixture.detectChanges();
    expect(element.querySelector('app-loading-state')).toBeNull();
  });
  return { fixture, http, element };
}

const button = (element: HTMLElement, text: string) =>
  [...element.querySelectorAll<HTMLButtonElement>('button')].find((b) => b.textContent?.trim().startsWith(text))!;

describe('SchoolInvoicesComponent', () => {
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('shows the balance due as the total less the payments so far', async () => {
    const { element } = await render([openInvoice]);

    const figures = element.querySelector('.mm-invoices__figures')!.textContent ?? '';
    expect(figures).toContain('28,750.00'); // total
    expect(figures).toContain('20,000.00'); // 28,750 less the 8,750 paid
  });

  it('shows the whole total as the balance when nothing has been paid', async () => {
    const { element } = await render([{ ...openInvoice, payments: [] }]);

    expect(element.querySelector('.mm-invoices__figures')!.textContent).toContain('28,750.00');
  });

  it('shows the overdue badge and date when the invoice is overdue', async () => {
    const { fixture, element } = await render([{ ...openInvoice, overdueSince: '2026-12-02T00:00:00Z' }]);
    button(element, 'Details').click();
    fixture.detectChanges();

    expect(element.textContent).toContain('Overdue');
    expect(element.textContent).toContain('Overdue since');
  });

  it('shows an empty message when there are no invoices', async () => {
    const { element } = await render([]);

    expect(element.textContent).toContain('No invoices yet');
  });

  it('records a payment for the whole total of an open invoice', async () => {
    const { fixture, http, element } = await render([{ ...openInvoice, payments: [] }]);
    button(element, 'Record payment').click();
    fixture.detectChanges();
    const reference = element.querySelector<HTMLInputElement>('input[formcontrolname="reference"]')!;
    reference.value = 'TRX-1';
    reference.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    button(element, 'Record payment of').click();

    const request = http.expectOne(`/api/Invoices/${openInvoice.id}/payments`);
    expect(request.request.method).toBe('POST');
    expect(JSON.parse(request.request.body as string)).toMatchObject({ invoiceId: openInvoice.id, amount: 28750, method: 'BankTransfer', reference: 'TRX-1' });
    request.flush(json('0190f3a0-0000-7000-8000-000000000031'));
    await vi.waitFor(() => http.expectOne(`/api/Invoices?schoolId=${SCHOOL}`).flush(json([])));
  });

  it('voids an open invoice and reloads the list', async () => {
    const { http, element } = await render([{ ...openInvoice, payments: [] }]);

    button(element, 'Void').click();

    const request = http.expectOne(`/api/Invoices/${openInvoice.id}/void`);
    expect(request.request.method).toBe('POST');
    request.flush(null, { status: 204, statusText: 'No Content' });
    await vi.waitFor(() => http.expectOne(`/api/Invoices?schoolId=${SCHOOL}`).flush(json([])));
  });
});
