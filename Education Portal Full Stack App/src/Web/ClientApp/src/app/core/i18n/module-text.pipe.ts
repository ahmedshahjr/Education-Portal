import { Pipe, PipeTransform } from '@angular/core';

interface ModuleText {
  readonly name: string;
  readonly description: string;
}

/**
 * Module names and descriptions in the page's language, by module key (Domain/Constants/ModuleKeys.cs). The server
 * stores them in English only (they are fixed in code, ModuleCatalog.cs), so the translations live here, next to the
 * rest of the app's text. A key added on the server before it is added here shows the server's English text.
 */
const MODULE_TEXT: Readonly<Record<string, ModuleText>> = {
  Branches: {
    name: $localize`:@@module.branches.name:Branches`,
    description: $localize`:@@module.branches.description:Create and edit the school's branches.`,
  },
  UsersAndRoles: {
    name: $localize`:@@module.usersAndRoles.name:Users & Roles`,
    description: $localize`:@@module.usersAndRoles.description:Staff accounts, roles with rights per module, branch assignment.`,
  },
  Students: {
    name: $localize`:@@module.students.name:Students`,
    description: $localize`:@@module.students.description:Student records, branch membership, link to parents.`,
  },
  Parents: {
    name: $localize`:@@module.parents.name:Parents`,
    description: $localize`:@@module.parents.description:Parent records, link to children.`,
  },
  Teachers: {
    name: $localize`:@@module.teachers.name:Teachers`,
    description: $localize`:@@module.teachers.description:Teacher records, branch membership.`,
  },
  Courses: {
    name: $localize`:@@module.courses.name:Courses`,
    description: $localize`:@@module.courses.description:Courses per branch, assigned teachers.`,
  },
  Enrolments: {
    name: $localize`:@@module.enrolments.name:Enrolments`,
    description: $localize`:@@module.enrolments.description:Enrolling students in courses.`,
  },
  Grades: {
    name: $localize`:@@module.grades.name:Grades`,
    description: $localize`:@@module.grades.description:Assessments and scores per enrolment.`,
  },
  Dashboard: {
    name: $localize`:@@module.dashboard.name:Dashboard`,
    description: $localize`:@@module.dashboard.description:School and branch overview for owners and staff.`,
  },
};

/** The parts of a module the pipe reads; matches ModuleDto and SchoolModuleDto. */
export interface ModuleLike {
  readonly key?: string;
  readonly name?: string;
  readonly description?: string;
}

/** A module's name or description in the page's language, falling back to the server's text. */
export function moduleText(module: ModuleLike, part: keyof ModuleText = 'name'): string {
  return (module.key ? MODULE_TEXT[module.key]?.[part] : undefined) ?? module[part] ?? '';
}

/** `{{ module | moduleText }}` or `{{ module | moduleText: 'description' }}`. */
@Pipe({ name: 'moduleText' })
export class ModuleTextPipe implements PipeTransform {
  transform(module: ModuleLike, part: keyof ModuleText = 'name'): string {
    return moduleText(module, part);
  }
}
