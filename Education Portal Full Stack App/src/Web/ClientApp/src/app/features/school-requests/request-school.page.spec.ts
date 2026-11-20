import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { describe, expect, it, vi } from 'vitest';
import { RequestSchoolPage } from './request-school.page';

const PK_ID = '0190f3a0-0000-7000-8000-000000000001';

describe('RequestSchoolPage', () => {
  function setup() {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    const fixture = TestBed.createComponent(RequestSchoolPage);
    fixture.detectChanges();
    // The countries come from the server when the page opens.
    TestBed.inject(HttpTestingController)
      .expectOne('/api/SchoolRequests/countries')
      .flush(new Blob([JSON.stringify([{ id: PK_ID, name: 'Pakistan', code: 'PK' }])], { type: 'application/json' }));
    fixture.detectChanges();
    return { fixture, element: fixture.nativeElement as HTMLElement };
  }

  function fill(element: HTMLElement, id: string, value: string) {
    const input = element.querySelector<HTMLInputElement>(`#${id}`)!;
    input.value = value;
    input.dispatchEvent(new Event('input'));
  }

  it('sends nothing until every field is filled in', () => {
    const { element } = setup();

    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();

    TestBed.inject(HttpTestingController).expectNone('/api/SchoolRequests');
  });

  it('registers the school and confirms it, with the password chosen', async () => {
    const { fixture, element } = setup();
    fill(element, 'schoolName', 'Greenfield School');
    fill(element, 'contactName', 'Ayesha Khan');
    fill(element, 'contactEmail', 'ayesha@greenfield.example');
    fill(element, 'administratorPassword', 'violet harbour lantern 47');
    fixture.componentInstance['form'].controls.countryId.setValue(PK_ID);
    fixture.detectChanges();

    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
    const http = TestBed.inject(HttpTestingController);
    const request = http.expectOne('/api/SchoolRequests');
    expect(request.request.method).toBe('POST');
    // The generated client sends the command as JSON text.
    expect(JSON.parse(request.request.body as string)).toMatchObject({
      schoolName: 'Greenfield School',
      countryId: PK_ID,
      contactEmail: 'ayesha@greenfield.example',
      administratorPassword: 'violet harbour lantern 47',
    });
    request.flush(new Blob(['"0190f3a0-0000-7000-8000-000000000009"'], { type: 'application/json' }));

    await vi.waitFor(() => expect(element.querySelector('[role="status"]')).not.toBeNull());
  });
});
