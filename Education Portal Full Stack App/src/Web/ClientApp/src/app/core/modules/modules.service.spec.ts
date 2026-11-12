import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ModulesService } from './modules.service';

describe('ModulesService', () => {
  let service: ModulesService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(ModulesService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('knows the modules switched on for the school', async () => {
    let done = false;
    service.load().subscribe(() => (done = true));
    // The generated client reads every response as a Blob, as the browser would deliver it.
    http.expectOne('/api/Modules/enabled').flush(new Blob([JSON.stringify(['Branches', 'Students'])], { type: 'application/json' }));

    await vi.waitFor(() => expect(done).toBe(true));
    expect(service.isEnabled('Students')).toBe(true);
    expect(service.isEnabled('Grades')).toBe(false);
  });

  it('shows no module when the list cannot be loaded', async () => {
    let done = false;
    service.load().subscribe(() => (done = true));
    http.expectOne('/api/Modules/enabled').flush(new Blob(['{}'], { type: 'application/json' }), { status: 500, statusText: 'Server Error' });

    await vi.waitFor(() => expect(done).toBe(true));
    expect(service.count()).toBe(0);
  });
});
