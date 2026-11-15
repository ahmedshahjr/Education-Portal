import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting, TestRequest } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { PricingPage } from './pricing.page';

const json = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

const plan = { id: 'p1', code: 'STANDARD', name: 'Standard', description: '', isActive: true, modules: ['EXAMS'] };
const modules = [
  { code: 'STUDENTS', name: 'Students', description: '', isCore: true, isActive: true, isDefaultEnabled: true, sortOrder: 1 },
  { code: 'EXAMS', name: 'Exams', description: '', isCore: false, isActive: true, isDefaultEnabled: false, sortOrder: 2 },
];

/** Answers the page's start-up requests. */
function answerStartUp(http: HttpTestingController) {
  http.match(() => true).forEach((request: TestRequest) => {
    const url = request.request.url;
    if (url.startsWith('/api/Plans')) {
      request.flush(json([plan]));
    } else if (url.startsWith('/api/PriceBooks')) {
      request.flush(json([]));
    } else if (url.startsWith('/api/Modules')) {
      request.flush(json(modules));
    } else {
      request.flush(json({ items: [], totalCount: 0 }));
    }
  });
}

async function render() {
  TestBed.configureTestingModule({
    imports: [PricingPage],
    providers: [provideHttpClient(), provideHttpClientTesting()],
  });
  const http = TestBed.inject(HttpTestingController);
  const fixture = TestBed.createComponent(PricingPage);
  fixture.detectChanges();
  answerStartUp(http);
  const element = fixture.nativeElement as HTMLElement;
  await vi.waitFor(() => {
    fixture.detectChanges();
    expect(element.textContent).toContain('Standard');
  });
  return { fixture, http, element };
}

/** Types into one field of the form it sits in and tells Angular about it. */
function type(input: HTMLInputElement, value: string) {
  input.value = value;
  input.dispatchEvent(new Event('input'));
}

describe('PricingPage', () => {
  beforeEach(() => TestBed.resetTestingModule());
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('shows a failed plan save next to the plan form', async () => {
    const { fixture, http, element } = await render();
    const planCard = element.querySelectorAll('mat-card')[0] as HTMLElement;
    const planForm = planCard.querySelector('form') as HTMLFormElement;

    type(planForm.querySelector<HTMLInputElement>('input[formControlName="code"]')!, 'PREMIUM');
    type(planForm.querySelector<HTMLInputElement>('input[formControlName="name"]')!, 'Premium');
    fixture.detectChanges();
    planForm.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();

    const request = http.expectOne({ method: 'POST', url: '/api/Plans' });
    request.flush(new Blob(['']), { status: 403, statusText: 'Forbidden' });

    await vi.waitFor(() => {
      fixture.detectChanges();
      expect(planCard.querySelector('[role="alert"]')?.textContent).toContain('do not have permission');
    });
  });
});
