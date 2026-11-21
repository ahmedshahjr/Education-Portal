import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';
import { API_BASE_URL } from '../../web-api-client';
import { CoursesPanelComponent } from './courses-panel.component';
import { EnrolmentsPanelComponent } from './enrolments-panel.component';
import { StudentAdmissionPage } from './student-admission.page';

const blob = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

const OFFERING = '0190f3a0-0000-7000-8000-000000000001';
const TEACHER = '0190f3a0-0000-7000-8000-000000000002';
const STUDENT = '0190f3a0-0000-7000-8000-000000000003';

const offerings = [{ id: OFFERING, level: 'Class1', academicYear: '2026-27', campusId: null }];
const people = (type: string, id: string) => [{ id, firstName: 'Sara', lastName: 'Ali', userType: type }];

interface FormHost {
  form: { setValue(value: unknown): void; patchValue(value: unknown): void };
}

/** Waits until the page has handled the server's answer, so nothing runs after the test has ended. */
async function settled(fixture: { componentInstance: unknown }): Promise<void> {
  await vi.waitFor(() => expect((fixture.componentInstance as { busy(): boolean }).busy()).toBe(false));
}

function configure() {
  TestBed.configureTestingModule({
    providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_BASE_URL, useValue: '' }],
  });
}

describe('CoursesPanelComponent', () => {
  async function render() {
    configure();
    const fixture = TestBed.createComponent(CoursesPanelComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne((r) => r.url.startsWith('/api/ClassOfferings')).flush(blob(offerings));
    http.expectOne((r) => r.url.startsWith('/api/UserDetails')).flush(blob(people('Teacher', TEACHER)));
    http.expectOne((r) => r.url.startsWith('/api/Courses') && r.method === 'GET').flush(blob([{ id: 'c1', name: 'Math', classOfferingId: OFFERING, teacherId: TEACHER }]));
    const element = fixture.nativeElement as HTMLElement;
    await vi.waitFor(async () => {
      await fixture.whenStable();
      fixture.detectChanges();
      expect(element.textContent).toContain('Math');
    });
    return { fixture, http, element };
  }

  it('shows each course with its class and teacher', async () => {
    const { element } = await render();

    expect(element.textContent).toContain('Class 1 · 2026-27');
    expect(element.textContent).toContain('Sara Ali');
  });

  it('asks the server for teachers only', async () => {
    configure();
    const fixture = TestBed.createComponent(CoursesPanelComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();

    const request = http.expectOne((r) => r.url.startsWith('/api/UserDetails'));

    expect(request.request.urlWithParams).toContain('userType=Teacher');
  });

  it('adds a course with its class and teacher', async () => {
    const { fixture, http, element } = await render();
    [...element.querySelectorAll('button')].find((b) => b.textContent?.includes('Add course'))!.click();
    fixture.detectChanges();
    (fixture.componentInstance as unknown as FormHost).form.setValue({ classOfferingId: OFFERING, name: 'Science', teacherId: TEACHER });
    fixture.detectChanges();

    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();

    const request = http.expectOne((r) => r.url.startsWith('/api/Courses') && r.method === 'POST');
    expect(JSON.parse(request.request.body as string)).toMatchObject({ classOfferingId: OFFERING, name: 'Science', teacherId: TEACHER });
    request.flush(blob('c2'), { status: 200, statusText: 'OK' });
    await settled(fixture);
  });

  it('sends nothing without a name', async () => {
    const { fixture, http, element } = await render();
    [...element.querySelectorAll('button')].find((b) => b.textContent?.includes('Add course'))!.click();
    fixture.detectChanges();

    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();

    http.expectNone((r) => r.method === 'POST');
  });
});

describe('EnrolmentsPanelComponent', () => {
  async function render() {
    configure();
    const fixture = TestBed.createComponent(EnrolmentsPanelComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne((r) => r.url.startsWith('/api/ClassOfferings')).flush(blob(offerings));
    const students = http.expectOne((r) => r.url.startsWith('/api/UserDetails'));
    expect(students.request.urlWithParams).toContain('userType=Student');
    students.flush(blob(people('Student', STUDENT)));
    await fixture.whenStable();
    fixture.detectChanges();
    return { fixture, http, element: fixture.nativeElement as HTMLElement };
  }

  it('sends nothing until a student and a class are chosen', async () => {
    const { http, element } = await render();

    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();

    http.expectNone((r) => r.method === 'POST');
  });

  it('enrols the student in the class offering', async () => {
    const { fixture, http, element } = await render();
    (fixture.componentInstance as unknown as FormHost).form.setValue({ studentId: STUDENT, classOfferingId: OFFERING });
    fixture.detectChanges();

    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();

    const request = http.expectOne((r) => r.url.startsWith('/api/Enrolments') && r.method === 'POST');
    expect(JSON.parse(request.request.body as string)).toEqual({ studentId: STUDENT, classOfferingId: OFFERING });
    request.flush(blob('e1'), { status: 200, statusText: 'OK' });
    await settled(fixture);
  });
});

describe('StudentAdmissionPage', () => {
  function render() {
    configure();
    const fixture = TestBed.createComponent(StudentAdmissionPage);
    fixture.detectChanges();
    return { fixture, http: TestBed.inject(HttpTestingController), element: fixture.nativeElement as HTMLElement };
  }

  it('sends nothing until the required fields are filled in', () => {
    const { fixture, http, element } = render();

    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
    fixture.detectChanges();

    http.expectNone((r) => r.method === 'POST');
  });

  it('admits the student with date-only dates', async () => {
    const { fixture, http, element } = render();
    (fixture.componentInstance as unknown as FormHost).form.patchValue({
      firstName: 'Omar',
      lastName: 'Khan',
      gender: 'Male',
      dateOfBirth: new Date(2015, 4, 17),
      fatherName: 'Imran Khan',
      guardianPhone: '+923001234567',
      nationality: 'Pakistani',
      address: '12 Main Road',
      city: 'Lahore',
      classApplied: 'Class 1',
      admissionDate: new Date(2026, 7, 1),
    });
    fixture.detectChanges();

    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();

    const request = http.expectOne((r) => r.url.startsWith('/api/StudentAdmissions') && r.method === 'POST');
    expect(JSON.parse(request.request.body as string)).toMatchObject({
      firstName: 'Omar',
      lastName: 'Khan',
      gender: 'Male',
      dateOfBirth: '2015-05-17',
      admissionDate: '2026-08-01',
      guardianPhone: '+923001234567',
    });
    request.flush(blob('s1'), { status: 200, statusText: 'OK' });
    await settled(fixture);
  });
});
