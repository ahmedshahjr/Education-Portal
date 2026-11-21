import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { catchError, map, switchMap } from 'rxjs/operators';
import { of } from 'rxjs';
import { problemMessages } from '../../core/http/problem-details';
import { ToastService } from '../../shared/services/toast.service';
import { UserDetailsClient, UserDetailsDto, UserType } from '../../web-api-client';
import { buildPersonChanges, needsAdmissionDetails } from './person-changes';
import { PersonFormData, PersonFormResult } from './person-form.models';
import { PersonFormDataService } from './person-form-data.service';
import { PersonSaverService } from './person-saver.service';

/**
 * Adds a person or changes one, on its own page (`/people/new` or `/people/:id`). Large entities get a page, not a
 * dialog (ADR-024). Adding also creates the person's login: an email and a one-time password, which the person replaces on
 * first sign-in. A parent's children, every account's roles and the campuses the person works at are chosen here too, and
 * saved with the person.
 */
@Component({
  selector: 'app-person-page',
  imports: [ReactiveFormsModule, RouterLink, MatButtonModule, MatFormFieldModule, MatInputModule, MatSelectModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './person-page.component.html',
  styleUrl: './person-page.component.css',
})
export class PersonPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly peopleClient = inject(UserDetailsClient);
  private readonly dataService = inject(PersonFormDataService);
  private readonly saver = inject(PersonSaverService);
  private readonly toast = inject(ToastService);
  private readonly formBuilder = inject(FormBuilder);

  protected readonly types = [UserType.Principal, UserType.Teacher, UserType.Staff, UserType.Student, UserType.Parent];
  protected readonly loading = signal(true);
  protected readonly busy = signal(false);
  protected readonly loadError = signal<string | null>(null);
  protected readonly errors = signal<readonly string[]>([]);

  protected data: PersonFormData | null = null;
  protected person: UserDetailsDto | null = null;
  protected adding = true;
  protected students: readonly UserDetailsDto[] = [];
  protected roles: PersonFormData['roles'] = [];
  protected campuses: PersonFormData['campuses'] = [];

  protected form = this.buildForm(null);
  protected readonly selectedType = signal<UserType>(UserType.Teacher);
  protected readonly showAdmissionNote = signal(false);
  protected readonly showChildren = signal(false);

  constructor() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id === null || id === 'new') {
      this.start(null);
    } else {
      this.peopleClient
        .getUserDetails(undefined)
        .pipe(
          map((people) => people.find((p) => p.id === id) ?? null),
          switchMap((person) => (person ? this.dataService.load(person) : of(null)))
        )
        .subscribe({
          next: (data) => {
            if (data === null) {
              this.loadError.set($localize`:@@people.notFound:This user was not found.`);
              this.loading.set(false);
            } else {
              this.start(data);
            }
          },
          error: (error: unknown) => {
            this.loadError.set(problemMessages(error)[0]);
            this.loading.set(false);
          },
        });
    }
  }

  /** Starts the form from the loaded data. A new person starts with an empty form and the school's choices. */
  private start(data: PersonFormData | null): void {
    const load = data ? of(data) : this.dataService.load(null);
    load.subscribe((loaded) => {
      this.data = loaded;
      this.person = loaded.person;
      this.adding = loaded.person === null;
      this.students = loaded.students;
      this.roles = loaded.roles;
      this.campuses = loaded.campuses;
      this.form = this.buildForm(loaded);
      this.selectedType.set(this.form.controls.userType.value);
      this.showAdmissionNote.set(needsAdmissionDetails(this.selectedType()));
      this.showChildren.set(this.selectedType() === UserType.Parent);
      this.form.controls.userType.valueChanges.subscribe((type) => {
        this.selectedType.set(type);
        this.showAdmissionNote.set(needsAdmissionDetails(type));
        this.showChildren.set(type === UserType.Parent);
      });
      this.loading.set(false);
    });
  }

  private buildForm(data: PersonFormData | null) {
    const person = data?.person ?? null;
    const adding = person === null;
    const built = this.formBuilder.nonNullable.group({
      firstName: [person?.firstName ?? '', [Validators.required, Validators.maxLength(100)]],
      middleName: [person?.middleName ?? '', [Validators.maxLength(100)]],
      lastName: [person?.lastName ?? '', [Validators.required, Validators.maxLength(100)]],
      userType: [person?.userType ?? UserType.Teacher, [Validators.required]],
      phone: [person?.phone ?? '', [Validators.maxLength(30)]],
      email: ['', [Validators.maxLength(256)]],
      oneTimePassword: ['', [Validators.maxLength(128)]],
      children: [[...(data?.currentChildIds ?? [])] as string[]],
      roles: [[...(data?.currentRoleIds ?? [])] as string[]],
      campuses: [[...(data?.currentCampusIds ?? [])] as string[]],
    });
    if (adding) {
      // Adding a person creates their login, so the email and the one-time password are required.
      built.controls.email.setValidators([Validators.required, Validators.email, Validators.maxLength(256)]);
      built.controls.oneTimePassword.setValidators([Validators.required, Validators.minLength(15), Validators.maxLength(128)]);
    }
    return built;
  }

  protected save(): void {
    if (this.form.invalid || this.data === null) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const result: PersonFormResult = {
      fields: buildPersonChanges(value, this.adding),
      childIds: value.userType === UserType.Parent ? value.children : [],
      roleIds: value.roles,
      campusIds: value.campuses,
    };

    this.busy.set(true);
    this.errors.set([]);
    this.saver.save(this.person, result, this.data).subscribe({
      next: () => {
        this.busy.set(false);
        this.toast.success(this.person ? $localize`:@@people.updated:User saved.` : $localize`:@@people.added:User added.`);
        this.router.navigate(['/people']);
      },
      error: (error: unknown) => {
        this.busy.set(false);
        // The person may have been saved before the roles, campuses or children failed; the list shows the truth.
        this.errors.set(problemMessages(error));
      },
    });
  }
}
