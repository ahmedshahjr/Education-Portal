import { ChangeDetectionStrategy, Component, computed, inject, signal, TemplateRef, viewChild } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
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
import {
  CreateUserDetailsCommand,
  UpdateUserDetailsCommand,
  UserDetailsClient,
  UserDetailsDto,
  UserType,
} from '../../web-api-client';

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
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
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
  private readonly formBuilder = inject(FormBuilder);
  private readonly toast = inject(ToastService);
  private readonly dialog = inject(MatDialog);

  protected readonly people = signal<readonly UserDetailsDto[] | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  /** The user being edited, or null. */
  protected readonly editingId = signal<string | null>(null);

  /** True while the add form is open. */
  protected readonly adding = signal(false);

  /** The type tab chosen. */
  protected readonly selectedType = signal<TypeFilter>('All');

  protected readonly types = [UserType.Principal, UserType.Teacher, UserType.Staff];

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

  /** The form is open for adding or for editing one user. */
  protected readonly formOpen = computed(() => this.adding() || this.editingId() !== null);

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

  protected readonly form = this.formBuilder.nonNullable.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    middleName: ['', [Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    userType: [UserType.Teacher as UserType, [Validators.required]],
    phone: ['', [Validators.maxLength(30)]],
  });

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

  protected startAdd(): void {
    this.errors.set([]);
    this.editingId.set(null);
    this.form.reset({ firstName: '', middleName: '', lastName: '', userType: UserType.Teacher, phone: '' });
    this.adding.set(true);
  }

  protected startEdit(person: UserDetailsDto): void {
    this.errors.set([]);
    this.adding.set(false);
    this.editingId.set(person.id);
    this.form.reset({
      firstName: person.firstName,
      middleName: person.middleName ?? '',
      lastName: person.lastName,
      userType: person.userType,
      phone: person.phone ?? '',
    });
  }

  protected cancel(): void {
    this.errors.set([]);
    this.adding.set(false);
    this.editingId.set(null);
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const editing = this.editingId();
    const value = this.form.getRawValue();
    const fields = {
      firstName: value.firstName.trim(),
      middleName: value.middleName.trim() || undefined,
      lastName: value.lastName.trim(),
      userType: value.userType,
      phone: value.phone.trim() || undefined,
    };

    this.busy.set(true);
    this.errors.set([]);
    const request: Observable<unknown> = editing
      ? this.peopleClient.updateUserDetails(editing, new UpdateUserDetailsCommand(fields))
      : this.peopleClient.createUserDetails(new CreateUserDetailsCommand(fields));

    request.subscribe({
      next: () => {
        this.busy.set(false);
        this.toast.success(editing ? $localize`:@@people.updated:User saved.` : $localize`:@@people.added:User added.`);
        this.cancel();
        this.load();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
      },
    });
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
