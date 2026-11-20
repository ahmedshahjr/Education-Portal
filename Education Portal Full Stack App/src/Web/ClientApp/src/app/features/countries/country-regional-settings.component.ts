import { formatDate } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, inject, input, LOCALE_ID, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { problemMessages } from '../../core/http/problem-details';
import { ToastService } from '../../shared/services/toast.service';
import { CountriesClient, CountryDto, DayOfWeek, UpdateCountryRegionalSettingsCommand } from '../../web-api-client';

/** The days of the week from Sunday, matching .NET's DayOfWeek (0 = Sunday). */
const DAYS: readonly DayOfWeek[] = [
  DayOfWeek.Sunday,
  DayOfWeek.Monday,
  DayOfWeek.Tuesday,
  DayOfWeek.Wednesday,
  DayOfWeek.Thursday,
  DayOfWeek.Friday,
  DayOfWeek.Saturday,
];

/** The most weekend days a country can have (the server enforces the same). */
export const MAX_WEEKEND_DAYS = 3;

/**
 * A country's regional settings (master portal): how many decimals its currency shows, an optional pattern phone numbers
 * must match, the first day of the week and the weekend days. Schools of the country use them in their calendar.
 */
@Component({
  selector: 'app-country-regional-settings',
  imports: [ReactiveFormsModule, MatButtonModule, MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './country-regional-settings.component.html',
  styleUrl: './country-regional-settings.component.css',
})
export class CountryRegionalSettingsComponent {
  private readonly client = inject(CountriesClient);
  private readonly formBuilder = inject(FormBuilder);
  private readonly toast = inject(ToastService);
  private readonly locale = inject(LOCALE_ID);

  readonly country = input.required<CountryDto>();
  readonly saved = output<void>();

  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  /** Each day with its name in the page's language (2000-01-02 was a Sunday). */
  protected readonly days = DAYS.map((day, index) => ({
    value: day,
    label: formatDate(new Date(2000, 0, 2 + index), 'EEEE', this.locale),
  }));
  protected readonly maxWeekendDays = MAX_WEEKEND_DAYS;

  readonly form = this.formBuilder.nonNullable.group({
    currencyDecimals: [2, [Validators.required, Validators.min(0), Validators.max(4)]],
    phonePattern: ['', [Validators.maxLength(200)]],
    weekStartDay: [DayOfWeek.Monday, Validators.required],
    weekendDays: [[DayOfWeek.Saturday, DayOfWeek.Sunday] as DayOfWeek[], [Validators.maxLength(MAX_WEEKEND_DAYS)]],
  });

  constructor() {
    effect(() => {
      const country = this.country();
      this.errors.set([]);
      this.form.reset({
        currencyDecimals: country.currencyDecimals,
        phonePattern: country.phonePattern ?? '',
        weekStartDay: country.weekStartDay,
        weekendDays: [...country.weekendDays],
      });
    });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    const command = new UpdateCountryRegionalSettingsCommand({
      id: this.country().id,
      currencyDecimals: value.currencyDecimals,
      phonePattern: value.phonePattern.trim() || undefined,
      weekStartDay: value.weekStartDay,
      weekendDays: value.weekendDays,
    });
    this.busy.set(true);
    this.errors.set([]);
    this.client.updateCountryRegionalSettings(this.country().id, command).subscribe({
      next: () => {
        this.busy.set(false);
        this.form.markAsPristine();
        this.toast.success($localize`:@@countries.regionalSaved:Regional settings saved.`);
        this.saved.emit();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }
}
