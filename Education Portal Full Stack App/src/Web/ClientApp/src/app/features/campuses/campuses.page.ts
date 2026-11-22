import { NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal, TemplateRef, viewChild } from '@angular/core';
import { FormArray, FormBuilder, FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Router } from '@angular/router';
import { ToastService } from '../../shared/services/toast.service';
import { Observable } from 'rxjs';
import { CampusDetailsDialogComponent } from './campus-details-dialog.component';
import { phoneValidator } from '../../shared/validation/contact-validators';
import { CampusLocationPickerComponent } from './campus-location-picker.component';
import { genderPolicyOptions } from './campus-gender-policy';
import { campusProfileOf, createCampusForm, CampusForm, MAX_PHONES } from './campus-form.base';
import { CampusScopeService } from '../../core/campus/campus-scope.service';
import { problemMessages } from '../../core/http/problem-details';
import { LatLng } from '../../core/maps/google-maps';
import { AppIconComponent } from '../../shared/components/app-icon/app-icon.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { DataTableCellContext, DataTableColumn, DataTableComponent } from '../../shared/components/data-table/data-table.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import {
  CampusDto,
  CampusesClient,
  CampusKind,
  CampusLocationsDto,
  CampusStateDto,
  UpdateCampusCommand,
  UserDetailsClient,
  UserDetailsDto,
  UserType,
} from '../../web-api-client';

