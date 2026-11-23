import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';
import { API_BASE_URL, ClassLevel } from '../../web-api-client';
import { ClassOfferingsPage } from './class-offerings.page';
import { CLASS_LEVELS, classLevelLabel } from '../school-page/school-page.labels';

const blob = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

const CAMPUS = '0190f3a0-0000-7000-8000-000000000003';

async function render(offerings: unknown[] = []) {
  TestBed.configureTestingModule({
    providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_BASE_URL, useValue: '' }],
  });
  const fixture = TestBed.createComponent(ClassOfferingsPage);
  const http = TestBed.inject(HttpTestingController);
  fixture.detectChanges();
  http.expectOne('/api/Campuses').flush(blob([{ id: CAMPUS, name: 'Main campus' }]));
  await vi.waitFor(() => http.expectOne((r) => r.url.startsWith('/api/ClassOfferings')).flush(blob(offerings)));
  const element = fixture.nativeElement as HTMLElement;
  await fixture.whenStable();
  fixture.detectChanges();
  return { fixture, http, element };
}

function fill(element: HTMLElement, name: string, value: string) {
  const input = element.querySelector<HTMLInputElement>(`input[formcontrolname="${name}"]`)!;
  input.value = value;
  input.dispatchEvent(new Event('input'));
}

async function openForm(element: HTMLElement, fixture: { detectChanges(): void }) {
  [...element.querySelectorAll('button')].find((b) => b.textContent?.includes('Add class offering'))!.click();
  fixture.detectChanges();
}

describe('ClassOfferingsPage form', () => {
  it('sends nothing for an invalid academic year or currency', async () => {
    const { fixture, http, element } = await render();
    await openForm(element, fixture);
    fill(element, 'academicYear', '26');
    fill(element, 'currencyCode', 'RS');
    fixture.detectChanges();

    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();

    http.expectNone((r) => r.method === 'POST');
  });

  it('refuses more seats left than seats in total', async () => {
    const { fixture, http, element } = await render();
    await openForm(element, fixture);
    fill(element, 'academicYear', '2026-27');
    fill(element, 'currencyCode', 'PKR');
    fill(element, 'seatsTotal', '10');
    fill(element, 'seatsAvailable', '11');
    fixture.detectChanges();

    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
    fixture.detectChanges();

    http.expectNone((r) => r.method === 'POST');
    expect(element.textContent).toContain('The seats left cannot be more than the seats in total.');
  });

  it('adds an offering with the currency in capitals', async () => {
    const { fixture, http, element } = await render();
    await openForm(element, fixture);
    (fixture.componentInstance as unknown as { form: { controls: { campusId: { setValue(value: string): void } } } }).form.controls.campusId.setValue(CAMPUS);
    fill(element, 'academicYear', '2026-27');
    fill(element, 'currencyCode', 'pkr');
    fill(element, 'monthlyFee', '5000');
    fill(element, 'seatsTotal', '30');
    fill(element, 'seatsAvailable', '30');
    fixture.detectChanges();

    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();

    const request = http.expectOne((r) => r.url.startsWith('/api/ClassOfferings') && r.method === 'POST');
    expect(JSON.parse(request.request.body as string)).toMatchObject({
      academicYear: '2026-27',
      currencyCode: 'PKR',
      monthlyFee: 5000,
      seatsTotal: 30,
      seatsAvailable: 30,
      level: 'Class1',
      status: 'Open',
      showFee: true,
    });
    request.flush(blob('0190f3a0-0000-7000-8000-0000000000aa'), { status: 201, statusText: 'Created' });
  });

  it('shows every offering for all campuses, and only a campus with its school-wide offerings when one is chosen', async () => {
    const row = (id: string, campusId: string | null) => ({
      id,
      campusId,
      level: ClassLevel.Class1,
      academicYear: '2026-27',
      monthlyFee: 1000,
      currencyCode: 'PKR',
      seatsTotal: 30,
      seatsAvailable: 10,
    });
    const { fixture } = await render([row('school', null), row('main', CAMPUS), row('other', '0190f3a0-0000-7000-8000-000000000009')]);
    const page = fixture.componentInstance as unknown as { visibleOfferings(): { id: string }[]; selectedCampusId: { set(id: string | null): void } };

    // The generated client reads the response blob asynchronously, so the rows arrive after a tick.
    await vi.waitFor(() => expect(page.visibleOfferings().map((o) => o.id)).toEqual(['school', 'main', 'other']));

    page.selectedCampusId.set(CAMPUS);
    fixture.detectChanges();
    expect(page.visibleOfferings().map((o) => o.id)).toEqual(['school', 'main']);

    page.selectedCampusId.set('0190f3a0-0000-7000-8000-00000000000e');
    expect(page.visibleOfferings().map((o) => o.id)).toEqual(['school']);
  });
  it('offers each class level with its own label', async () => {
    const { fixture } = await render();
    const page = fixture.componentInstance as unknown as { levels: readonly string[] };
    const labels = page.levels.map((level) => classLevelLabel(level as never));

    expect(page.levels).toEqual(CLASS_LEVELS);
    expect(labels.every((text) => text.length > 0)).toBe(true);
    expect(new Set(labels).size).toBe(CLASS_LEVELS.length);
  });
});
