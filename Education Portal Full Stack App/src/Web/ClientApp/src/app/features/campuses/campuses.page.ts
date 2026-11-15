import { NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { AbstractControl, FormArray, FormBuilder, FormControl, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable } from 'rxjs';
import { problemMessages } from '../../core/http/problem-details';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import {
  CampusDetailsInput,
  CampusDto,
  CampusesClient,
  CampusKind,
  CampusLocationsDto,
  CampusSettingsDto,
  CampusStateDto,
  CreateCampusCommand,
  UpdateCampusCommand,
  UpdateCampusSettingsCommand,
  UserDetailsClient,
  UserDetailsDto,
  UserType,
} from '../../web-api-client';

/** The most phone numbers a campus can list (the server enforces the same limit). */
const MAX_PHONES = 5;

/** The campus form's phones: between one and five numbers, none blank. */
function phonesValidator(control: AbstractControl): ValidationErrors | null {
  const array = control as FormArray<FormControl<string>>;
  const filled = array.controls.map((c) => c.value.trim()).filter((v) => v.length > 0);
  if (filled.length === 0) {
    return { required: true };
  }
  return filled.length > MAX_PHONES ? { maxPhones: true } : null;
}

/**
 * The school's campuses, for its administrators (the Campuses module). A campus has a four-digit code, unique in the school,
 * and contact and registration details: the address, the state, the city, the postal code, one to five phone numbers, the
 * school number and the short name are required; the website and the principal are optional. The state and city come from
 * the school's country. The main campus is the school's registration campus: it cannot be deleted.
 */
@Component({
  selector: 'app-campuses-page',
  imports: [
    NgTemplateOutlet,
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
    PageHeaderComponent,
    EmptyStateComponent,
    LoadingStateComponent,
    StatusBadgeComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './campuses.page.html',
  styleUrl: './campuses.page.css',
})
export class CampusesPage {
  private readonly campusesClient = inject(CampusesClient);
  private readonly userDetailsClient = inject(UserDetailsClient);
  private readonly formBuilder = inject(FormBuilder);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  protected readonly campuses = signal<readonly CampusDto[] | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  /** The campus being edited, or null. */
  protected readonly editingId = signal<string | null>(null);

  /** True while the add form is open. */
  protected readonly adding = signal(false);

  /** The states and cities of the school's country, or null while they load. */
  protected readonly locations = signal<CampusLocationsDto | null>(null);

  /** The people who can be a campus's principal. */
  protected readonly principals = signal<readonly UserDetailsDto[]>([]);

  /** The code settings as the server holds them, or null while they load. */
  protected readonly settings = signal<CampusSettingsDto | null>(null);
  protected readonly settingsErrors = signal<readonly string[]>([]);

  protected readonly kinds = CampusKind;
  protected readonly maxPhones = MAX_PHONES;

  /** The form is open for adding or for editing one campus. */
  protected readonly formOpen = computed(() => this.adding() || this.editingId() !== null);

  readonly form = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    address: ['', [Validators.required, Validators.maxLength(300)]],
    stateId: ['', [Validators.required]],
    cityId: ['', [Validators.required]],
    postalCode: ['', [Validators.required, Validators.maxLength(10)]],
    phones: this.formBuilder.array<FormControl<string>>(
      [this.formBuilder.nonNullable.control('')],
      { validators: [phonesValidator] }
    ),
    website: ['', [Validators.maxLength(2048)]],
    schoolNumber: ['', [Validators.required, Validators.maxLength(50)]],
    shortName: ['', [Validators.required, Validators.maxLength(25)]],
    principalId: [''],
  });

  protected readonly settingsForm = this.formBuilder.nonNullable.group({
    prefixEnabled: [false],
    prefix: ['', [Validators.maxLength(6), Validators.pattern(/^[a-z0-9]*$/i)]],
  });

  constructor() {
    this.load();
    this.loadSettings();
    this.loadLocations();
    this.loadPrincipals();
  }

  get phones(): FormArray<FormControl<string>> {
    return this.form.controls.phones;
  }

  /** The cities of the state chosen in the form. */
  protected citiesOf(stateId: string): readonly { id: string; name: string }[] {
    const state: CampusStateDto | undefined = this.locations()?.states.find((s) => s.id === stateId);
    return state?.cities ?? [];
  }

  protected load(): void {
    this.loadError.set(null);
    this.campuses.set(null);
    this.campusesClient.getCampuses().subscribe({
      next: (campuses) => this.campuses.set(campuses),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  protected loadLocations(): void {
    this.campusesClient.getCampusLocations().subscribe({
      next: (locations) => this.locations.set(locations),
      error: () => this.locations.set(new CampusLocationsDto({ states: [] })),
    });
  }

  protected loadPrincipals(): void {
    this.userDetailsClient.getUserDetails(UserType.Principal).subscribe({
      next: (people) => this.principals.set(people),
      error: () => this.principals.set([]),
    });
  }

  protected loadSettings(): void {
    this.campusesClient.getCampusSettings().subscribe({
      next: (settings) => {
        this.settings.set(settings);
        this.settingsForm.reset({ prefixEnabled: settings.prefixEnabled, prefix: settings.prefix ?? '' });
        this.updatePrefixValidation();
      },
      error: (error: unknown) => this.settingsErrors.set(problemMessages(error)),
    });
  }

  /** The prefix is required only while it is switched on. */
  protected updatePrefixValidation(): void {
    const prefix = this.settingsForm.controls.prefix;
    if (this.settingsForm.controls.prefixEnabled.value) {
      prefix.setValidators([Validators.required, Validators.maxLength(6), Validators.pattern(/^[a-z0-9]*$/i)]);
    } else {
      prefix.setValidators([Validators.maxLength(6), Validators.pattern(/^[a-z0-9]*$/i)]);
    }
    prefix.updateValueAndValidity();
  }

  protected saveSettings(): void {
    if (this.settingsForm.invalid) {
      return;
    }
    const value = this.settingsForm.getRawValue();
    this.busy.set(true);
    this.settingsErrors.set([]);
    this.campusesClient
      .updateCampusSettings(new UpdateCampusSettingsCommand({ prefixEnabled: value.prefixEnabled, prefix: value.prefix.trim().toUpperCase() || undefined }))
      .subscribe({
        next: () => {
          this.busy.set(false);
          this.snackBar.open($localize`:@@campuses.settingsSaved:Campus codes saved.`, undefined, { duration: 3000 });
          this.loadSettings();
          this.load();
        },
        error: (error: unknown) => {
          this.busy.set(false);
          this.settingsErrors.set(problemMessages(error));
        },
      });
  }

  startAdd(): void {
    this.errors.set([]);
    this.editingId.set(null);
    this.resetForm();
    this.adding.set(true);
  }

  protected startEdit(campus: CampusDto): void {
    this.errors.set([]);
    this.adding.set(false);
    this.editingId.set(campus.id);
    const details = campus.details;
    const phones = details.phones.length > 0 ? details.phones : [''];
    this.form.reset({
      name: campus.name,
      address: details.address ?? '',
      stateId: details.stateId ?? '',
      cityId: details.cityId ?? '',
      postalCode: details.postalCode ?? '',
      website: details.website ?? '',
      schoolNumber: details.schoolNumber ?? '',
      shortName: details.shortName ?? '',
      principalId: details.principalId ?? '',
    });
    this.setPhones(phones);
  }

  protected cancel(): void {
    this.errors.set([]);
    this.adding.set(false);
    this.editingId.set(null);
  }

  /** Changing the state clears the city, which must belong to the new state. */
  protected onStateChange(): void {
    this.form.controls.cityId.setValue('');
  }

  protected addPhone(): void {
    if (this.phones.length < MAX_PHONES) {
      this.phones.push(this.formBuilder.nonNullable.control(''));
      this.phones.updateValueAndValidity();
    }
  }

  protected removePhone(index: number): void {
    if (this.phones.length > 1) {
      this.phones.removeAt(index);
      this.phones.updateValueAndValidity();
    }
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const editing = this.editingId();
    const value = this.form.getRawValue();
    const details = new CampusDetailsInput({
      address: value.address.trim(),
      stateId: value.stateId,
      cityId: value.cityId,
      postalCode: value.postalCode.trim(),
      phones: this.phones.controls.map((c) => c.value.trim()).filter((v) => v.length > 0),
      website: value.website.trim() || undefined,
      schoolNumber: value.schoolNumber.trim(),
      shortName: value.shortName.trim(),
      principalId: value.principalId || undefined,
    });

    this.busy.set(true);
    this.errors.set([]);
    const request: Observable<unknown> = editing
      ? this.campusesClient.updateCampus(editing, new UpdateCampusCommand({ name: value.name.trim(), details }))
      : this.campusesClient.createCampus(new CreateCampusCommand({ name: value.name.trim(), details }));

    request.subscribe({
      next: () => {
        this.busy.set(false);
        this.snackBar.open(
          editing
            ? $localize`:@@campuses.updated:Campus saved.`
            : $localize`:@@campuses.added:Campus added.`,
          undefined,
          { duration: 3000 }
        );
        this.cancel();
        this.load();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }

  /** Asks first: a deleted campus disappears for everyone; its data is kept. */
  protected confirmDelete(campus: CampusDto): void {
    const data: ConfirmDialogData = {
      title: $localize`:@@campuses.deleteTitle:Delete this campus?`,
      message: $localize`:@@campuses.deleteMessage:The campus disappears from the school. Its data is kept. A campus with module grants must have them turned off first.`,
      confirmLabel: $localize`:@@campuses.delete:Delete`,
    };
    this.dialog
      .open(ConfirmDialogComponent, { data, autoFocus: 'dialog' })
      .afterClosed()
      .subscribe((confirmed: boolean | undefined) => {
        if (confirmed === true) {
          this.run(this.campusesClient.deleteCampus(campus.id), $localize`:@@campuses.deleted:Campus deleted.`);
        }
      });
  }

  private resetForm(): void {
    this.form.reset({
      name: '',
      address: '',
      stateId: '',
      cityId: '',
      postalCode: '',
      website: '',
      schoolNumber: '',
      shortName: '',
      principalId: '',
    });
    this.setPhones(['']);
  }

  private setPhones(numbers: readonly string[]): void {
    this.phones.clear();
    for (const number of numbers) {
      this.phones.push(this.formBuilder.nonNullable.control(number));
    }
    this.phones.updateValueAndValidity();
  }

  private run(request: Observable<unknown>, done: string): void {
    this.busy.set(true);
    this.errors.set([]);
    request.subscribe({
      next: () => {
        this.busy.set(false);
        this.snackBar.open(done, undefined, { duration: 3000 });
        this.load();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }
}