/**
 * The school's campuses, for its administrators (the Campuses module), as a table. A row opens the campus's details in a
 * dialog; the icons edit it or delete it. Adding a campus has its own page (/campuses/new). A campus has a four-digit code,
 * unique in the school, and contact and registration details: the address, the state (province), the city, the postal code,
 * one to five phone numbers, the school number, the short name and the place on the map are required; the website and the
 * principal are optional. The state and city come from the school's country. The main campus is the school's registration
 * campus: it cannot be deleted. The code prefix is set under Customization, Campus codes.
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
    PageHeaderComponent,
    EmptyStateComponent,
    StatusBadgeComponent,
    AppIconComponent,
    DataTableComponent,
    CampusLocationPickerComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './campuses.page.html',
  styleUrl: './campuses.page.css',
})
export class CampusesPage {
  private readonly campusesClient = inject(CampusesClient);
  private readonly userDetailsClient = inject(UserDetailsClient);
  private readonly formBuilder = inject(FormBuilder);
  private readonly toast = inject(ToastService);
  private readonly dialog = inject(MatDialog);
  private readonly campusScope = inject(CampusScopeService);
  private readonly router = inject(Router);

  protected readonly campuses = signal<readonly CampusDto[] | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  /** The campus being edited, or null. */
  protected readonly editingId = signal<string | null>(null);

  /** The states and cities of the school's country, or null while they load. */
  protected readonly locations = signal<CampusLocationsDto | null>(null);

  /** The people who can be a campus's principal. */
  protected readonly principals = signal<readonly UserDetailsDto[]>([]);

  protected readonly kinds = CampusKind;

  /** The gender policies a campus can have, with their labels. */
  protected readonly genderPolicies = genderPolicyOptions();
  protected readonly maxPhones = MAX_PHONES;

  /** The list filters: a state (province) and a city, or '' for all. */
  readonly stateFilter = signal('');
  readonly cityFilter = signal('');

  /** The campuses the filters let through. */
  readonly visibleCampuses = computed(() => {
    const state = this.stateFilter();
    const city = this.cityFilter();
    // The campus chosen in the top bar narrows the list too.
    return (this.campuses() ?? []).filter(
      (campus) =>
        this.campusScope.shows(campus.id) &&
        (state === '' || campus.details.stateId === state) &&
        (city === '' || campus.details.cityId === city)
    );
  });

  /** The cities for the city filter: those of the chosen state, or every city of the country when no state is chosen. */
  readonly cityFilterOptions = computed(() => {
    const state = this.stateFilter();
    const states = this.locations()?.states ?? [];
    return (state === '' ? states : states.filter((s) => s.id === state)).flatMap((s) => s.cities);
  });

  /** The edit form is open while one campus is being edited. */
  protected readonly formOpen = computed(() => this.editingId() !== null);

  /** The cell templates the table draws for the name, the type and the actions. */
  private readonly nameCell = viewChild.required<TemplateRef<DataTableCellContext<CampusDto>>>('nameCell');
  private readonly kindCell = viewChild.required<TemplateRef<DataTableCellContext<CampusDto>>>('kindCell');
  private readonly actionsCell = viewChild.required<TemplateRef<DataTableCellContext<CampusDto>>>('actionsCell');

  protected readonly columns = computed<DataTableColumn<CampusDto>[]>(() => [
    {
      key: 'name',
      header: $localize`:@@campuses.colCampus:Campus`,
      valueFn: (c) => `${c.name} ${c.displayCode}`,
      cell: this.nameCell(),
    },
    { key: 'address', header: $localize`:@@campuses.addressColumn:Address`, valueFn: (c) => c.details.address ?? '' },
    { key: 'state', header: $localize`:@@campuses.state:State or province`, valueFn: (c) => c.details.stateName ?? '' },
    { key: 'city', header: $localize`:@@campuses.city:City`, valueFn: (c) => c.details.cityName ?? '' },
    { key: 'principal', header: $localize`:@@campuses.principal:Principal`, valueFn: (c) => c.details.principalName ?? '' },
    { key: 'phone', header: $localize`:@@campuses.phones:Phone numbers`, valueFn: (c) => c.details.phones.join(' · ') },
    { key: 'kind', header: $localize`:@@campuses.colType:Type`, valueFn: (c) => c.kind, cell: this.kindCell() },
    { key: 'actions', header: $localize`:@@common.actions:Actions`, sortable: false, align: 'right', cell: this.actionsCell() },
  ]);

  protected readonly form: CampusForm = createCampusForm(this.formBuilder);

  constructor() {
    this.load();
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

  /** Choosing a state drops a city that is not in it. */
  setStateFilter(stateId: string): void {
    this.stateFilter.set(stateId);
    if (this.cityFilter() !== '' && !this.cityFilterOptions().some((c) => c.id === this.cityFilter())) {
      this.cityFilter.set('');
    }
  }

  clearFilters(): void {
    this.stateFilter.set('');
    this.cityFilter.set('');
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
      error: () => this.locations.set(new CampusLocationsDto({ countryName: undefined, states: [] })),
    });
  }

  protected loadPrincipals(): void {
    this.userDetailsClient.getUserDetails(UserType.Principal).subscribe({
      next: (people) => this.principals.set(people),
      error: () => this.principals.set([]),
    });
  }

  /** Opens the campus's details in a dialog. */
  protected openDetails(campus: CampusDto): void {
    this.dialog.open(CampusDetailsDialogComponent, { data: campus, width: '640px', autoFocus: 'dialog' });
  }

  /** Adding a campus has its own page. */
  protected addCampus(): void {
    void this.router.navigate(['/campuses', 'new']);
  }

  startEdit(campus: CampusDto): void {
    this.errors.set([]);
    this.editingId.set(campus.id);
    const details = campus.details;
    const phones = details.phones.length > 0 ? details.phones : [''];
    this.form.reset({
      name: campus.name,
      localName: campus.localName ?? '',
      genderPolicy: campus.genderPolicy,
      address: details.address ?? '',
      stateId: details.stateId ?? '',
      cityId: details.cityId ?? '',
      postalCode: details.postalCode ?? '',
      website: details.website ?? '',
      schoolNumber: details.schoolNumber ?? '',
      shortName: details.shortName ?? '',
      principalId: details.principalId ?? '',
      latitude: details.latitude ?? null,
      longitude: details.longitude ?? null,
    });
    this.setPhones(phones);
  }

  protected cancel(): void {
    this.errors.set([]);
    this.editingId.set(null);
  }

  /** The place the map picker reported: the form stores it. */
  setLocation(point: LatLng): void {
    this.form.patchValue({ latitude: point.lat, longitude: point.lng });
    this.form.markAsDirty();
  }

  /** A Google Maps link to the campus's place. Null when the campus has no place. */
  protected mapLink(details: { latitude?: number | null; longitude?: number | null }): string | null {
    if (details.latitude == null || details.longitude == null) {
      return null;
    }
    return `https://www.google.com/maps?q=${details.latitude},${details.longitude}`;
  }

  /** Changing the state clears the city, which must belong to the new state. */
  protected onStateChange(): void {
    this.form.controls.cityId.setValue('');
  }

  protected addPhone(): void {
    if (this.phones.length < MAX_PHONES) {
      this.phones.push(this.formBuilder.nonNullable.control('', [phoneValidator]));
      this.phones.updateValueAndValidity();
    }
  }

  protected removePhone(index: number): void {
    if (this.phones.length > 1) {
      this.phones.removeAt(index);
      this.phones.updateValueAndValidity();
    }
  }

  /** Saves the campus being edited. */
  save(): void {
    const editing = this.editingId();
    if (editing === null) {
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.busy.set(true);
    this.errors.set([]);
    this.campusesClient.updateCampus(editing, new UpdateCampusCommand(campusProfileOf(this.form))).subscribe({
      next: () => {
        this.busy.set(false);
        this.toast.success($localize`:@@campuses.updated:Campus saved.`);
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

  private setPhones(numbers: readonly string[]): void {
    this.phones.clear();
    for (const number of numbers) {
      this.phones.push(this.formBuilder.nonNullable.control(number, [phoneValidator]));
    }
    this.phones.updateValueAndValidity();
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
