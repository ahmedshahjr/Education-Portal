import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { describe, expect, it, vi } from 'vitest';
import { RequestSchoolPage } from './request-school.page';

describe('RequestSchoolPage', () => {
  function setup() {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    const fixture = TestBed.createComponent(RequestSchoolPage);
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

  it('sends the request and confirms it', async () => {
    const { fixture, element } = setup();
    fill(element, 'schoolName', 'Greenfield School');
    fill(element, 'contactName', 'Ayesha Khan');
    fill(element, 'contactEmail', 'ayesha@greenfield.example');
    fixture.detectChanges();

    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
    const http = TestBed.inject(HttpTestingController);
    const request = http.expectOne('/api/SchoolRequests');
    expect(request.request.method).toBe('POST');
    // The generated client sends the command as JSON text.
    expect(JSON.parse(request.request.body as string)).toMatchObject({ schoolName: 'Greenfield School', contactEmail: 'ayesha@greenfield.example' });
    request.flush(new Blob(['1'], { type: 'application/json' }));

    await vi.waitFor(() => expect(element.querySelector('[role="status"]')).not.toBeNull());
  });
});
