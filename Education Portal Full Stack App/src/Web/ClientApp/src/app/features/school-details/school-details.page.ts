import { formatDate } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, LOCALE_ID, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { problemMessages } from '../../core/http/problem-details';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { ToastService } from '../../shared/services/toast.service';
import { phoneValidator } from '../../shared/validation/contact-validators';
import { emailValidator } from '../../shared/validation/email-validators';
import { websiteValidator } from '../../shared/validation/website-validators';
import {
  CampusesClient,
  CampusLocationsDto,
  SchoolDetailsClient,
  SchoolDetailsDto,
  UpdateSchoolDetailsCommand,
} from '../../web-api-client';

/**
 * The school's own registration and contact details (School module): the name in the local language, the registration
 * and tax numbers, the head-office address with its state and city (from the school's country), phone, email, website and
 * the month the academic year usually starts. Everything is optional; the server checks the formats and the city again.
 */
@Component({
  selector: 'app-school-details-page',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    EmptyStateComponent,
    LoadingStateComponent,
    PageHeaderComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './school-details.page.html',
  styleUrl: './school-details.page.css',
})
export class SchoolDetailsPage {
  private readonly client = inject(SchoolDetailsClient);
  private readonly campusesClient = inject(CampusesClient);
  private readonly formBuilder = inject(FormBuilder);
  private readonly toast = inject(ToastService);
  private readonly locale = inject(LOCALE_ID);

  protected readonly loaded = signal(false);
  protected readonly loadError = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);
  protected readonly locations = signal<CampusLocationsDto | null>(null);

  /** The twelve months, named in the page's language. */
  protected readonly months = Array.from({ length: 12 }, (_, index) => ({
    value: index + 1,
    label: formatDate(new Date(2000, index, 1), 'MMMM', this.locale),
  }));

  readonly form = this.formBuilder.nonNullable.group({
    localName: ['', [Validators.maxLength(200)]],
    registrationNumber: ['', [Validators.maxLength(50)]],
    registrationAuthority: ['', [Validators.maxLength(200)]],
    taxNumber: ['', [Validators.maxLength(50)]],
    address: ['', [Validators.maxLength(300)]],
    stateId: [''],
    cityId: [''],
    phone: ['', [Validators.maxLength(30), phoneValidator]],
    email: ['', [Validators.maxLength(254), emailValidator]],
    website: ['', [Validators.maxLength(2048), websiteValidator]],
    academicYearStartMonth: [8, [Validators.required, Validators.min(1), Validators.max(12)]],
  });

  constructor() {
    this.load();
    this.campusesClient.getCampusLocations().subscribe({
      next: (locations) => this.locations.set(locations),
      error: () => this.locations.set(new CampusLocationsDto({ countryName: undefined, states: [] })),
    });
  }

  load(): void {
    this.loadError.set(null);
    this.client.getSchoolDetails(undefined).subscribe({
      next: (details) => {
        this.fill(details);
        this.loaded.set(true);
      },
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  /** The cities of the state chosen in the form. */
  protected citiesOf(stateId: string): readonly { id: string; name: string }[] {
    return this.locations()?.states.find((state) => state.id === stateId)?.cities ?? [];
  }

  /** Changing the state clears the city, which must belong to the new state. */
  protected onStateChange(): void {
    this.form.controls.cityId.setValue('');
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    const text = (input: string): string | undefined => input.trim() || undefined;
    const command = new UpdateSchoolDetailsCommand({
      localName: text(value.localName),
      registrationNumber: text(value.registrationNumber),
      registrationAuthority: text(value.registrationAuthority),
      taxNumber: text(value.taxNumber),
      address: text(value.address),
      cityId: value.cityId || undefined,
      phone: text(value.phone),
      email: text(value.email),
      website: text(value.website),
      academicYearStartMonth: value.academicYearStartMonth,
    });
    this.busy.set(true);
    this.errors.set([]);
    this.client.updateSchoolDetails(undefined, command).subscribe({
      next: () => {
        this.busy.set(false);
        this.form.markAsPristine();
        this.toast.success($localize`:@@schoolDetails.saved:School details saved.`);
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }

  private fill(details: SchoolDetailsDto): void {
    this.form.reset({
      localName: details.localName ?? '',
      registrationNumber: details.registrationNumber ?? '',
      registrationAuthority: details.registrationAuthority ?? '',
      taxNumber: details.taxNumber ?? '',
      address: details.address ?? '',
      stateId: details.stateId ?? '',
      cityId: details.cityId ?? '',
      phone: details.phone ?? '',
      email: details.email ?? '',
      website: details.website ?? '',
      academicYearStartMonth: details.academicYearStartMonth,
    });
  }
}
