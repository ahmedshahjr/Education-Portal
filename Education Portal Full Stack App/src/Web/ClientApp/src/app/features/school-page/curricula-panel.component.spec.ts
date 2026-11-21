import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';
import { API_BASE_URL } from '../../web-api-client';
import { CurriculaPanelComponent } from './curricula-panel.component';

const blob = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

async function render(saved: string[]) {
  TestBed.configureTestingModule({
    providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_BASE_URL, useValue: '' }],
  });
  const fixture = TestBed.createComponent(CurriculaPanelComponent);
  const http = TestBed.inject(HttpTestingController);
  fixture.detectChanges();
  http.expectOne((r) => r.url.startsWith('/api/SchoolCurricula') && r.method === 'GET').flush(blob(saved));
  const element = fixture.nativeElement as HTMLElement;
  await vi.waitFor(async () => {
    await fixture.whenStable();
    fixture.detectChanges();
    expect(element.querySelectorAll('mat-checkbox').length).toBeGreaterThan(0);
  });
  return { fixture, http, element };
}

describe('CurriculaPanelComponent', () => {
  it('ticks the curricula the school already teaches', async () => {
    const { element } = await render(['Cambridge', 'Montessori']);

    const checked = [...element.querySelectorAll('mat-checkbox')].filter((c) => c.querySelector('input')?.checked).map((c) => c.textContent?.trim());
    expect(checked).toEqual(['Cambridge', 'Montessori']);
  });

  it('saves the chosen curricula in the order of the list', async () => {
    const { fixture, http, element } = await render(['Montessori']);

    const boxes = [...element.querySelectorAll<HTMLInputElement>('mat-checkbox input')];
    boxes.find((box) => box.closest('mat-checkbox')?.textContent?.includes('Matric'))!.click();
    fixture.detectChanges();
    [...element.querySelectorAll('button')].find((b) => b.textContent?.includes('Save'))!.click();

    const request = http.expectOne((r) => r.url.startsWith('/api/SchoolCurricula') && r.method === 'PUT');
    expect(JSON.parse(request.request.body as string).kinds).toEqual(['Matric', 'Montessori']);
    request.flush(null, { status: 204, statusText: 'No Content' });
  });
});
