import { ChangeDetectionStrategy, Component, computed, inject, input, signal, TemplateRef, viewChild } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
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
import { ToastService } from '../../shared/services/toast.service';
import { CreateShiftCommand, IShiftDto, ShiftDto, ShiftsClient, UpdateShiftCommand } from '../../web-api-client';
import { timeRangeValidator } from '../../shared/validation/date-validators';
import { fromApiTime, toApiTime } from './academic-calendar.logic';

/** A shift as the table shows it, with its campus name. */
interface ShiftRow extends IShiftDto {
  readonly campusName: string;
}

/**
 * The shifts of each campus (morning, evening), with their start and end times. A shift belongs to one campus; its name is
 * unique in that campus.
 */
@Component({
  selector: 'app-shifts-panel',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    AppIconComponent,
    CampusFilterComponent,
    DataTableComponent,
    EmptyStateComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './shifts-panel.component.html',
  styleUrl: './academic-calendar.page.css',
})
export class ShiftsPanelComponent {
  private readonly client = inject(ShiftsClient);
  private readonly formBuilder = inject(FormBuilder);
  private readonly toast = inject(ToastService);
  private readonly dialog = inject(MatDialog);

  /** The school's campuses, loaded once by the page. */
  readonly campuses = input.required<readonly CampusOption[]>();

  readonly shifts = signal<readonly ShiftDto[] | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);
  readonly selectedCampusId = signal<string | null>(null);
  /** The shift whose form is open: 'new', its id, or null. */
  readonly editing = signal<string | null>(null);

  protected readonly rows = computed<readonly ShiftRow[]>(() => {
    const names = new Map(this.campuses().map((campus) => [campus.id, campus.name]));
    const campus = this.selectedCampusId();
    return (this.shifts() ?? [])
      .filter((shift) => campus === null || shift.campusId === campus)
      .map((shift) => ({ ...shift, campusName: names.get(shift.campusId) ?? '' }));
  });

  private readonly actionsCell = viewChild.required<TemplateRef<DataTableCellContext<ShiftRow>>>('actionsCell');

  protected readonly columns = computed<DataTableColumn<ShiftRow>[]>(() => [
    { key: 'campus', header: $localize`:@@academicCalendar.colCampus:Campus`, valueFn: (s) => s.campusName },
    { key: 'name', header: $localize`:@@academicCalendar.colShift:Shift`, valueFn: (s) => s.name },
    { key: 'start', header: $localize`:@@academicCalendar.colStarts:Starts`, valueFn: (s) => fromApiTime(s.startTime) },
    { key: 'end', header: $localize`:@@academicCalendar.colEnds:Ends`, valueFn: (s) => fromApiTime(s.endTime) },
    { key: 'actions', header: $localize`:@@common.actions:Actions`, sortable: false, align: 'right', cell: this.actionsCell() },
  ]);

  readonly form = this.formBuilder.nonNullable.group(
    {
      campusId: ['', Validators.required],
      name: ['', [Validators.required, Validators.maxLength(50)]],
      startTime: ['', Validators.required],
      endTime: ['', Validators.required],
    },
    { validators: [timeRangeValidator('startTime', 'endTime')] }
  );

  constructor() {
    this.load();
  }

  load(): void {
    this.loadError.set(null);
    this.client.getShifts(undefined, undefined).subscribe({
      next: (shifts) => this.shifts.set(shifts),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  startAdd(): void {
    this.errors.set([]);
    const campuses = this.campuses();
    const campusId = this.selectedCampusId() ?? (campuses.length === 1 ? campuses[0].id : '');
    this.form.reset({ campusId, name: '', startTime: '', endTime: '' });
    this.form.controls.campusId.enable();
    this.editing.set('new');
  }

  startEdit(shift: IShiftDto): void {
    this.errors.set([]);
    this.form.reset({
      campusId: shift.campusId,
      name: shift.name,
      startTime: fromApiTime(shift.startTime),
      endTime: fromApiTime(shift.endTime),
    });
    // A shift stays in its campus; to move it, delete it and add it to the other campus.
    this.form.controls.campusId.disable();
    this.editing.set(shift.id);
  }

  cancel(): void {
    this.errors.set([]);
    this.editing.set(null);
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    const editing = this.editing();
    const times = { name: value.name.trim(), startTime: toApiTime(value.startTime), endTime: toApiTime(value.endTime) };
    const request: Observable<unknown> =
      editing === 'new'
        ? this.client.createShift(undefined, new CreateShiftCommand({ campusId: value.campusId, ...times }))
        : this.client.updateShift(undefined, editing ?? '', new UpdateShiftCommand(times));
    this.run(request, $localize`:@@academicCalendar.shiftSaved:Shift saved.`, () => this.editing.set(null));
  }

  confirmDelete(shift: IShiftDto): void {
    const data: ConfirmDialogData = {
      title: $localize`:@@academicCalendar.deleteShiftTitle:Delete this shift?`,
      message: $localize`:@@academicCalendar.deleteShiftMessage:The shift is removed from its campus.`,
      confirmLabel: $localize`:@@academicCalendar.delete:Delete`,
    };
    this.dialog
      .open(ConfirmDialogComponent, { data, autoFocus: 'dialog' })
      .afterClosed()
      .subscribe((confirmed: boolean | undefined) => {
        if (confirmed === true) {
          this.run(this.client.deleteShift(undefined, shift.id), $localize`:@@academicCalendar.shiftDeleted:Shift deleted.`);
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
