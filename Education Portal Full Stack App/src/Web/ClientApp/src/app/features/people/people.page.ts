import { ChangeDetectionStrategy, Component, computed, inject, signal, TemplateRef, viewChild } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { ToastService } from '../../shared/services/toast.service';
import { forkJoin, Observable, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import { problemMessages } from '../../core/http/problem-details';
import { AppIconComponent } from '../../shared/components/app-icon/app-icon.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { DataTableCellContext, DataTableColumn, DataTableComponent } from '../../shared/components/data-table/data-table.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { FilterOption, FilterPanelComponent } from '../../shared/components/filter-panel/filter-panel.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import {
  CreateUserDetailsCommand,
  LinkStudentCommand,
  SchoolRoleDto,
  SchoolRolesClient,
  SetAccountRolesCommand,
  UpdateUserDetailsCommand,
  UserDetailsClient,
  UserDetailsDto,
  UserType,
} from '../../web-api-client';
import { planChildLinks, sameRoles } from './person-changes';
import { PersonDialogComponent, PersonDialogData, PersonDialogResult } from './person-dialog.component';

/** The type tabs: everyone, or one user type. */
type TypeFilter = 'All' | UserType;

/**
 * The users of the school (principals, teachers and other staff, the Staff module), as a table. The tabs filter by type; the
 * icons edit a user or delete it. A deleted user is hidden everywhere, and a principal of a campus can be deleted only after
 * another principal is chosen there (the server refuses it, and the message is shown).
 */
