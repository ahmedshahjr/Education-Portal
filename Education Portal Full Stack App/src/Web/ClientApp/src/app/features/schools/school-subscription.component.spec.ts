import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { of } from 'rxjs';
import { SchoolSummaryDto } from '../../web-api-client';
import { SchoolSubscriptionComponent } from './school-subscription.component';

const json = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });
const SCHOOL = '0190f3a0-0000-7000-8000-000000000005';
const url = (path: string) => `/api/Schools/${SCHOOL}/${path}`;

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
  modules: [],
};
const plans = [{ id: 'p1', code: 'BASIC', name: 'Basic', description: '', isActive: true, modules: ['EXAMS'] }];

const openDialog = vi.fn();

async function render(subscriptionResponse: { body?: unknown; status?: number }) {
  TestBed.configureTestingModule({
    imports: [SchoolSubscriptionComponent],
    providers: [provideHttpClient(), provideHttpClientTesting(), { provide: MatDialog, useValue: { open: openDialog } }],
  });
  const http = TestBed.inject(HttpTestingController);
  const fixture = TestBed.createComponent(SchoolSubscriptionComponent);
  fixture.componentRef.setInput('school', new SchoolSummaryDto({ id: SCHOOL, name: 'School One', domain: 'school1.com', isMaster: false, countryId: undefined }));
  fixture.detectChanges();
  http.expectOne('/api/Plans').flush(json(plans));
  const response = http.expectOne(url('subscription'));
  if (subscriptionResponse.status === 404) {
    response.flush(json({}), { status: 404, statusText: 'Not Found' });
  } else {
    response.flush(json(subscriptionResponse.body));
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

/** After a change the component reloads the subscription from the server. */
async function reloadAfterChange(http: HttpTestingController, body: unknown) {
  await vi.waitFor(() => http.expectOne(url('subscription')).flush(json(body)));
}

describe('SchoolSubscriptionComponent', () => {
  beforeEach(() => openDialog.mockReset());
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('offers to start a subscription when the school has none', async () => {
    const { element } = await render({ status: 404 });

    expect(element.querySelector('form')).not.toBeNull();
    expect(button(element, 'Start subscription').disabled).toBe(true); // a plan must be chosen first
  });

  it('shows the plan switch on while the plan runs', async () => {
    const { element } = await render({ body: subscription });

    expect(element.querySelector('[role="switch"]')!.getAttribute('aria-checked')).toBe('true');
  });

  it('suspends the plan only after the master confirms', async () => {
    openDialog.mockReturnValue({ afterClosed: () => of(true) });
    const { http, element } = await render({ body: subscription });

    element.querySelector<HTMLButtonElement>('[role="switch"]')!.click();

    expect(openDialog).toHaveBeenCalledOnce();
    const request = http.expectOne(url('subscription/suspend'));
    expect(request.request.method).toBe('POST');
    request.flush(null, { status: 204, statusText: 'No Content' });
    await reloadAfterChange(http, { ...subscription, status: 'Suspended' });
  });

  it('keeps the plan on when the master declines the confirm step', async () => {
    openDialog.mockReturnValue({ afterClosed: () => of(false) });
    const { http, element } = await render({ body: subscription });

    const toggle = element.querySelector<HTMLButtonElement>('[role="switch"]')!;
    toggle.click();

    http.expectNone(url('subscription/suspend'));
    expect(toggle.getAttribute('aria-checked')).toBe('true');
  });

  it('reactivates a suspended plan when the switch is turned on', async () => {
    const { http, element } = await render({ body: { ...subscription, status: 'Suspended' } });

    element.querySelector<HTMLButtonElement>('[role="switch"]')!.click();

    const request = http.expectOne(url('subscription/reactivate'));
    expect(request.request.method).toBe('POST');
    request.flush(null, { status: 204, statusText: 'No Content' });
    await reloadAfterChange(http, subscription);
  });

  it('extends the period and reloads it', async () => {
    const { fixture, http, element } = await render({ body: subscription });
    const days = element.querySelector<HTMLInputElement>('input[formcontrolname="days"]')!;
    days.value = '10';
    days.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    button(element, 'Extend').click();

    const request = http.expectOne(url('subscription/extend'));
    expect(JSON.parse(request.request.body as string)).toMatchObject({ days: 10 });
    request.flush(null, { status: 204, statusText: 'No Content' });
    await reloadAfterChange(http, subscription);
  });
});
