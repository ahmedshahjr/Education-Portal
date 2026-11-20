import { ChangeDetectionStrategy, Component, computed, inject, signal, TemplateRef, viewChild } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Observable } from 'rxjs';
import { problemMessages } from '../../core/http/problem-details';
import { CalendarDatePipe } from '../../core/i18n/calendar-date.pipe';
import { AppIconComponent } from '../../shared/components/app-icon/app-icon.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { DataTableCellContext, DataTableColumn, DataTableComponent } from '../../shared/components/data-table/data-table.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { ToastService } from '../../shared/services/toast.service';
import {
  AcademicYearDto,
  AcademicYearsClient,
  AcademicYearStatus,
  CreateAcademicYearCommand,
  CreateTermCommand,
  TermDto,
  UpdateAcademicYearCommand,
  UpdateTermCommand,
} from '../../web-api-client';
import { labelOf, yearStatusOptions } from './academic-calendar.labels';
import { fromApiDate } from './academic-calendar.logic';

/** A year as the table and forms use it: its dates already moved to local calendar days. */
export interface YearRow {
  readonly id: string;
  readonly name: string;
  readonly startDate: Date;
  readonly endDate: Date;
  readonly isCurrent: boolean;
  readonly status: AcademicYearStatus;
  readonly terms: readonly TermRow[];
}

export interface TermRow {
  readonly id: string;
  readonly name: string;
  readonly sequence: number;
  readonly startDate: Date;
  readonly endDate: Date;
}

function toYearRow(year: AcademicYearDto): YearRow {
  return {
    id: year.id,
    name: year.name,
    startDate: fromApiDate(year.startDate),
    endDate: fromApiDate(year.endDate),
    isCurrent: year.isCurrent,
    status: year.status,
    terms: [...year.terms]
      .sort((a, b) => a.sequence - b.sequence)
      .map((term: TermDto) => ({
        id: term.id,
        name: term.name,
        sequence: term.sequence,
        startDate: fromApiDate(term.startDate),
        endDate: fromApiDate(term.endDate),
      })),
  };
}

/**
 * The school's academic years and their terms. One year is the current one; a closed year can no longer be changed. A row
 * opens the year's terms below the table. The server checks every rule again (no overlapping years or terms, a term inside
 * its year, one current year).
 */
