import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { SchoolSummaryDto } from '../../web-api-client';
import { SchoolProfileComponent } from './school-profile.component';

const SCHOOL = '0190f3a0-0000-7000-8000-000000000005';

function render(isMaster = false) {
  TestBed.configureTestingModule({ imports: [SchoolProfileComponent], providers: [provideHttpClient(), provideHttpClientTesting()] });
  const http = TestBed.inject(HttpTestingController);
  const fixture = TestBed.createComponent(SchoolProfileComponent);
  fixture.componentRef.setInput(
    'school',
    new SchoolSummaryDto({ id: SCHOOL, name: 'School One', domain: 'school1.com', isMaster, countryId: undefined })
  );
  fixture.detectChanges();
  const element = fixture.nativeElement as HTMLElement;
  const input = (name: string) => element.querySelector<HTMLInputElement>(`input[formcontrolname="${name}"]`)!;
  const type = (el: HTMLInputElement, value: string) => {
    el.value = value;
    el.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  };
  const save = () => element.querySelector<HTMLButtonElement>('button[type="submit"]')!;
  return { fixture, http, element, input, type, save };
}

describe('SchoolProfileComponent', () => {
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('shows the school as it is and no save bar until something changes', () => {
    const { element, input } = render();

    expect(input('name').value).toBe('School One');
    expect(input('domain').value).toBe('school1.com');
    expect(element.querySelector('.mm-school-profile__bar')).toBeNull();
  });

  it('shows the save bar once a field changes, and Cancel puts the values back', () => {
    const { fixture, element, input, type } = render();

    type(input('name'), 'School Uno');
    expect(element.querySelector('.mm-school-profile__bar')).not.toBeNull();

    element.querySelector<HTMLButtonElement>('.mm-school-profile__bar button')!.click();
    fixture.detectChanges();
    expect(input('name').value).toBe('School One');
    expect(element.querySelector('.mm-school-profile__bar')).toBeNull();
  });

  it('locks the domain of the master tenant', () => {
    const { input } = render(true);

    expect(input('domain').disabled).toBe(true);
  });

  it('sends the name and the lower-case domain, then reports the saved school', async () => {
    const { fixture, http, input, type, save } = render();
    const saved = vi.fn();
    fixture.componentInstance.saved.subscribe(saved);

    type(input('name'), '  School Uno ');
    type(input('domain'), 'Uno.Example.com');
    save().click();

    const request = http.expectOne(`/api/Schools/${SCHOOL}`);
    expect(request.request.method).toBe('PUT');
    expect(JSON.parse(request.request.body as string)).toMatchObject({ name: 'School Uno', domain: 'uno.example.com' });
    request.flush(null, { status: 204, statusText: 'No Content' });

    // The generated client reads the response asynchronously.
    await vi.waitFor(() => expect(saved).toHaveBeenCalledWith(expect.objectContaining({ id: SCHOOL, name: 'School Uno', domain: 'uno.example.com' })));
  });

  it('shows the server message when a save is refused', async () => {
    const { fixture, http, element, input, type, save } = render();

    type(input('domain'), 'taken.example.com');
    save().click();

    http
      .expectOne(`/api/Schools/${SCHOOL}`)
      .flush(
        new Blob([JSON.stringify({ title: 'Validation failed', status: 400, errors: { Domain: ['Another school already uses this domain.'] } })], { type: 'application/json' }),
        { status: 400, statusText: 'Bad Request' }
      );
    await vi.waitFor(() => {
      fixture.detectChanges();
      expect(element.querySelector('[role="alert"]')).not.toBeNull();
    });
  });
});
