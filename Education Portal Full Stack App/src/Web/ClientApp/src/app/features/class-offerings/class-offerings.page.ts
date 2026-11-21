import { ChangeDetectionStrategy, Component, computed, inject, signal, TemplateRef, viewChild } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Observable } from 'rxjs';
import { problemMessages } from '../../core/http/problem-details';
import { AppIconComponent } from '../../shared/components/app-icon/app-icon.component';
import { CampusFilterComponent, CampusOption } from '../../shared/components/campus-filter/campus-filter.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { DataTableCellContext, DataTableColumn, DataTableComponent } from '../../shared/components/data-table/data-table.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { ToastService } from '../../shared/services/toast.service';
import {
  AdmissionStatus,
  CampusDto,
  CampusesClient,
  ClassLevel,
  ClassOfferingsClient,
  CreateClassOfferingCommand,
  IClassOfferingDto,
  UpdateClassOfferingCommand,
} from '../../web-api-client';
import {
  ADMISSION_STATUSES,
  admissionStatusLabel,
  CLASS_LEVELS,
  classLevelLabel,
} from '../school-page/school-page.labels';
import { ACADEMIC_YEAR_PATTERN, CURRENCY_PATTERN, rowsForCampus } from './class-offerings.logic';

/** A class offering as the table shows it, with its campus name resolved ("All campuses" when it has none). */
interface OfferingRow extends IClassOfferingDto {
  readonly campusName: string;
}

/** The seats left cannot be more than the seats in total. */
function seatsValidator(group: AbstractControl): ValidationErrors | null {
  const total = group.get('seatsTotal')?.value as number | null;
  const available = group.get('seatsAvailable')?.value as number | null;
  return total !== null && available !== null && available > total ? { seats: true } : null;
}

/**
 * The school's class offerings (School module), as a table, with a form to add, change and delete them. When the school has
 * more than one campus, a dropdown above the table filters the rows by campus (AC4.1, AC4.2); the server still decides which
 * offerings the user may see and change.
 */
@Component({
  selector: 'app-class-offerings-page',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    PageHeaderComponent,
    EmptyStateComponent,
    DataTableComponent,
    CampusFilterComponent,
    AppIconComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './class-offerings.page.html',
  styleUrl: './class-offerings.page.css',
})
export class ClassOfferingsPage {
  private readonly offeringsClient = inject(ClassOfferingsClient);
  private readonly campusesClient = inject(CampusesClient);
  private readonly toast = inject(ToastService);
  private readonly dialog = inject(MatDialog);

  protected readonly levels = CLASS_LEVELS;
  protected readonly levelLabel = classLevelLabel;
  protected readonly statuses = ADMISSION_STATUSES;
  protected readonly statusLabel = admissionStatusLabel;

  protected readonly offerings = signal<readonly OfferingRow[] | null>(null);
  protected readonly campuses = signal<readonly CampusDto[]>([]);
  protected readonly loadError = signal<string | null>(null);

  /** The campus the list is filtered to, or null for all campuses. */
  protected readonly selectedCampusId = signal<string | null>(null);

  /** The offering whose form is open: 'new', its id, or null. */
  protected readonly editing = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  protected readonly campusOptions = computed<readonly CampusOption[]>(() =>
    this.campuses().map((campus) => ({ id: campus.id, name: campus.name }))
  );

  protected readonly visibleOfferings = computed(() =>
    rowsForCampus(this.offerings() ?? [], this.selectedCampusId())
  );

  private readonly actionsCell = viewChild.required<TemplateRef<DataTableCellContext<OfferingRow>>>('actionsCell');

  protected readonly columns = computed<DataTableColumn<OfferingRow>[]>(() => [
    { key: 'campus', header: $localize`:@@classOfferings.colCampus:Campus`, valueFn: (o) => o.campusName },
    { key: 'level', header: $localize`:@@classOfferings.colLevel:Class`, valueFn: (o) => classLevelLabel(o.level) },
    { key: 'year', header: $localize`:@@classOfferings.colYear:Academic year`, valueFn: (o) => o.academicYear },
    { key: 'fee', header: $localize`:@@classOfferings.colFee:Monthly fee`, valueFn: (o) => `${o.currencyCode} ${o.monthlyFee}` },
    { key: 'seats', header: $localize`:@@classOfferings.colSeats:Seats left`, valueFn: (o) => `${o.seatsAvailable} / ${o.seatsTotal}` },
    { key: 'status', header: $localize`:@@classOfferings.colStatus:Admissions`, valueFn: (o) => admissionStatusLabel(o.status) },
    { key: 'actions', header: $localize`:@@common.actions:Actions`, sortable: false, align: 'right', cell: this.actionsCell() },
  ]);

