import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ModuleCode, RolePermissionDto, SchoolRoleDto } from '../../web-api-client';
import { RolesPage } from './roles.page';

const json = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

const teacher = {
  id: '0190f3a0-0000-7000-8000-000000000001',
  name: 'Teacher',
  description: 'Teaches classes',
  isSystem: true,
  permissions: [{ module: 'Students', canView: true, canCreate: true, canEdit: false, canDelete: false, canApprove: true, canExport: false }],
};

const registrar = {
  id: '0190f3a0-0000-7000-8000-000000000002',
  name: 'Registrar',
  description: undefined,
  isSystem: false,
  permissions: [],
};

const students = { code: 'Students', name: 'Students', description: '', isCore: true, isActive: true, isDefaultEnabled: true, sortOrder: 1, requires: [] };

async function render() {
  TestBed.configureTestingModule({
    imports: [RolesPage],
    providers: [provideHttpClient(), provideHttpClientTesting()],
  });
  const http = TestBed.inject(HttpTestingController);
  const fixture = TestBed.createComponent(RolesPage);
  fixture.detectChanges();

  http.expectOne('/api/SchoolRoles').flush(json([teacher, registrar]));
  http.expectOne('/api/Modules/enabled').flush(json(['Students']));
  await vi.waitFor(() => http.expectOne('/api/Modules').flush(json([students])));

  const element = fixture.nativeElement as HTMLElement;
  await vi.waitFor(() => {
    fixture.detectChanges();
    // The modules come in after the roles; the rights columns show once both are there.
    expect(element.textContent).toContain('View, Create, Approve');
  });
  return { fixture, http, element, page: fixture.componentInstance };
}

describe('RolesPage', () => {
  beforeEach(() => TestBed.resetTestingModule());
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('marks a built-in role, shows its description and its approve right, and offers no delete for it', async () => {
    const { element } = await render();

    expect(element.textContent).toContain('Built-in');
    expect(element.textContent).toContain('Teaches classes');
    expect(element.textContent).toContain('View, Create, Approve');
    expect(element.querySelector('[aria-label="Delete Teacher"]')).toBeNull();
    expect(element.querySelector('[aria-label="Delete Registrar"]')).not.toBeNull();
  });

  it('saves the changes a role dialog returns, then loads the list again', async () => {
    const { http, page } = await render();

    // The dialog closes with the role's name, description and rights; the page sends them.
    page['saveRole'](teacher as unknown as SchoolRoleDto, {
      name: 'Teacher',
      description: 'Teaches and marks',
      permissions: [
        new RolePermissionDto({ module: ModuleCode.Students, canView: true, canCreate: true, canEdit: false, canDelete: false, canApprove: true, canExport: true }),
      ],
    });

    const request = http.expectOne(`/api/SchoolRoles/${teacher.id}`);
    expect(request.request.method).toBe('PUT');
    const body = JSON.parse(request.request.body as string);
    expect(body.name).toBe('Teacher');
    expect(body.description).toBe('Teaches and marks');
    expect(body.permissions).toEqual([
      { module: 'Students', canView: true, canCreate: true, canEdit: false, canDelete: false, canApprove: true, canExport: true },
    ]);
    request.flush(null, { status: 204, statusText: 'No Content' });

    // The list loads again.
    http.expectOne('/api/SchoolRoles').flush(json([teacher, registrar]));
    http.expectOne('/api/Modules/enabled').flush(json(['Students']));
    await vi.waitFor(() => http.expectOne('/api/Modules').flush(json([students])));
    await vi.waitFor(() => expect(page['modules']().length).toBe(1));
  });
});
