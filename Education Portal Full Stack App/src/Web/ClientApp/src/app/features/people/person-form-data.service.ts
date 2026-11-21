import { inject, Injectable } from '@angular/core';
import { forkJoin, Observable, of } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { CampusDto, CampusesClient, SchoolRoleDto, SchoolRolesClient, UserDetailsClient, UserDetailsDto, UserType } from '../../web-api-client';
import { PersonFormData } from './person-form.models';

/** Loads what the person page needs: the school's students, roles and campuses, and the person's own links. */
@Injectable({ providedIn: 'root' })
export class PersonFormDataService {
  private readonly peopleClient = inject(UserDetailsClient);
  private readonly rolesClient = inject(SchoolRolesClient);
  private readonly campusesClient = inject(CampusesClient);

  /** The data for a person, or for a new one when `person` is null. The built-in roles are added to the school first. */
  load(person: UserDetailsDto | null): Observable<PersonFormData> {
    return this.rolesClient.ensureBuiltInRoles().pipe(
      catchError(() => of(undefined)),
      switchMap(() =>
        forkJoin({
          students: this.peopleClient.getUserDetails(UserType.Student).pipe(catchError(() => of([] as UserDetailsDto[]))),
          roles: this.rolesClient.getRoles().pipe(catchError(() => of([] as SchoolRoleDto[]))),
          children: person
            ? this.peopleClient.getChildren(person.id).pipe(catchError(() => of([] as UserDetailsDto[])))
            : of([] as UserDetailsDto[]),
          roleIds: person ? this.peopleClient.getAccountRoles(person.id).pipe(catchError(() => of([] as string[]))) : of([] as string[]),
          campuses: this.campusesClient.getCampuses().pipe(catchError(() => of([] as CampusDto[]))),
          campusIds: person ? this.peopleClient.getPersonCampuses(person.id).pipe(catchError(() => of([] as string[]))) : of([] as string[]),
        })
      ),
      switchMap(({ students, roles, children, roleIds, campuses, campusIds }) =>
        of<PersonFormData>({
          person,
          students,
          roles,
          currentChildIds: children.map((child) => child.id),
          currentRoleIds: roleIds,
          campuses,
          currentCampusIds: campusIds,
        })
      )
    );
  }
}
