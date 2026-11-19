import { Pipe, PipeTransform } from '@angular/core';
import { ModuleCode } from '../../web-api-client';

interface ModuleText {
  readonly name: string;
  readonly description: string;
}

/**
 * Module names and descriptions in the page's language, by module code (the ModuleCode enum). The server
 * stores them in English only (seeded from ModuleCatalog.cs), so the translations live here, next to the rest of the
 * app's text. A module the master adds or renames in the catalogue shows the server's text until it is added here.
 */
const MODULE_TEXT: Readonly<Partial<Record<ModuleCode, ModuleText>>> = {
  School: {
    name: $localize`:@@module.school.name:School`,
    description: $localize`:@@module.school.description:School profile, settings and the school overview dashboard.`,
  },
  Campus: {
    name: $localize`:@@module.campus.name:Campuses`,
    description: $localize`:@@module.campus.description:Create and edit the school's campuses.`,
  },
  AcademicSetup: {
    name: $localize`:@@module.academicSetup.name:Academic setup`,
    description: $localize`:@@module.academicSetup.description:Academic years, classes, courses and enrolments.`,
  },
  UsersRoles: {
    name: $localize`:@@module.usersRoles.name:Users & Roles`,
    description: $localize`:@@module.usersRoles.description:Staff accounts, roles with rights per module, campus assignment.`,
  },
  Students: {
    name: $localize`:@@module.students.name:Students`,
    description: $localize`:@@module.students.description:Student records and campus membership.`,
  },
  Staff: {
    name: $localize`:@@module.staff.name:Staff`,
    description: $localize`:@@module.staff.description:Teacher and staff records and campus membership.`,
  },
  Admissions: {
    name: $localize`:@@module.admissions.name:Admissions`,
    description: $localize`:@@module.admissions.description:Enquiries, applications and admission decisions.`,
  },
  Attendance: {
    name: $localize`:@@module.attendance.name:Attendance`,
    description: $localize`:@@module.attendance.description:Daily and per-lesson attendance for students and staff.`,
  },
  Fees: {
    name: $localize`:@@module.fees.name:Fees`,
    description: $localize`:@@module.fees.description:Fee structures, student invoices and fee collection.`,
  },
  ParentCommunication: {
    name: $localize`:@@module.parentCommunication.name:Parent communication`,
    description: $localize`:@@module.parentCommunication.description:Parent records, links to children and messages to parents.`,
  },
  Exams: {
    name: $localize`:@@module.exams.name:Exams`,
    description: $localize`:@@module.exams.description:Assessments, scores and report cards.`,
  },
  Timetable: {
    name: $localize`:@@module.timetable.name:Timetable`,
    description: $localize`:@@module.timetable.description:Class and teacher timetables.`,
  },
  HrPayroll: {
    name: $localize`:@@module.hrPayroll.name:HR & Payroll`,
    description: $localize`:@@module.hrPayroll.description:Staff contracts, leave and payroll.`,
  },
  Transport: {
    name: $localize`:@@module.transport.name:Transport`,
    description: $localize`:@@module.transport.description:Routes, vehicles and student transport.`,
  },
  Library: {
    name: $localize`:@@module.library.name:Library`,
    description: $localize`:@@module.library.description:Library catalogue, loans and returns.`,
  },
  Lms: {
    name: $localize`:@@module.lms.name:LMS`,
    description: $localize`:@@module.lms.description:Online lessons, assignments and course material.`,
  },
  Hostel: {
    name: $localize`:@@module.hostel.name:Hostel`,
    description: $localize`:@@module.hostel.description:Hostel rooms, allocations and boarders.`,
  },
};

/** The parts of a module the pipe reads; matches ModuleDto and SchoolModuleDto. */
export interface ModuleLike {
  readonly code?: string;
  readonly name?: string;
  readonly description?: string;
}

/** A module's name or description in the page's language, falling back to the server's text. */
export function moduleText(module: ModuleLike, part: keyof ModuleText = 'name'): string {
  return (module.code ? MODULE_TEXT[module.code]?.[part] : undefined) ?? module[part] ?? '';
}

/** `{{ module | moduleText }}` or `{{ module | moduleText: 'description' }}`. */
@Pipe({ name: 'moduleText' })
export class ModuleTextPipe implements PipeTransform {
  transform(module: ModuleLike, part: keyof ModuleText = 'name'): string {
    return moduleText(module, part);
  }
}
