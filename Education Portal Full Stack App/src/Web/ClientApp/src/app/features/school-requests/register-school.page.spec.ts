import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { describe, expect, it, vi } from 'vitest';
import { RegisterSchoolPage } from './register-school.page';

const COUNTRY_ID = '0190f3a0-0000-7000-8000-000000000001';
const PASSWORD = 'violet harbour lantern 47';

describe('RegisterSchoolPage', () => {
  async function setup() {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    const fixture = TestBed.createComponent(RegisterSchoolPage);
    fixture.detectChanges();
    const http = TestBed.inject(HttpTestingController);
    http
      .expectOne('/api/SchoolRegistrations/countries')
      .flush(new Blob([JSON.stringify([{ id: COUNTRY_ID, name: 'Pakistan' }])], { type: 'application/json' }));
    await fixture.whenStable();
    return { fixture, http, element: fixture.nativeElement as HTMLElement };
  }

  function fill(element: HTMLElement, id: string, value: string) {
    const input = element.querySelector<HTMLInputElement>(`#${id}`)!;
    input.value = value;
    input.dispatchEvent(new Event('input'));
  }

  function fillEverything(element: HTMLElement, repeat = PASSWORD) {
    fill(element, 'schoolName', 'Greenfield School');
    fill(element, 'administratorEmail', 'ayesha@greenfield.example');
    fill(element, 'newPassword', PASSWORD);
    fill(element, 'repeatPassword', repeat);
  }

  /** The country is a Material select; the form control is set directly instead of opening the overlay. */
  function chooseCountry(fixture: { componentInstance: unknown }) {
    (fixture.componentInstance as { form: { controls: { countryId: { setValue(value: string): void } } } }).form.controls.countryId.setValue(COUNTRY_ID);
  }

  it('offers the countries the server lists', async () => {
    const { fixture } = await setup();

    // The generated client reads every response as a Blob, which is read asynchronously.
    await vi.waitFor(() =>
      expect((fixture.componentInstance as unknown as { countries(): { name: string }[] }).countries().map((c) => c.name)).toEqual(['Pakistan']),
    );
  });

  it('sends nothing until every field is filled in', async () => {
    const { http, element } = await setup();

    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();

    http.expectNone('/api/SchoolRegistrations');
  });

  it('sends nothing while the two passwords differ', async () => {
    const { fixture, http, element } = await setup();
    fillEverything(element, 'a different passphrase 99');
    chooseCountry(fixture);
    fixture.detectChanges();

    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
    fixture.detectChanges();

    http.expectNone('/api/SchoolRegistrations');
    expect(element.textContent).toContain('The two passwords do not match.');
  });

  it('refuses a password shorter than the policy minimum', async () => {
    const { fixture, http, element } = await setup();
    fill(element, 'schoolName', 'Greenfield School');
    fill(element, 'administratorEmail', 'ayesha@greenfield.example');
    fill(element, 'newPassword', 'short');
    fill(element, 'repeatPassword', 'short');
    chooseCountry(fixture);
    fixture.detectChanges();

    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();

    http.expectNone('/api/SchoolRegistrations');
  });

  it('creates the school and sends the visitor to sign in', async () => {
    const { fixture, http, element } = await setup();
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    fillEverything(element);
    chooseCountry(fixture);
    fixture.detectChanges();

    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
    const request = http.expectOne('/api/SchoolRegistrations');
    expect(request.request.method).toBe('POST');
    expect(JSON.parse(request.request.body as string)).toMatchObject({
      schoolName: 'Greenfield School',
      countryId: COUNTRY_ID,
      administratorEmail: 'ayesha@greenfield.example',
      password: PASSWORD,
      confirmPassword: PASSWORD,
    });
    request.flush(new Blob([JSON.stringify('0190f3a0-0000-7000-8000-0000000000aa')], { type: 'application/json' }), { status: 201, statusText: 'Created' });

    await vi.waitFor(() => expect(navigate).toHaveBeenCalledWith('/login'));
  });

  it("shows the server's message and stays on the page when the school is refused", async () => {
    const { fixture, http, element } = await setup();
    fillEverything(element);
    chooseCountry(fixture);
    fixture.detectChanges();

    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
    http.expectOne('/api/SchoolRegistrations').flush(
      new Blob([JSON.stringify({ errors: { AdministratorEmail: ['Another account already uses this email.'] } })], { type: 'application/problem+json' }),
      { status: 400, statusText: 'Bad Request' },
    );

    await vi.waitFor(async () => {
      await fixture.whenStable();
      fixture.detectChanges();
      expect(element.querySelector('#register-error')?.textContent).toContain('Another account already uses this email.');
    });
  });
});
