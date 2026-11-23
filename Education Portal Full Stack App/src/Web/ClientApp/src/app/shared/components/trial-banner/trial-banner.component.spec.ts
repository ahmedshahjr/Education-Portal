import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';
import { API_BASE_URL } from '../../../web-api-client';
import { TrialBannerComponent } from './trial-banner.component';

// The generated client reads each response body as a Blob, which completes over several macrotasks.
const settle = async () => {
  for (let i = 0; i < 5; i++) {
    await new Promise((resolve) => setTimeout(resolve));
  }
};

const json = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

function setup(): { fixture: ComponentFixture<TrialBannerComponent>; http: HttpTestingController } {
  TestBed.configureTestingModule({
    providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_BASE_URL, useValue: '' }],
  });
  const fixture = TestBed.createComponent(TrialBannerComponent);
  return { fixture, http: TestBed.inject(HttpTestingController) };
}

// The generated client reads each response body as a Blob, which completes on a later macrotask.
async function answer(fixture: ComponentFixture<TrialBannerComponent>, http: HttpTestingController, body: unknown, status = 200) {
  fixture.detectChanges();
  http.expectOne('/api/Subscriptions/mine').flush(json(body), { status, statusText: status === 200 ? 'OK' : 'Error' });
  await settle();
  fixture.detectChanges();
  return fixture.nativeElement as HTMLElement;
}

describe('TrialBannerComponent (integration)', () => {
  it('shows the days remaining of a school in its trial', async () => {
    const { fixture, http } = setup();

    const element = await answer(fixture, http, { trialDaysRemaining: 7 });

    expect(element.querySelector('[role="status"]')?.textContent).toContain('7 days remaining');
  });

  it('says the trial ends today on the last day', async () => {
    const { fixture, http } = setup();

    const element = await answer(fixture, http, { trialDaysRemaining: 0 });

    expect(element.querySelector('[role="status"]')?.textContent).toContain('ends today');
  });

  it('shows nothing when the school is not on a trial', async () => {
    const { fixture, http } = setup();

    const element = await answer(fixture, http, { trialDaysRemaining: null });

    expect(element.querySelector('[role="status"]')).toBeNull();
  });

  it('shows nothing when the subscription cannot be read, as for the master tenant', async () => {
    const { fixture, http } = setup();

    const element = await answer(fixture, http, {}, 404);

    expect(element.querySelector('[role="status"]')).toBeNull();
  });

  it('shows the days passed in by the sign-in page and does not call the API', () => {
    const { fixture, http } = setup();
    fixture.componentRef.setInput('daysRemaining', 3);
    fixture.detectChanges();

    http.verify();
    expect((fixture.nativeElement as HTMLElement).querySelector('[role="status"]')?.textContent).toContain('3 days remaining');
  });
});