@Component({
  selector: 'app-academic-years-panel',
  imports: [
    CalendarDatePipe,
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatCheckboxModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    AppIconComponent,
    DataTableComponent,
    EmptyStateComponent,
    StatusBadgeComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './academic-years-panel.component.html',
  styleUrl: './academic-calendar.page.css',
})
export class AcademicYearsPanelComponent {
  private readonly client = inject(AcademicYearsClient);
  private readonly formBuilder = inject(FormBuilder);
  private readonly toast = inject(ToastService);
  private readonly dialog = inject(MatDialog);

  readonly years = signal<readonly YearRow[] | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  /** The year whose form is open: 'new' for a new year, its id when editing, or null. */
  readonly editingYear = signal<string | null>(null);
  /** The year whose terms are shown below the table. */
  readonly selectedYearId = signal<string | null>(null);
  /** The term whose form is open: 'new', its id, or null. */
  readonly editingTerm = signal<string | null>(null);

  protected readonly statuses = yearStatusOptions();
  protected readonly closed = AcademicYearStatus.Closed;

  readonly selectedYear = computed(() => this.years()?.find((year) => year.id === this.selectedYearId()) ?? null);

  private readonly nameCell = viewChild.required<TemplateRef<DataTableCellContext<YearRow>>>('nameCell');
  private readonly statusCell = viewChild.required<TemplateRef<DataTableCellContext<YearRow>>>('statusCell');
  private readonly actionsCell = viewChild.required<TemplateRef<DataTableCellContext<YearRow>>>('actionsCell');

  protected readonly columns = computed<DataTableColumn<YearRow>[]>(() => [
    { key: 'name', header: $localize`:@@academicCalendar.colYear:Academic year`, valueFn: (y) => y.name, cell: this.nameCell() },
    { key: 'terms', header: $localize`:@@academicCalendar.colTerms:Terms`, valueFn: (y) => y.terms.length },
    { key: 'status', header: $localize`:@@academicCalendar.colStatus:Status`, valueFn: (y) => this.statusLabel(y.status), cell: this.statusCell() },
    { key: 'actions', header: $localize`:@@common.actions:Actions`, sortable: false, align: 'right', cell: this.actionsCell() },
  ]);

  readonly yearForm = this.formBuilder.group({
    name: this.formBuilder.nonNullable.control('', [Validators.required, Validators.maxLength(20)]),
    startDate: this.formBuilder.control<Date | null>(null, Validators.required),
    endDate: this.formBuilder.control<Date | null>(null, Validators.required),
    isCurrent: this.formBuilder.nonNullable.control(false),
    status: this.formBuilder.nonNullable.control(AcademicYearStatus.Planned),
  });

  readonly termForm = this.formBuilder.group({
    name: this.formBuilder.nonNullable.control('', [Validators.required, Validators.maxLength(50)]),
    sequence: this.formBuilder.nonNullable.control(1, [Validators.required, Validators.min(1), Validators.max(12)]),
    startDate: this.formBuilder.control<Date | null>(null, Validators.required),
    endDate: this.formBuilder.control<Date | null>(null, Validators.required),
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loadError.set(null);
    this.client.getAcademicYears(undefined).subscribe({
      next: (years) => this.years.set(years.map(toYearRow)),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  protected statusLabel(status: AcademicYearStatus): string {
    return labelOf(this.statuses, status);
  }

  selectYear(year: YearRow): void {
    this.selectedYearId.set(year.id);
    this.editingTerm.set(null);
  }

  startAddYear(): void {
    this.errors.set([]);
    this.yearForm.reset({ name: '', startDate: null, endDate: null, isCurrent: false, status: AcademicYearStatus.Planned });
    this.editingYear.set('new');
  }

  startEditYear(year: YearRow): void {
    this.errors.set([]);
    this.yearForm.reset({
      name: year.name,
      startDate: year.startDate,
      endDate: year.endDate,
      isCurrent: year.isCurrent,
      status: year.status,
    });
    this.editingYear.set(year.id);
  }

  cancelYear(): void {
    this.errors.set([]);
    this.editingYear.set(null);
  }

  saveYear(): void {
    const value = this.yearForm.getRawValue();
    if (this.yearForm.invalid || value.startDate === null || value.endDate === null) {
      this.yearForm.markAllAsTouched();
      return;
    }
    const editing = this.editingYear();
    const request: Observable<unknown> =
      editing === 'new'
        ? this.client.createAcademicYear(
            undefined,
            new CreateAcademicYearCommand({
              name: value.name.trim(),
              startDate: value.startDate,
              endDate: value.endDate,
              isCurrent: value.isCurrent,
            })
          )
        : this.client.updateAcademicYear(
            undefined,
            editing ?? '',
            new UpdateAcademicYearCommand({
              name: value.name.trim(),
              startDate: value.startDate,
              endDate: value.endDate,
              status: value.status,
            })
          );
    this.run(request, $localize`:@@academicCalendar.yearSaved:Academic year saved.`, () => this.editingYear.set(null));
  }

  makeCurrent(year: YearRow): void {
    this.run(
      this.client.setCurrentAcademicYear(undefined, year.id),
      $localize`:@@academicCalendar.yearMadeCurrent:This is now the current academic year.`
    );
  }

  confirmDeleteYear(year: YearRow): void {
    const data: ConfirmDialogData = {
      title: $localize`:@@academicCalendar.deleteYearTitle:Delete this academic year?`,
      message: $localize`:@@academicCalendar.deleteYearMessage:A year that still has terms cannot be deleted; delete its terms first.`,
      confirmLabel: $localize`:@@academicCalendar.delete:Delete`,
    };
    this.confirm(data, () =>
      this.run(this.client.deleteAcademicYear(undefined, year.id), $localize`:@@academicCalendar.yearDeleted:Academic year deleted.`, () => {
        if (this.selectedYearId() === year.id) {
          this.selectedYearId.set(null);
        }
      })
    );
  }

  startAddTerm(): void {
    const year = this.selectedYear();
    if (!year) {
      return;
    }
    this.errors.set([]);
    const next = year.terms.length === 0 ? 1 : Math.min(12, Math.max(...year.terms.map((t) => t.sequence)) + 1);
    this.termForm.reset({ name: '', sequence: next, startDate: null, endDate: null });
    this.editingTerm.set('new');
  }

  startEditTerm(term: TermRow): void {
    this.errors.set([]);
    this.termForm.reset({ name: term.name, sequence: term.sequence, startDate: term.startDate, endDate: term.endDate });
    this.editingTerm.set(term.id);
  }

  cancelTerm(): void {
    this.errors.set([]);
    this.editingTerm.set(null);
  }

  saveTerm(): void {
    const year = this.selectedYear();
    const value = this.termForm.getRawValue();
    if (!year || this.termForm.invalid || value.startDate === null || value.endDate === null) {
      this.termForm.markAllAsTouched();
      return;
    }
    const editing = this.editingTerm();
    const body = { name: value.name.trim(), sequence: value.sequence, startDate: value.startDate, endDate: value.endDate };
    const request: Observable<unknown> =
      editing === 'new'
        ? this.client.createTerm(undefined, year.id, new CreateTermCommand(body))
        : this.client.updateTerm(undefined, editing ?? '', new UpdateTermCommand(body));
    this.run(request, $localize`:@@academicCalendar.termSaved:Term saved.`, () => this.editingTerm.set(null));
  }

  confirmDeleteTerm(term: TermRow): void {
    const data: ConfirmDialogData = {
      title: $localize`:@@academicCalendar.deleteTermTitle:Delete this term?`,
      message: $localize`:@@academicCalendar.deleteTermMessage:The term is removed from the academic year.`,
      confirmLabel: $localize`:@@academicCalendar.delete:Delete`,
    };
    this.confirm(data, () =>
      this.run(this.client.deleteTerm(undefined, term.id), $localize`:@@academicCalendar.termDeleted:Term deleted.`)
    );
  }

  private confirm(data: ConfirmDialogData, then: () => void): void {
    this.dialog
      .open(ConfirmDialogComponent, { data, autoFocus: 'dialog' })
      .afterClosed()
      .subscribe((confirmed: boolean | undefined) => {
        if (confirmed === true) {
          then();
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