@Component({
  selector: 'app-people-page',
  imports: [
    MatButtonModule,
    MatCardModule,
    PageHeaderComponent,
    EmptyStateComponent,
    StatusBadgeComponent,
    AppIconComponent,
    DataTableComponent,
    FilterPanelComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './people.page.html',
  styleUrl: './people.page.css',
})
export class PeoplePage {
  private readonly peopleClient = inject(UserDetailsClient);
  private readonly rolesClient = inject(SchoolRolesClient);
  private readonly toast = inject(ToastService);
  private readonly dialog = inject(MatDialog);

  protected readonly people = signal<readonly UserDetailsDto[] | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  /** The type tab chosen. */
  protected readonly selectedType = signal<TypeFilter>('All');


  protected readonly typeTabs: readonly FilterOption<TypeFilter>[] = [
    { id: 'All', label: $localize`:@@people.all:All` },
    { id: UserType.Principal, label: $localize`:@@people.principals:Principals` },
    { id: UserType.Teacher, label: $localize`:@@people.teachers:Teachers` },
    { id: UserType.Staff, label: $localize`:@@people.staff:Staff` },
  ];

  /** The people the type tab lets through. */
  protected readonly visiblePeople = computed(() => {
    const type = this.selectedType();
    return (this.people() ?? []).filter((person) => type === 'All' || person.userType === type);
  });

  private readonly nameCell = viewChild.required<TemplateRef<DataTableCellContext<UserDetailsDto>>>('nameCell');
  private readonly typeCell = viewChild.required<TemplateRef<DataTableCellContext<UserDetailsDto>>>('typeCell');
  private readonly actionsCell = viewChild.required<TemplateRef<DataTableCellContext<UserDetailsDto>>>('actionsCell');

  protected readonly columns = computed<DataTableColumn<UserDetailsDto>[]>(() => [
    {
      key: 'name',
      header: $localize`:@@people.colName:Name`,
      valueFn: (p) => this.fullName(p),
      cell: this.nameCell(),
    },
    { key: 'type', header: $localize`:@@people.userType:Type`, valueFn: (p) => p.userType, cell: this.typeCell() },
    { key: 'phone', header: $localize`:@@people.phone:Phone`, valueFn: (p) => p.phone ?? '' },
    { key: 'actions', header: $localize`:@@common.actions:Actions`, sortable: false, align: 'right', cell: this.actionsCell() },
  ]);

  constructor() {
    this.load();
  }

  /** The name as the table and the form show it. */
  protected fullName(person: UserDetailsDto): string {
    return `${person.firstName}${person.middleName ? ' ' + person.middleName : ''} ${person.lastName}`;
  }

  protected load(): void {
    this.loadError.set(null);
    this.people.set(null);
    this.peopleClient.getUserDetails(undefined).subscribe({
      next: (people) => this.people.set(people),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  /**
   * Opens the modal to add a person, or to change one. The modal chooses from the school's students and roles, and starts
   * from the children and roles the person has now. The built-in roles are added to the school first, once it is needed.
   */
  protected openDialog(person: UserDetailsDto | null): void {
    this.errors.set([]);
    this.rolesClient
      .ensureBuiltInRoles()
      .pipe(
        catchError(() => of(undefined)),
        switchMap(() =>
          forkJoin({
            students: this.peopleClient.getUserDetails(UserType.Student).pipe(catchError(() => of([] as UserDetailsDto[]))),
            roles: this.rolesClient.getRoles().pipe(catchError(() => of([] as SchoolRoleDto[]))),
            children: person
              ? this.peopleClient.getChildren(person.id).pipe(catchError(() => of([] as UserDetailsDto[])))
              : of([] as UserDetailsDto[]),
            roleIds: person ? this.peopleClient.getAccountRoles(person.id).pipe(catchError(() => of([] as string[]))) : of([] as string[]),
          })
        )
      )
      .subscribe(({ students, roles, children, roleIds }) => {
        const data: PersonDialogData = {
          person,
          students,
          roles,
          currentChildIds: children.map((child) => child.id),
          currentRoleIds: roleIds,
        };
        this.dialog
          .open<PersonDialogComponent, PersonDialogData, PersonDialogResult>(PersonDialogComponent, { data, width: '640px', autoFocus: 'first-tabbable' })
          .afterClosed()
          .subscribe((result) => {
            if (result) {
              this.save(person, result, data.currentRoleIds, data.currentChildIds);
            }
          });
      });
  }

  /** Saves the person, then the roles of their account and the children of a parent, as the modal chose them. */
  private save(person: UserDetailsDto | null, result: PersonDialogResult, startRoles: readonly string[], startChildren: readonly string[]): void {
    this.busy.set(true);
    this.errors.set([]);

    const saved: Observable<string> = person
      ? this.peopleClient.updateUserDetails(person.id, new UpdateUserDetailsCommand(result.fields)).pipe(map(() => person.id))
      : this.peopleClient.createUserDetails(new CreateUserDetailsCommand(result.fields));

    saved.pipe(switchMap((personId) => this.applyRelations(personId, result, startRoles, startChildren))).subscribe({
      next: () => {
        this.busy.set(false);
        this.toast.success(person ? $localize`:@@people.updated:User saved.` : $localize`:@@people.added:User added.`);
        this.load();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
        // The person may have been saved before the roles or children failed, so the list is shown again.
        this.load();
      },
    });
  }

  /** The roles of the account and the links of a parent, changed only where they differ from what the person had. */
  private applyRelations(
    personId: string,
    result: PersonDialogResult,
    startRoles: readonly string[],
    startChildren: readonly string[]
  ): Observable<unknown> {
    const roles: Observable<unknown> = sameRoles(result.roleIds, startRoles)
      ? of(null)
      : this.peopleClient.setAccountRoles(personId, new SetAccountRolesCommand({ roleIds: [...result.roleIds] }));

    const links = planChildLinks(startChildren, result.childIds);
    const children: Observable<unknown>[] = [
      ...links.add.map((studentId) => this.peopleClient.linkStudent(personId, new LinkStudentCommand({ studentId }))),
      ...links.remove.map((studentId) => this.peopleClient.unlinkStudent(personId, studentId)),
    ];

    return forkJoin([roles, ...children]);
  }

  /** Asks first. The user is hidden everywhere; the server refuses a principal of a campus. */
  protected confirmDelete(person: UserDetailsDto): void {
    const data: ConfirmDialogData = {
      title: $localize`:@@people.deleteTitle:Delete this user?`,
      message: $localize`:@@people.deleteMessage:The user is hidden from the school. A principal of a campus must be replaced there first.`,
      confirmLabel: $localize`:@@people.delete:Delete`,
    };
    this.dialog
      .open(ConfirmDialogComponent, { data, autoFocus: 'dialog' })
      .afterClosed()
      .subscribe((confirmed: boolean | undefined) => {
        if (confirmed === true) {
          this.run(this.peopleClient.deleteUserDetails(person.id), $localize`:@@people.deleted:User deleted.`);
        }
      });
  }

  private run(request: Observable<unknown>, done: string): void {
    this.busy.set(true);
    this.errors.set([]);
    request.subscribe({
      next: () => {
        this.busy.set(false);
        this.toast.success(done);
        this.load();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }
}
