import { ClassLevel } from '../../web-api-client';
import { classLevelLabel } from '../school-page/school-page.labels';

/** The part of a class offering the pickers need. */
export interface OfferingLike {
  readonly level: ClassLevel;
  readonly academicYear: string;
}

/** The part of a person the pickers need. */
export interface PersonLike {
  readonly firstName: string;
  readonly middleName?: string | null;
  readonly lastName: string;
}

/** How a class offering is named in a dropdown: its class and academic year, such as "Class 1 · 2026-27". */
export function offeringLabel(offering: OfferingLike): string {
  return `${classLevelLabel(offering.level)} · ${offering.academicYear}`;
}

/** A person's full name: first, middle (when there is one) and last name. */
export function personName(person: PersonLike): string {
  return [person.firstName, person.middleName, person.lastName].filter((part): part is string => !!part && part.trim() !== '').join(' ');
}
