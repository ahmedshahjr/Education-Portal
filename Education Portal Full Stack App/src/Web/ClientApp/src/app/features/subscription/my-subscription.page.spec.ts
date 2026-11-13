import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { MySubscriptionPage } from './my-subscription.page';

const json = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

const subscription = {
  id: '0190f3a0-0000-7000-8000-000000000009',
  planCode: 'BASIC',
  countryCode: 'PK',
  currencyCode: 'PKR',
  billingCycle: 'Monthly',
  amount: 25000,
  status: 'Active',
  startDate: '2026-11-01T00:00:00Z',
  endDate: '2026-12-01T00:00:00Z',
  modules: [{ code: 'STUDENTS', status: 'Active', validFrom: '2026-11-01T00:00:00Z', validTo: '2026-12-01T00:00:00Z' }],
};

async function render(body: unknown) {
  TestBed.configureTestingModule({ imports: [MySubscriptionPage], providers: [provideHttpClient(), provideHttpClientTesting()] });
  const http = TestBed.inject(HttpTestingController);
  const fixture = TestBed.createComponent(MySubscriptionPage);
  fixture.detectChanges();
  http.expectOne('/api/Subscriptions/mine').flush(json(body));
  const element = fixture.nativeElement as HTMLElement;
  await vi.waitFor(() => {
    fixture.detectChanges();
    expect(element.querySelector('app-loading-state')).toBeNull();
  });
  return { fixture, http, element };
}

describe('MySubscriptionPage', () => {
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('shows the plan, the price and the add-ons that can be asked for', async () => {
    const { element } = await render({
      subscription,
      availableAddOns: [
        { code: 'LIBRARY', name: 'Library', description: 'Loans', price: 500, requested: false },
        { code: 'HOSTEL', name: 'Hostel', description: 'Rooms', price: 900, requested: true },
      ],
      requests: [],
    });

    expect(element.textContent).toContain('BASIC');
    expect(element.textContent).toContain('25,000.00 PKR');
    const buttons = element.querySelectorAll('button');
    expect(buttons.length).toBe(1); // Hostel is already requested, so only Library can be asked for.
    expect(element.textContent).toContain('Requested');
  });

  it('asks for a module', async () => {
    const { http, element } = await render({
      subscription,
      availableAddOns: [{ code: 'LIBRARY', name: 'Library', description: 'Loans', price: 500, requested: false }],
      requests: [],
    });

    element.querySelector('button')!.click();

    const request = http.expectOne('/api/AddOnRequests');
    expect(request.request.method).toBe('POST');
    expect(JSON.parse(request.request.body as string)).toEqual({ moduleCode: 'LIBRARY' });
    request.flush(json('0190f3a0-0000-7000-8000-000000000011'));
    await vi.waitFor(() => http.expectOne('/api/Subscriptions/mine').flush(json({ subscription, availableAddOns: [], requests: [] })));
  });

  it('says so when the school has no subscription', async () => {
    const { element } = await render({ availableAddOns: [], requests: [] });

    expect(element.querySelector('app-empty-state')).not.toBeNull();
  });
});
