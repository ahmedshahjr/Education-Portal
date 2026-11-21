import { ChangeDetectionStrategy, Component, computed, inject, signal, TemplateRef, viewChild } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { ToastService } from '../../shared/services/toast.service';
import { Observable } from 'rxjs';
import { problemMessages } from '../../core/http/problem-details';
import { AppIconComponent } from '../../shared/components/app-icon/app-icon.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { DataTableCellContext, DataTableColumn, DataTableComponent } from '../../shared/components/data-table/data-table.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { FilterOption, FilterPanelComponent } from '../../shared/components/filter-panel/filter-panel.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { UserDetailsClient, UserDetailsDto, UserType } from '../../web-api-client';

/** The type tabs: everyone, or one user type. */
type TypeFilter = 'All' | UserType;

/**
 * The users of the school (principals, teachers and other staff, the Staff module), as a table. The tabs filter by type.
 * Add and edit open the person's own page (`/people/new`, `/people/:id`), since people are large entities (ADR-024). The
 * delete icon asks first. A deleted user is hidden everywhere, and a principal of a campus can be deleted only after
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
  private readonly toast = inject(ToastService);
  private readonly dialog = inject(MatDialog);
  private readonly router = inject(Router);

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

  /** The name as the table shows it. */
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

  /** Opens the person's page: a new person, or the one in the row. */
  protected openDialog(person: UserDetailsDto | null): void {
    this.router.navigate(person ? ['/people', person.id] : ['/people', 'new']);
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
