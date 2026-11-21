import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { describe, expect, it, vi } from 'vitest';
import { AccessRight, ModuleCode, MyRightsDto, UsersClient } from '../../web-api-client';
import { RightsService } from './rights.service';

const rights = (grants: { all: boolean; campusIds?: string[] }): MyRightsDto =>
  MyRightsDto.fromJS({
    modules: [
      {
        module: ModuleCode.Students,
        rights: [{ right: AccessRight.Edit, allCampuses: grants.all, campusIds: grants.campusIds ?? [] }],
      },
    ],
  });

function setup(answer: () => ReturnType<UsersClient['getMyRights']>) {
  TestBed.configureTestingModule({
    providers: [{ provide: UsersClient, useValue: { getMyRights: vi.fn(answer) } }],
  });
  return TestBed.inject(RightsService);
}

describe('RightsService', () => {
  it('gives no rights before anything is loaded (fails closed)', () => {
    const service = setup(() => of(rights({ all: true })));

    expect(service.can(ModuleCode.Students, AccessRight.Edit)).toBe(false);
  });

  it('gives a right on every campus when the role applies to all campuses', async () => {
    const service = setup(() => of(rights({ all: true })));
    await new Promise<void>((resolve) => service.load().subscribe(() => resolve()));

    expect(service.can(ModuleCode.Students, AccessRight.Edit)).toBe(true);
    expect(service.can(ModuleCode.Students, AccessRight.Edit, 'any-campus')).toBe(true);
    expect(service.can(ModuleCode.Students, AccessRight.Delete)).toBe(false);
  });

  it('gives a right only on the listed campuses', async () => {
    const service = setup(() => of(rights({ all: false, campusIds: ['north'] })));
    await new Promise<void>((resolve) => service.load().subscribe(() => resolve()));

    expect(service.can(ModuleCode.Students, AccessRight.Edit, 'north')).toBe(true);
    expect(service.can(ModuleCode.Students, AccessRight.Edit, 'south')).toBe(false);
    // Without a campus the question is "any campus", as on the server.
    expect(service.can(ModuleCode.Students, AccessRight.Edit)).toBe(true);
  });

  it('fails closed when loading the rights fails', async () => {
    const service = setup(() => throwError(() => new Error('down')));
    await new Promise<void>((resolve) => service.load().subscribe(() => resolve()));

    expect(service.can(ModuleCode.Students, AccessRight.Edit)).toBe(false);
  });
});
