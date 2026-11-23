import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';
import { API_BASE_URL } from '../../web-api-client';
import { AssessmentsPanelComponent } from './assessments-panel.component';
import { GradingScalesPanelComponent } from './grading-scales-panel.component';
import { ReportCardPanelComponent } from './report-card-panel.component';

const blob = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

const COURSE = '0190f3a0-0000-7000-8000-000000000001';
const STUDENT = '0190f3a0-0000-7000-8000-000000000002';
const ASSESSMENT = '0190f3a0-0000-7000-8000-000000000003';
const OFFERING = '0190f3a0-0000-7000-8000-000000000004';
const SCALE = '0190f3a0-0000-7000-8000-000000000005';

function configure() {
  TestBed.configureTestingModule({
    providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_BASE_URL, useValue: '' }],
  });
}

async function settled(fixture: { componentInstance: unknown }): Promise<void> {
  await vi.waitFor(() => expect((fixture.componentInstance as { busy(): boolean }).busy()).toBe(false));
}

describe('GradingScalesPanelComponent', () => {
  async function render() {
    configure();
    const fixture = TestBed.createComponent(GradingScalesPanelComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne('/api/GradingScales').flush(
      blob([{ id: SCALE, name: 'Standard', bands: [{ minPercent: 0, maxPercent: 49.99, letter: 'F', points: 0 }, { minPercent: 50, maxPercent: 100, letter: 'A', points: 4 }] }])
    );
    const element = fixture.nativeElement as HTMLElement;
    await vi.waitFor(async () => {
      await fixture.whenStable();
      fixture.detectChanges();
      expect(element.textContent).toContain('Standard');
    });
    return { fixture, http, element };
  }

  function fill(element: HTMLElement, selector: string, value: string) {
    const input = element.querySelector<HTMLInputElement>(selector)!;
    input.value = value;
    input.dispatchEvent(new Event('input'));
  }

  it('shows the bands of a scale from the highest percent down', async () => {
    const { element } = await render();

    expect(element.textContent).toContain('A 50-100 (4)  F 0-49.99 (0)');
  });

  it('refuses two bands that share a percent', async () => {
    const { fixture, http, element } = await render();
    [...element.querySelectorAll('button')].find((b) => b.textContent?.includes('Add grading scale'))!.click();
    fixture.detectChanges();
    fill(element, 'input[formcontrolname="name"]', 'Broken');
    fill(element, 'input[formcontrolname="letter"]', 'A');
    [...element.querySelectorAll('button')].find((b) => b.textContent?.includes('Add band'))!.click();
    fixture.detectChanges();
    fixture.detectChanges();
    const letters = element.querySelectorAll<HTMLInputElement>('input[formcontrolname="letter"]');
    letters[1].value = 'B';
    letters[1].dispatchEvent(new Event('input'));
    fixture.detectChanges();

    // Both bands cover 0-100, so they overlap.
    expect(element.textContent).toContain('bands cannot share a percent');
    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
    http.expectNone((r) => r.method === 'POST');
  });

  it('saves a scale with one band', async () => {
    const { fixture, http, element } = await render();
    [...element.querySelectorAll('button')].find((b) => b.textContent?.includes('Add grading scale'))!.click();
    fixture.detectChanges();
    fill(element, 'input[formcontrolname="name"]', 'Simple');
    fill(element, 'input[formcontrolname="letter"]', 'P');
    fixture.detectChanges();

    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();

    const request = http.expectOne((r) => r.url === '/api/GradingScales' && r.method === 'POST');
    expect(JSON.parse(request.request.body as string)).toMatchObject({ name: 'Simple', bands: [{ minPercent: 0, maxPercent: 100, letter: 'P', points: 0 }] });
    request.flush(blob('s2'), { status: 200, statusText: 'OK' });
    await settled(fixture);
  });
});

describe('AssessmentsPanelComponent', () => {
  async function render() {
    configure();
    const fixture = TestBed.createComponent(AssessmentsPanelComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne((r) => r.url.startsWith('/api/Courses')).flush(blob([{ id: COURSE, name: 'Math', classOfferingId: OFFERING, teacherId: 't' }]));
    http.expectOne((r) => r.url.startsWith('/api/UserDetails')).flush(blob([{ id: STUDENT, firstName: 'Omar', lastName: 'Khan', userType: 'Student' }]));
    await fixture.whenStable();
    fixture.detectChanges();
    return { fixture, http, element: fixture.nativeElement as HTMLElement };
  }

  async function chooseCourse(fixture: { componentInstance: unknown; detectChanges(): void; whenStable(): Promise<unknown> }, http: HttpTestingController) {
    (fixture.componentInstance as { chooseCourse(id: string): void }).chooseCourse(COURSE);
    const request = http.expectOne((r) => r.url.startsWith('/api/Assessments') && r.method === 'GET');
    expect(request.request.urlWithParams).toContain(`courseId=${COURSE}`);
    request.flush(blob([{ id: ASSESSMENT, name: 'Midterm', weight: 40, maximumScore: 50 }]));
    await vi.waitFor(async () => {
      await fixture.whenStable();
      fixture.detectChanges();
      expect((fixture as unknown as { nativeElement: HTMLElement }).nativeElement.textContent).toContain('Midterm');
    });
  }

  it('lists the assessments of the chosen course', async () => {
    const { fixture, http, element } = await render();

    await chooseCourse(fixture, http);

    expect(element.textContent).toContain('40%');
  });

  it('adds an assessment to the course', async () => {
    const { fixture, http, element } = await render();
    await chooseCourse(fixture, http);
    [...element.querySelectorAll('button')].find((b) => b.textContent?.includes('Add assessment'))!.click();
    fixture.detectChanges();
    const form = (fixture.componentInstance as unknown as { assessmentForm: { setValue(v: unknown): void } }).assessmentForm;
    form.setValue({ name: 'Final', weight: 60, maximumScore: 100 });
    fixture.detectChanges();

    element.querySelector<HTMLButtonElement>('form button[type="submit"]')!.click();

    const request = http.expectOne((r) => r.url === '/api/Assessments' && r.method === 'POST');
    expect(JSON.parse(request.request.body as string)).toEqual({ courseId: COURSE, name: 'Final', weight: 60, maximumScore: 100 });
    request.flush(blob('a2'), { status: 200, statusText: 'OK' });
    await settled(fixture);
    http.expectOne((r) => r.url.startsWith('/api/Assessments') && r.method === 'GET').flush(blob([]));
  });

  it('records a score for a student and shows what it is worth in percent', async () => {
    const { fixture, http, element } = await render();
    await chooseCourse(fixture, http);
    const scoreForm = (fixture.componentInstance as unknown as { scoreForm: { setValue(v: unknown): void } }).scoreForm;
    scoreForm.setValue({ assessmentId: ASSESSMENT, studentId: STUDENT, value: 25 });
    fixture.detectChanges();

    expect(element.textContent).toContain('50%');
    element.querySelectorAll<HTMLButtonElement>('form button[type="submit"]')[0].click();

    const request = http.expectOne((r) => r.url.startsWith(`/api/Scores`) && r.method === 'PUT');
    expect(request.request.url).toContain(ASSESSMENT);
    expect(request.request.url).toContain(STUDENT);
    expect(JSON.parse(request.request.body as string)).toEqual({ value: 25 });
    request.flush(null, { status: 204, statusText: 'No Content' });
    await settled(fixture);
  });

  it('shows a score as a percent of the maximum, the full maximum as 100%', async () => {
    const { fixture, http, element } = await render();
    await chooseCourse(fixture, http);
    const scoreForm = (fixture.componentInstance as unknown as { scoreForm: { setValue(v: unknown): void } }).scoreForm;

    scoreForm.setValue({ assessmentId: ASSESSMENT, studentId: STUDENT, value: 1 });
    fixture.detectChanges();
    expect(element.textContent).toContain('2%');

    scoreForm.setValue({ assessmentId: ASSESSMENT, studentId: STUDENT, value: 50 });
    fixture.detectChanges();
    expect(element.textContent).toContain('100%');
  });

  it('refuses a negative score', async () => {
    const { fixture, http, element } = await render();
    await chooseCourse(fixture, http);
    (fixture.componentInstance as unknown as { scoreForm: { setValue(v: unknown): void } }).scoreForm.setValue({ assessmentId: ASSESSMENT, studentId: STUDENT, value: -1 });
    fixture.detectChanges();

    element.querySelectorAll<HTMLButtonElement>('form button[type="submit"]')[0].click();

    http.expectNone((r) => r.url.startsWith('/api/Scores'));
  });
});

describe('ReportCardPanelComponent', () => {
  it('shows the lines and the grade point average', async () => {
    configure();
    const fixture = TestBed.createComponent(ReportCardPanelComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne((r) => r.url.startsWith('/api/UserDetails')).flush(blob([{ id: STUDENT, firstName: 'Omar', lastName: 'Khan', userType: 'Student' }]));
    http.expectOne((r) => r.url.startsWith('/api/ClassOfferings')).flush(blob([{ id: OFFERING, level: 'Class1', academicYear: '2026-27' }]));
    http.expectOne('/api/GradingScales').flush(blob([{ id: SCALE, name: 'Standard', bands: [] }]));
    await fixture.whenStable();
    (fixture.componentInstance as unknown as { form: { setValue(v: unknown): void } }).form.setValue({ studentId: STUDENT, classOfferingId: OFFERING, gradingScaleId: SCALE });
    fixture.detectChanges();
    const element = fixture.nativeElement as HTMLElement;

    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();

    const request = http.expectOne((r) => r.url.startsWith('/api/ReportCards'));
    expect(request.request.url).toContain(STUDENT);
    request.flush(blob({ lines: [{ courseName: 'Math', percent: 75, letter: 'A', points: 4 }, { courseName: 'Art' }], gpa: 4 }));

    await vi.waitFor(async () => {
      await fixture.whenStable();
      fixture.detectChanges();
      expect(element.textContent).toContain('Math');
    });
    expect(element.textContent).toContain('Grade point average:');
    expect(element.textContent).toContain('Art');
    await settled(fixture);
  });
});
