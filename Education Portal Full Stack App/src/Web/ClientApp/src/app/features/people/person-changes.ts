import { UserType } from '../../web-api-client';

/** What the create-user modal returns. The account fields are set only when a person is added. */
export interface PersonChanges {
  readonly firstName: string;
  readonly middleName?: string;
  readonly lastName: string;
  readonly userType: UserType;
  readonly phone?: string;
  readonly email?: string;
  readonly oneTimePassword?: string;
}

/** The raw values of the modal's form, as the controls hold them. */
export interface PersonFormValue {
  readonly firstName: string;
  readonly middleName: string;
  readonly lastName: string;
  readonly userType: UserType;
  readonly phone: string;
  readonly email: string;
  readonly oneTimePassword: string;
}

/**
 * Turns the form into the fields the server takes: names and phone trimmed, blank optional values left out. A login is
 * asked for only when a person is added: an existing person's account is not changed here.
 */
export function buildPersonChanges(value: PersonFormValue, adding: boolean): PersonChanges {
  const base = {
    firstName: value.firstName.trim(),
    middleName: value.middleName.trim() || undefined,
    lastName: value.lastName.trim(),
    userType: value.userType,
    phone: value.phone.trim() || undefined,
  };

  return adding ? { ...base, email: value.email.trim(), oneTimePassword: value.oneTimePassword } : base;
}

/** A student's record is completed in Admissions (date of birth, father's name, class), so the modal says so. */
export function needsAdmissionDetails(type: UserType): boolean {
  return type === UserType.Student;
}

/** The links a parent's children need: the students to link and the ones to unlink, to go from the current list to the wanted one. */
export function planChildLinks(current: readonly string[], wanted: readonly string[]): { add: string[]; remove: string[] } {
  const now = new Set(current);
  const next = new Set(wanted);
  return {
    add: [...new Set(wanted)].filter((id) => !now.has(id)),
    remove: [...new Set(current)].filter((id) => !next.has(id)),
  };
}

/** True when two lists of role ids hold the same roles, in any order. */
export function sameRoles(a: readonly string[], b: readonly string[]): boolean {
  if (new Set(a).size !== new Set(b).size) {
    return false;
  }
  const other = new Set(b);
  return a.every((id) => other.has(id));
}
