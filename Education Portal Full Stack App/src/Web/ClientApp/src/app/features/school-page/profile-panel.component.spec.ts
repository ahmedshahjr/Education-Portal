import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';
import { API_BASE_URL } from '../../web-api-client';
import { ProfilePanelComponent } from './profile-panel.component';

const blob = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });
const emptyProfile = { tagline: '', about: '', genderPolicy: 'CoEducation', isPublished: false };

async function render(profile: unknown = emptyProfile) {
  TestBed.configureTestingModule({
    providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_BASE_URL, useValue: '' }],
  });
  const fixture = TestBed.createComponent(ProfilePanelComponent);
  const http = TestBed.inject(HttpTestingController);
  fixture.detectChanges();
  http.expectOne((r) => r.url.startsWith('/api/SchoolProfile') && r.method === 'GET').flush(blob(profile));
  const element = fixture.nativeElement as HTMLElement;
  await vi.waitFor(async () => {
    await fixture.whenStable();
    fixture.detectChanges();
    expect(element.querySelector('form')).not.toBeNull();
  });
  return { fixture, http, element };
}

function fill(element: HTMLElement, selector: string, value: string) {
  const input = element.querySelector<HTMLInputElement | HTMLTextAreaElement>(selector)!;
  input.value = value;
  input.dispatchEvent(new Event('input'));
}

describe('ProfilePanelComponent', () => {
  it('starts as a draft that cannot be published before it is saved', async () => {
    const { element } = await render();

    expect(element.textContent).toContain('Draft');
    const publish = [...element.querySelectorAll('button')].find((b) => b.textContent?.includes('Publish'))!;
    expect(publish.disabled).toBe(true);
  });

  it('sends nothing until the tagline and the text are filled in', async () => {
    const { fixture, http, element } = await render();

    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
    fixture.detectChanges();

    http.expectNone((r) => r.method === 'PUT');
  });

  it('saves the page', async () => {
    const { fixture, http, element } = await render();
    fill(element, 'input[formcontrolname="tagline"]', 'Learning together');
    fill(element, 'textarea[formcontrolname="about"]', 'A school for everyone.');
    fixture.detectChanges();

    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
    const request = http.expectOne((r) => r.url.startsWith('/api/SchoolProfile') && r.method === 'PUT');

    expect(JSON.parse(request.request.body as string)).toMatchObject({ tagline: 'Learning together', about: 'A school for everyone.' });
    request.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('shows a published page and offers to take it down', async () => {
    const { element } = await render({ ...emptyProfile, tagline: 'Hello', about: 'About', isPublished: true });

    expect(element.textContent).toContain('Published');
    expect([...element.querySelectorAll('button')].some((b) => b.textContent?.includes('Take down'))).toBe(true);
  });
});
