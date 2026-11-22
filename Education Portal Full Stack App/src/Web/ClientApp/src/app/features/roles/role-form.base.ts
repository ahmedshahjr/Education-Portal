import { inject } from '@angular/core';
import { FormBuilder, FormControl, Validators } from '@angular/forms';
import { CampusDto, ModuleCode, RolePermissionDto, SchoolRoleDto } from '../../web-api-client';
import { RIGHT_KEYS, Right, rolePermissions } from './role-rights';

/** A module of the school as the roles screen needs it: its code (the name comes from the code, in the page's language). */
export interface RoleModule {
  readonly code: ModuleCode;
}

/** What the form saves: the role's name, description and rights. */
export interface RoleFormResult {
  readonly name: string;
  readonly description: string | undefined;
  readonly permissions: RolePermissionDto[];
}

function buildRoleForm(formBuilder: FormBuilder) {
  return formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: ['', [Validators.maxLength(500)]],
  });
}

/** The type of the role form's group (name and description). */
export type RoleForm = ReturnType<typeof buildRoleForm>;

/**
 * The role form's logic, shared by the add and edit role pages: the name and description, one checkbox per (module, right),
 * a built-in role's locked name, and the rights it sends. The data is applied once it has loaded (`applyRole`), so one
 * page can serve both adding and editing. The School module is not shown (every school has it; a role does not set it).
 */
export abstract class RoleFormBase {
  protected readonly formBuilder = inject(FormBuilder);

  protected role: SchoolRoleDto | null = null;
  protected modules: readonly RoleModule[] = [];

  /** The school's campuses (ADR-022). With one campus the grid has no campus choice: the rights are for all campuses. */
  protected campuses: readonly CampusDto[] = [];

  protected readonly rights: readonly { key: Right; label: string }[];

  /** One checkbox per (module, right), and one per (module, campus) plus one "all campuses" box per module. */
  private readonly controls = new Map<string, FormControl<boolean>>();

  protected readonly form: RoleForm = buildRoleForm(this.formBuilder);

  constructor(rightLabels: Record<Right, string>) {
    this.rights = RIGHT_KEYS.map((key) => ({ key, label: rightLabels[key] }));
  }

  /** True when the school has more than one campus, so each module gets a campus choice. */
  protected get multiCampus(): boolean {
    return this.campuses.length > 1;
  }

  /** Puts a role (or nothing, when adding), the school's modules and its campuses into the form. */
  protected applyRole(role: SchoolRoleDto | null, modules: readonly RoleModule[], campuses: readonly CampusDto[]): void {
    this.role = role;
    this.campuses = campuses;
    this.form.reset({ name: role?.name ?? '', description: role?.description ?? '' });
    if (role?.isSystem) {
      this.form.controls.name.disable();
    } else {
      this.form.controls.name.enable();
    }
    this.setModules(modules);
  }

  /** Sets the modules the grid shows, and the rights and campuses of the role in them. */
  protected setModules(modules: readonly RoleModule[]): void {
    this.modules = modules.filter((m) => m.code !== ModuleCode.School);
    for (const module of this.modules) {
      const permission = this.role?.permissions.find((p) => p.module === module.code);
      for (const right of RIGHT_KEYS) {
        this.grant(module.code, right).setValue(permission?.[right] ?? false);
      }
      // A role with no rights in the module, or a new role, gets all campuses.
      const allCampuses = permission?.allCampuses ?? true;
      this.allCampusesGrant(module.code).setValue(allCampuses);
      for (const campus of this.campuses) {
        this.campusGrant(module.code, campus.id).setValue(!allCampuses && (permission?.campusIds ?? []).includes(campus.id));
      }
    }
  }

  /** True when every right of every shown module is ticked (drives the "select all" box). */
  protected allSelected(): boolean {
    return this.modules.length > 0 && this.modules.every((m) => RIGHT_KEYS.every((r) => this.grant(m.code, r).value));
  }

  /** Ticks or clears every right of every shown module. */
  protected selectAll(on: boolean): void {
    for (const module of this.modules) {
      for (const right of RIGHT_KEYS) {
        this.grant(module.code, right).setValue(on);
      }
    }
  }

  protected grant(module: string, right: Right): FormControl<boolean> {
    return this.control(`${module}:${right}`);
  }

  /** The "all campuses" box of a module. */
  protected allCampusesGrant(module: string): FormControl<boolean> {
    return this.control(`${module}:all-campuses`);
  }

  /** The box of one campus in a module; only used when the module is not for all campuses. */
  protected campusGrant(module: string, campusId: string): FormControl<boolean> {
    return this.control(`${module}:campus:${campusId}`);
  }

  private control(key: string): FormControl<boolean> {
    let control = this.controls.get(key);
    if (!control) {
      control = this.formBuilder.nonNullable.control(false);
      this.controls.set(key, control);
    }
    return control;
  }

  /** The campus choice a module's rights are sent with: all campuses, or the campuses ticked (ADR-022). */
  private campusScope(module: ModuleCode): { allCampuses: boolean; campusIds: string[] } {
    const allCampuses = !this.multiCampus || this.allCampusesGrant(module).value;
    const campusIds = allCampuses ? [] : this.campuses.filter((c) => this.campusGrant(module, c.id).value).map((c) => c.id);
    return { allCampuses, campusIds };
  }

  /** Builds the result from the form, or marks the form as touched and returns null when it is not valid. */
  protected buildResult(): RoleFormResult | null {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return null;
    }

    const value = this.form.getRawValue();
    const permissions = rolePermissions(
      this.modules.map((m) => m.code),
      (module, right) => this.grant(module, right).value
    ).map((p) => new RolePermissionDto({ ...p, ...this.campusScope(p.module) }));

    return { name: value.name.trim(), description: value.description.trim() || undefined, permissions };
  }
}
