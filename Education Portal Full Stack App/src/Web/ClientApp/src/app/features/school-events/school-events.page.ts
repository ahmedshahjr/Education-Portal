import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { LocalDatePipe } from '../../core/i18n/local-date.pipe';
import { problemMessages } from '../../core/http/problem-details';
import { DataTableColumn, DataTableComponent } from '../../shared/components/data-table/data-table.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { ToastService } from '../../shared/services/toast.service';
import { CreateSchoolEventCommand, EventAudience, SchoolEventDto, SchoolEventsClient } from '../../web-api-client';
import {
  audienceLabel,
  endsAfterStart,
  EVENT_AUDIENCES,
  EVENT_DESCRIPTION_MAX_LENGTH,
  EVENT_TITLE_MAX_LENGTH,
  fromDateInput,
  monthRange,
  toDateInput,
} from './school-events.logic';

/** An event as the table shows it. */
interface EventRow {
  readonly id: string;
  readonly title: string;
  readonly starts: string;
  readonly ends: string;
  readonly audience: string;
}

/** The event must end after it starts. */
function endsAfterStartValidator(group: AbstractControl): ValidationErrors | null {
  const startsAt = group.get('startsAt')?.value as string;
  const endsAt = group.get('endsAt')?.value as string;
  return startsAt && endsAt && !endsAfterStart(startsAt, endsAt) ? { endsBeforeStart: true } : null;
}

/**
 * The school's events (School module): what is on in a range of dates, and a form to add an event for everyone, staff, parents
 * or students. Times are shown in the user's time zone and stored in UTC.
 */
@Component({
  selector: 'app-school-events-page',
  imports: [ReactiveFormsModule, MatButtonModule, MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule, DataTableComponent, PageHeaderComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './school-events.page.html',
  styleUrl: '../academics/academics.css',
})
export class SchoolEventsPage implements OnInit {
  private readonly client = inject(SchoolEventsClient);
  private readonly toast = inject(ToastService);
  private readonly dates = new LocalDatePipe();

  protected readonly audiences = EVENT_AUDIENCES;
  protected readonly audienceLabel = audienceLabel;
  protected readonly titleMax = EVENT_TITLE_MAX_LENGTH;
  protected readonly descriptionMax = EVENT_DESCRIPTION_MAX_LENGTH;

  protected readonly events = signal<readonly SchoolEventDto[] | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly adding = signal(false);
  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  protected readonly range = inject(FormBuilder).nonNullable.group({
    from: [toDateInput(monthRange(new Date()).from), Validators.required],
    to: [toDateInput(monthRange(new Date()).to), Validators.required],
  });

  protected readonly form = inject(FormBuilder).nonNullable.group(
    {
      title: ['', [Validators.required, Validators.maxLength(EVENT_TITLE_MAX_LENGTH)]],
      description: ['', Validators.maxLength(EVENT_DESCRIPTION_MAX_LENGTH)],
      startsAt: ['', Validators.required],
      endsAt: ['', Validators.required],
      audience: [EventAudience.All as EventAudience, Validators.required],
    },
    { validators: [endsAfterStartValidator] }
  );

  protected readonly rows = computed<readonly EventRow[]>(() =>
    [...(this.events() ?? [])]
      .sort((a, b) => a.startsAt.getTime() - b.startsAt.getTime())
      .map((event) => ({
        id: event.id,
        title: event.title,
        starts: this.dates.transform(event.startsAt, 'dateTime'),
        ends: this.dates.transform(event.endsAt, 'dateTime'),
        audience: audienceLabel(event.audience),
      }))
  );

  protected readonly columns: DataTableColumn<EventRow>[] = [
    { key: 'title', header: $localize`:@@events.colTitle:Event`, valueFn: (e) => e.title },
    { key: 'starts', header: $localize`:@@events.colStarts:Starts`, valueFn: (e) => e.starts },
    { key: 'ends', header: $localize`:@@events.colEnds:Ends`, valueFn: (e) => e.ends },
    { key: 'audience', header: $localize`:@@events.colAudience:For`, valueFn: (e) => e.audience },
  ];

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    const from = fromDateInput(this.range.controls.from.value);
    const to = fromDateInput(this.range.controls.to.value);
    if (from === null || to === null || to < from) {
      this.loadError.set($localize`:@@events.rangeInvalid:Choose a start date and an end date that is not before it.`);
      return;
    }

    this.loadError.set(null);
    this.events.set(null);
    this.client.getCalendar(from, to).subscribe({
      next: (events) => this.events.set(events),
      error: (error: unknown) => {
        this.events.set([]);
        this.loadError.set(problemMessages(error)[0]);
      },
    });
  }

  protected startAdd(): void {
    this.errors.set([]);
    this.form.reset({ title: '', description: '', startsAt: '', endsAt: '', audience: EventAudience.All });
    this.adding.set(true);
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.busy.set(true);
    this.errors.set([]);
    this.client
      .createSchoolEvent(
        new CreateSchoolEventCommand({
          title: value.title.trim(),
          description: value.description.trim() || undefined,
          startsAt: new Date(value.startsAt),
          endsAt: new Date(value.endsAt),
          audience: value.audience,
        })
      )
      .subscribe({
        next: () => {
          this.busy.set(false);
          this.adding.set(false);
          this.toast.success($localize`:@@events.saved:Event added.`);
          this.load();
        },
        error: (error: unknown) => {
          this.busy.set(false);
          this.errors.set(problemMessages(error));
        },
      });
  }
}
