import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Router, RouterLink } from '@angular/router';
import { problemMessages } from '../../core/http/problem-details';
import { LatLng } from '../../core/maps/google-maps';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { phoneValidator } from '../../shared/validation/contact-validators';
import { ToastService } from '../../shared/services/toast.service';
import {
  CampusesClient,
  CampusLocationsDto,
  CampusStateDto,
  CreateCampusCommand,
  UserDetailsClient,
  UserDetailsDto,
  UserType,
} from '../../web-api-client';
import { CampusLocationPickerComponent } from './campus-location-picker.component';
import { campusProfileOf, createCampusForm, MAX_PHONES } from './campus-form.base';
import { genderPolicyOptions } from './campus-gender-policy';

/**
 * The page for adding a campus (route /campuses/new). Saving returns to the campus list with a confirmation; Cancel goes back
 * to the list without saving. Editing a campus stays on the list page.
 */
@Component({
  selector: 'app-campus-page',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    PageHeaderComponent,
    CampusLocationPickerComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './campus-page.component.html',
  styleUrl: './campus-page.component.css',
})
export class CampusPageComponent {
  private readonly campusesClient = inject(CampusesClient);
  private readonly userDetailsClient = inject(UserDetailsClient);
  private readonly formBuilder = inject(FormBuilder);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  /** The states and cities of the school's country, or null while they load. */
  protected readonly locations = signal<CampusLocationsDto | null>(null);

  /** The people who can be a campus's principal. */
  protected readonly principals = signal<readonly UserDetailsDto[]>([]);

  /** The gender policies a campus can have, with their labels. */
  protected readonly genderPolicies = genderPolicyOptions();
  protected readonly maxPhones = MAX_PHONES;

  protected readonly form = createCampusForm(this.formBuilder);

  constructor() {
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

  /** Changing the state clears the city, which must belong to the new state. */
  protected onStateChange(): void {
    this.form.controls.cityId.setValue('');
  }

  /** The place the map picker reported: the form stores it. */
  setLocation(point: LatLng): void {
    this.form.patchValue({ latitude: point.lat, longitude: point.lng });
    this.form.markAsDirty();
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

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.busy.set(true);
    this.errors.set([]);
    this.campusesClient.createCampus(new CreateCampusCommand(campusProfileOf(this.form))).subscribe({
      next: () => {
        this.busy.set(false);
        this.toast.success($localize`:@@campuses.added:Campus added.`);
        void this.router.navigate(['/campuses']);
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }
}
