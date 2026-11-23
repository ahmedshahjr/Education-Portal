import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';
import { AccessRight, API_BASE_URL, ModuleCode } from '../../web-api-client';
import { RightsService } from './rights.service';

const json = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

/** The server's answer for `GET /api/Users/me/rights`: one module with one right. */
const answer = (grant: { allCampuses: boolean; campusIds?: string[] }) => ({
  modules: [{ module: ModuleCode.Students, rights: [{ right: AccessRight.Edit, allCampuses: grant.allCampuses, campusIds: grant.campusIds ?? [] }] }],
});

/** Starts the real service and its real client, and answers the rights request over HttpTestingController. */
async function loadWith(status: number, body: unknown) {
  TestBed.configureTestingModule({
    providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_BASE_URL, useValue: '' }],
  });
  const service = TestBed.inject(RightsService);
  const http = TestBed.inject(HttpTestingController);
  const done = new Promise<void>((resolve) => service.load().subscribe({ next: () => resolve(), error: () => resolve() }));
  http
    .expectOne('/api/Users/me/rights')
    .flush(json(body), { status, statusText: status === 200 ? 'OK' : 'Error' });
  await done;
  return service;
}

describe('RightsService (integration)', () => {
  it('gives no rights before anything is loaded (fails closed)', () => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_BASE_URL, useValue: '' }],
    });
    const service = TestBed.inject(RightsService);

    expect(service.can(ModuleCode.Students, AccessRight.Edit)).toBe(false);
  });

  it('gives a right on every campus when the role applies to all campuses', async () => {
    const service = await loadWith(200, answer({ allCampuses: true }));

    expect(service.can(ModuleCode.Students, AccessRight.Edit)).toBe(true);
    expect(service.can(ModuleCode.Students, AccessRight.Edit, 'any-campus')).toBe(true);
    expect(service.can(ModuleCode.Students, AccessRight.Delete)).toBe(false);
  });

  it('gives a right only on the listed campuses', async () => {
    const service = await loadWith(200, answer({ allCampuses: false, campusIds: ['north'] }));

    expect(service.can(ModuleCode.Students, AccessRight.Edit, 'north')).toBe(true);
    expect(service.can(ModuleCode.Students, AccessRight.Edit, 'south')).toBe(false);
    // Without a campus the question is "any campus", as on the server.
    expect(service.can(ModuleCode.Students, AccessRight.Edit)).toBe(true);
  });

  it('fails closed when loading the rights fails', async () => {
    const service = await loadWith(500, { title: 'Server error' });

    expect(service.can(ModuleCode.Students, AccessRight.Edit)).toBe(false);
  });
});
