import { CampusDto, SchoolRoleDto, UserDetailsDto } from '../../web-api-client';
import { PersonChanges } from './person-changes';

/** What the person form needs: the person (or null when adding), and the choices and current links. */
export interface PersonFormData {
  /** The person being changed, or null when a person is added. */
  readonly person: UserDetailsDto | null;
  /** The school's students, to choose a parent's children from. */
  readonly students: readonly UserDetailsDto[];
  /** The school's roles, to choose the account's roles from. */
  readonly roles: readonly SchoolRoleDto[];
  /** The students the parent is linked to now (empty for a new person). */
  readonly currentChildIds: readonly string[];
  /** The roles the person's account holds now (empty for a new person). */
  readonly currentRoleIds: readonly string[];
  /** The school's campuses, to choose where the person works from. */
  readonly campuses: readonly CampusDto[];
  /** The campuses the person is assigned to now (empty for a new person). */
  readonly currentCampusIds: readonly string[];
}

/** What the form saves: the person's fields, the children a parent should have, the roles of the account and the campuses. */
export interface PersonFormResult {
  readonly fields: PersonChanges;
  readonly childIds: readonly string[];
  readonly roleIds: readonly string[];
  readonly campusIds: readonly string[];
}
