import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
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

  const element = fixture.nativeElement as HTMLElement;
  await vi.waitFor(() => {
    fixture.detectChanges();
    expect(element.textContent).toContain('View, Create, Approve');
  });
  return { fixture, http, element };
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
});
