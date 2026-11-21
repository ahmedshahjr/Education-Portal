import { inject, Injectable } from '@angular/core';
import { forkJoin, Observable, of } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
import {
  CreateUserDetailsCommand,
  LinkStudentCommand,
  SavePersonAccessCommand,
  UpdateUserDetailsCommand,
  UserDetailsClient,
  UserDetailsDto,
} from '../../web-api-client';
import { PersonFormData, PersonFormResult } from './person-form.models';
import { planChildLinks, sameIds } from './person-changes';

/**
 * Saves a person: the person's fields first, then their roles and campuses in one call (R3.2, so they change together or
 * not at all), then the children of a parent, changed only where they differ from what the person had.
 */
@Injectable({ providedIn: 'root' })
export class PersonSaverService {
  private readonly peopleClient = inject(UserDetailsClient);

  save(person: UserDetailsDto | null, result: PersonFormResult, start: PersonFormData): Observable<string> {
    const saved: Observable<string> = person
      ? this.peopleClient.updateUserDetails(person.id, new UpdateUserDetailsCommand(result.fields)).pipe(map(() => person.id))
      : this.peopleClient.createUserDetails(new CreateUserDetailsCommand(result.fields));

    return saved.pipe(
      switchMap((personId) => this.applyRelations(personId, result, start).pipe(map(() => personId)))
    );
  }

  private applyRelations(personId: string, result: PersonFormResult, start: PersonFormData): Observable<unknown> {
    const accessChanged = !sameIds(result.roleIds, start.currentRoleIds) || !sameIds(result.campusIds, start.currentCampusIds);
    const access: Observable<unknown> = accessChanged
      ? this.peopleClient.savePersonAccess(personId, new SavePersonAccessCommand({ roleIds: [...result.roleIds], campusIds: [...result.campusIds] }))
      : of(null);

    const links = planChildLinks(start.currentChildIds, result.childIds);
    const children: Observable<unknown>[] = [
      ...links.add.map((studentId) => this.peopleClient.linkStudent(personId, new LinkStudentCommand({ studentId }))),
      ...links.remove.map((studentId) => this.peopleClient.unlinkStudent(personId, studentId)),
    ];

    return forkJoin([access, ...children]);
  }
}
