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
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { DataTableCellContext, DataTableColumn, DataTableComponent } from '../../shared/components/data-table/data-table.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import {
  ApproveSchoolRequestCommand,
  CountriesClient,
  CountryDto,
  DataRegion,
  SchoolRequestDto,
  SchoolRequestStatus,
  SchoolRequestsClient,
} from '../../web-api-client';

/**
 * The master tenant's queue of school requests, as a table. A pending request is approved by giving the school its domain,
 * country, first administrator and a one-time password: the school is registered with its 3-month (90-day) free trial. A request can
 * also be rejected, which creates nothing. The approve form opens below the table, for the request chosen.
 */
@Component({
  selector: 'app-school-requests-queue-page',
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
    DataTableComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './school-requests-queue.page.html',
  styleUrl: './school-requests-queue.page.css',
})
export class SchoolRequestsQueuePage {
  private readonly requestsClient = inject(SchoolRequestsClient);
  private readonly countriesClient = inject(CountriesClient);
  private readonly formBuilder = inject(FormBuilder);
  private readonly toast = inject(ToastService);
  private readonly dialog = inject(MatDialog);

  protected readonly requests = signal<readonly SchoolRequestDto[] | null>(null);
  protected readonly countries = signal<readonly CountryDto[]>([]);
  protected readonly loadError = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  /** The request being approved, or null. */
  protected readonly approvingId = signal<string | null>(null);

  protected readonly statuses = SchoolRequestStatus;
  protected readonly regions = [DataRegion.SouthAsia, DataRegion.MiddleEast, DataRegion.NorthAmerica];

  /** The school being approved, for the form's heading. */
  protected readonly approving = computed(() => (this.requests() ?? []).find((r) => r.id === this.approvingId()) ?? null);

  protected readonly form = this.formBuilder.nonNullable.group({
    domain: ['', [Validators.required, Validators.maxLength(253)]],
    countryId: ['', [Validators.required]],
    dataRegion: [''],
    administratorEmail: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    administratorPassword: ['', [Validators.required, Validators.minLength(15), Validators.maxLength(128)]],
  });

  private readonly statusCell = viewChild.required<TemplateRef<DataTableCellContext<SchoolRequestDto>>>('statusCell');
  private readonly actionsCell = viewChild.required<TemplateRef<DataTableCellContext<SchoolRequestDto>>>('actionsCell');

  protected readonly columns = computed<DataTableColumn<SchoolRequestDto>[]>(() => [
    { key: 'schoolName', header: $localize`:@@requests.colSchool:School`, valueFn: (r) => r.schoolName },
    { key: 'contactName', header: $localize`:@@requests.colContact:Contact`, valueFn: (r) => `${r.contactName} ${r.contactEmail}` },
    { key: 'status', header: $localize`:@@requests.colStatus:Status`, valueFn: (r) => r.status, cell: this.statusCell() },
    { key: 'actions', header: $localize`:@@common.actions:Actions`, sortable: false, align: 'right', cell: this.actionsCell() },
  ]);

  constructor() {
    this.load();
    this.countriesClient.getCountries(1, 100).subscribe({
      next: (page) => this.countries.set(page.items),
      error: () => this.countries.set([]),
    });
  }

  protected load(): void {
    this.loadError.set(null);
    this.requests.set(null);
    this.requestsClient.getSchoolRequests().subscribe({
      next: (requests) => this.requests.set(requests),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  protected startApprove(request: SchoolRequestDto): void {
    this.errors.set([]);
    this.approvingId.set(request.id);
    this.form.reset({ domain: '', countryId: '', dataRegion: '', administratorEmail: '', administratorPassword: '' });
  }

  protected cancelApprove(): void {
    this.approvingId.set(null);
    this.errors.set([]);
  }

  protected approve(request: SchoolRequestDto): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.busy.set(true);
    this.errors.set([]);
    this.requestsClient
      .approveSchoolRequest(
        request.id,
        new ApproveSchoolRequestCommand({
          domain: value.domain.trim().toLowerCase(),
          countryId: value.countryId,
          // The select holds the enum's text value; the command takes the generated enum type, which has the same values.
          dataRegion: (value.dataRegion || undefined) as unknown as ApproveSchoolRequestCommand['dataRegion'],
          administratorEmail: value.administratorEmail.trim(),
          administratorPassword: value.administratorPassword,
        })
      )
      .subscribe({
        next: () => {
          this.busy.set(false);
          this.toast.success($localize`:@@requests.approved:School approved. It has a 3-month (90-day) free trial.`);
          this.approvingId.set(null);
          this.load();
        },
        error: (error: unknown) => {
          this.busy.set(false);
          this.errors.set(problemMessages(error));
        },
      });
  }

  /** Asks first: a rejected request creates no school and no account. */
  protected confirmReject(request: SchoolRequestDto): void {
    const data: ConfirmDialogData = {
      title: $localize`:@@requests.rejectTitle:Reject this request?`,
      message: $localize`:@@requests.rejectMessage:No school and no account is created for it.`,
      confirmLabel: $localize`:@@requests.reject:Reject`,
    };
    this.dialog
      .open(ConfirmDialogComponent, { data, autoFocus: 'dialog' })
      .afterClosed()
      .subscribe((confirmed: boolean | undefined) => {
        if (confirmed === true) {
          this.run(this.requestsClient.rejectSchoolRequest(request.id), $localize`:@@requests.rejected:Request rejected.`);
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
