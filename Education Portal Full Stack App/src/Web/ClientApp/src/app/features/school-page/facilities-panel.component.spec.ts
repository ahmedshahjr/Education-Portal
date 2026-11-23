import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';
import { API_BASE_URL } from '../../web-api-client';
import { FacilitiesPanelComponent } from './facilities-panel.component';
import { FACILITY_KINDS } from './school-page.labels';

const blob = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

describe('FacilitiesPanelComponent', () => {
  it('shows one box for each facility the school can have, each with its own label', async () => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_BASE_URL, useValue: '' }],
    });
    const fixture = TestBed.createComponent(FacilitiesPanelComponent);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    http.expectOne((r) => r.url.startsWith('/api/SchoolFacilities')).flush(blob([]));
    const element = fixture.nativeElement as HTMLElement;

    await vi.waitFor(() => {
      fixture.detectChanges();
      expect(element.querySelectorAll('mat-checkbox').length).toBe(FACILITY_KINDS.length);
    });
    const labels = [...element.querySelectorAll('mat-checkbox')].map((box) => box.textContent?.trim() ?? '');
    expect(labels.every((text) => text.length > 0)).toBe(true);
    expect(new Set(labels).size).toBe(FACILITY_KINDS.length);
  });
});