  protected readonly form = inject(FormBuilder).group(
    {
      campusId: [null as string | null],
      level: [ClassLevel.Class1 as ClassLevel, Validators.required],
      academicYear: ['', [Validators.required, Validators.pattern(ACADEMIC_YEAR_PATTERN)]],
      monthlyFee: [0, [Validators.required, Validators.min(0)]],
      admissionFee: [null as number | null, Validators.min(0)],
      currencyCode: ['', [Validators.required, Validators.pattern(CURRENCY_PATTERN)]],
      seatsTotal: [0, [Validators.required, Validators.min(0)]],
      seatsAvailable: [0, [Validators.required, Validators.min(0)]],
      status: [AdmissionStatus.Open as AdmissionStatus, Validators.required],
      showFee: [true],
    },
    { validators: [seatsValidator] }
  );

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loadError.set(null);
    this.offerings.set(null);
    this.campusesClient.getCampuses().subscribe({
      next: (campuses) => {
        this.campuses.set(campuses);
        this.loadOfferings(campuses);
      },
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  private loadOfferings(campuses: readonly CampusDto[]): void {
    const names = new Map(campuses.map((campus) => [campus.id, campus.name]));
    this.offeringsClient.getClassOfferings(undefined).subscribe({
      next: (rows) =>
        this.offerings.set(
          rows.map((row) => ({
            ...row,
            campusName: row.campusId ? (names.get(row.campusId) ?? '') : $localize`:@@classOfferings.allCampuses:All campuses`,
          }))
        ),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  protected startAdd(): void {
    this.errors.set([]);
    this.form.reset({
      campusId: this.selectedCampusId(),
      level: ClassLevel.Class1,
      academicYear: '',
      monthlyFee: 0,
      admissionFee: null,
      currencyCode: '',
      seatsTotal: 0,
      seatsAvailable: 0,
      status: AdmissionStatus.Open,
      showFee: true,
    });
    this.form.controls.campusId.enable();
    this.form.controls.level.enable();
    this.editing.set('new');
  }

  protected startEdit(offering: IClassOfferingDto): void {
    this.errors.set([]);
    this.form.reset({
      campusId: offering.campusId ?? null,
      level: offering.level,
      academicYear: offering.academicYear,
      monthlyFee: offering.monthlyFee,
      admissionFee: offering.admissionFee ?? null,
      currencyCode: offering.currencyCode,
      seatsTotal: offering.seatsTotal,
      seatsAvailable: offering.seatsAvailable,
      status: offering.status,
      showFee: offering.showFee,
    });
    // An offering stays in its campus and class; to change those, delete it and add it again.
    this.form.controls.campusId.disable();
    this.form.controls.level.disable();
    this.editing.set(offering.id);
  }

  protected cancel(): void {
    this.errors.set([]);
    this.editing.set(null);
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const editing = this.editing();
    const details = {
      academicYear: value.academicYear?.trim() ?? '',
      monthlyFee: value.monthlyFee ?? 0,
      admissionFee: value.admissionFee ?? undefined,
      currencyCode: (value.currencyCode ?? '').trim().toUpperCase(),
      seatsTotal: value.seatsTotal ?? 0,
      seatsAvailable: value.seatsAvailable ?? 0,
      status: value.status ?? AdmissionStatus.Open,
      showFee: value.showFee ?? true,
    };
    const request: Observable<unknown> =
      editing === 'new'
        ? this.offeringsClient.createClassOffering(
            undefined,
            new CreateClassOfferingCommand({ campusId: value.campusId ?? undefined, level: value.level ?? ClassLevel.Class1, ...details })
          )
        : this.offeringsClient.updateClassOffering(undefined, editing ?? '', new UpdateClassOfferingCommand(details));
    this.run(request, $localize`:@@classOfferings.saved:Class offering saved.`, () => this.editing.set(null));
  }

  protected confirmDelete(offering: IClassOfferingDto): void {
    const data: ConfirmDialogData = {
      title: $localize`:@@classOfferings.deleteTitle:Delete this class offering?`,
      message: $localize`:@@classOfferings.deleteMessage:It is removed from the school's public page.`,
      confirmLabel: $localize`:@@classOfferings.delete:Delete`,
    };
    this.dialog
      .open(ConfirmDialogComponent, { data, autoFocus: 'dialog' })
      .afterClosed()
      .subscribe((confirmed: boolean | undefined) => {
        if (confirmed === true) {
          this.run(this.offeringsClient.deleteClassOffering(undefined, offering.id), $localize`:@@classOfferings.deleted:Class offering deleted.`);
        }
      });
  }

  private run(request: Observable<unknown>, done: string, after?: () => void): void {
    this.busy.set(true);
    this.errors.set([]);
    request.subscribe({
      next: () => {
        this.busy.set(false);
        this.toast.success(done);
        after?.();
        this.load();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }
}
