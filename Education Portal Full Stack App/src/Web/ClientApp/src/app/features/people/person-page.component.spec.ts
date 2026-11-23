import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting, TestRequest } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';
import { describe, expect, it, vi } from 'vitest';
import { API_BASE_URL, UserType } from '../../web-api-client';
import { PersonPageComponent } from './person-page.component';

const json = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });
const NEW_PERSON = '0190f3a0-0000-7000-8000-000000000022';

const tick = () => new Promise((resolve) => setTimeout(resolve, 0));

/**
 * Answers every request the page makes while it opens with an empty school (no students, roles or campuses). The page
 * makes its next requests only after the first ones answer, so this loops until nothing is waiting.
 */
async function answerStartUp(http: HttpTestingController): Promise<void> {
  for (let round = 0; round < 10; round++) {
    await tick();
    const pending: TestRequest[] = http.match(() => true);
    if (pending.length === 0) {
      return;
    }
    for (const request of pending) {
      if (request.request.url.includes('/built-in')) {
        request.flush(null, { status: 204, statusText: 'No Content' });
      } else {
        request.flush(json([]));
      }
    }
  }
}

async function render() {
  TestBed.configureTestingModule({
    imports: [PersonPageComponent],
    providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([]), { provide: API_BASE_URL, useValue: '' }],
  });
  const fixture = TestBed.createComponent(PersonPageComponent);
  const http = TestBed.inject(HttpTestingController);
  fixture.detectChanges();
  await answerStartUp(http);
  await vi.waitFor(() => {
    fixture.detectChanges();
    expect((fixture.componentInstance as unknown as PersonPageHost).loading()).toBe(false);
  });
  return { fixture, http, page: fixture.componentInstance as unknown as PersonPageHost };
}

interface PersonPageHost {
  loading(): boolean;
  form: { patchValue(value: Record<string, unknown>): void; invalid: boolean };
  selectedType(): UserType;
  showAdmissionNote(): boolean;
  save(): void;
}

describe('PersonPageComponent, adding a person', () => {
  it('says a student is completed in Admissions, and only a student', async () => {
    const { page } = await render();

    page.form.patchValue({ userType: UserType.Student });
    expect(page.showAdmissionNote()).toBe(true);
    page.form.patchValue({ userType: UserType.Teacher });
    expect(page.showAdmissionNote()).toBe(false);
  });

  it('adds a teacher with trimmed names, and leaves out a blank middle name and phone', async () => {
    const { page, http, fixture } = await render();
    const router = TestBed.inject(Router);
    const navigate = vi.spyOn(router, 'navigate').mockResolvedValue(true);

    page.form.patchValue({
      firstName: '  Ayesha ',
      middleName: '   ',
      lastName: ' Khan ',
      userType: UserType.Teacher,
      phone: '  ',
      email: 'ayesha@school.local',
      oneTimePassword: 'violet harbour lantern 47',
      roles: [],
      campuses: [],
      children: [],
    });
    expect(page.form.invalid).toBe(false);
    page.save();

    const request = http.expectOne({ method: 'POST', url: '/api/UserDetails' });
    const body = JSON.parse(request.request.body as string) as Record<string, unknown>;
    expect(body).toMatchObject({
      firstName: 'Ayesha',
      lastName: 'Khan',
      userType: UserType.Teacher,
      email: 'ayesha@school.local',
      oneTimePassword: 'violet harbour lantern 47',
    });
    expect(body).not.toHaveProperty('middleName');
    expect(body).not.toHaveProperty('phone');
    request.flush(json(NEW_PERSON));

    await vi.waitFor(() => {
      fixture.detectChanges();
      expect(navigate).toHaveBeenCalledWith(['/people']);
    });
  });

  it('does not send a person without a one-time password of at least 15 characters', async () => {
    const { page, http } = await render();

    page.form.patchValue({ firstName: 'Ayesha', lastName: 'Khan', userType: UserType.Teacher, email: 'ayesha@school.local', oneTimePassword: 'too short' });
    expect(page.form.invalid).toBe(true);
    page.save();

    http.expectNone({ method: 'POST', url: '/api/UserDetails' });
  });

  it('keeps a middle name and a phone number when they are given, trimmed', async () => {
    const { page, http } = await render();
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    page.form.patchValue({
      firstName: 'Ayesha',
      middleName: ' Noor ',
      lastName: 'Khan',
      userType: UserType.Teacher,
      phone: ' +92 300 1234567 ',
      email: 'ayesha@school.local',
      oneTimePassword: 'violet harbour lantern 47',
    });
    page.save();

    const request = http.expectOne({ method: 'POST', url: '/api/UserDetails' });
    expect(JSON.parse(request.request.body as string)).toMatchObject({ middleName: 'Noor', phone: '+92 300 1234567' });
    request.flush(json(NEW_PERSON));
    await vi.waitFor(() => expect(navigate).toHaveBeenCalledWith(['/people']));
  });
});

describe('PersonPageComponent, changing a person', () => {
  const EXISTING = '0190f3a0-0000-7000-8000-000000000023';

  it('saves the changes without a login: no email or one-time password is sent', async () => {
    TestBed.configureTestingModule({
      imports: [PersonPageComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: API_BASE_URL, useValue: '' },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap({ id: EXISTING }) } } },
      ],
    });
    const fixture = TestBed.createComponent(PersonPageComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();

    const existing = { id: EXISTING, firstName: 'Ayesha', lastName: 'Khan', userType: UserType.Teacher, email: 'ayesha@school.local' };
    for (let round = 0; round < 10; round++) {
      await tick();
      const pending = http.match(() => true);
      if (pending.length === 0) {
        break;
      }
      for (const request of pending) {
        if (request.request.url.includes('/built-in')) {
          request.flush(null, { status: 204, statusText: 'No Content' });
        } else if (request.request.url === '/api/UserDetails' || request.request.url === '/api/UserDetails?') {
          request.flush(json([existing]));
        } else {
          request.flush(json([]));
        }
      }
    }
    await vi.waitFor(() => expect((fixture.componentInstance as unknown as PersonPageHost).loading()).toBe(false));

    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    const page = fixture.componentInstance as unknown as PersonPageHost;
    page.form.patchValue({ firstName: ' Ayesha ', middleName: '', lastName: 'Khan', userType: UserType.Teacher });
    page.save();

    const request = http.expectOne({ method: 'PUT', url: `/api/UserDetails/${EXISTING}` });
    const body = JSON.parse(request.request.body as string) as Record<string, unknown>;
    expect(body).toMatchObject({ firstName: 'Ayesha', lastName: 'Khan' });
    expect(body).not.toHaveProperty('email');
    expect(body).not.toHaveProperty('oneTimePassword');
    request.flush(null, { status: 204, statusText: 'No Content' });
    await vi.waitFor(() => expect(navigate).toHaveBeenCalledWith(['/people']));
  });
});
