import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { problemMessages } from '../../core/http/problem-details';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { CreateUserDetailsCommand, UserDetailsClient, UserDetailsDto, UserType } from '../../web-api-client';

/** The people of the school: principals, teachers and other staff (the Staff module). */
@Component({
  selector: 'app-people-page',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    PageHeaderComponent,
    EmptyStateComponent,
    LoadingStateComponent,
    StatusBadgeComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './people.page.html',
  styleUrl: './people.page.css',
})
export class PeoplePage {
  private readonly peopleClient = inject(UserDetailsClient);
  private readonly formBuilder = inject(FormBuilder);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly people = signal<readonly UserDetailsDto[] | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  /** The type filter: a user type, or null for everyone. */
  protected readonly filter = signal<UserType | null>(null);

  protected readonly types = [UserType.Principal, UserType.Teacher, UserType.Staff];

  protected readonly form = this.formBuilder.nonNullable.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    middleName: ['', [Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    userType: [UserType.Teacher as UserType, [Validators.required]],
    phone: ['', [Validators.maxLength(30)]],
  });

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loadError.set(null);
    this.people.set(null);
    this.peopleClient.getUserDetails(this.filter() ?? undefined).subscribe({
      next: (people) => this.people.set(people),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  protected setFilter(type: UserType | null): void {
    this.filter.set(type);
    this.load();
  }

  protected add(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.busy.set(true);
    this.errors.set([]);
    this.peopleClient
      .createUserDetails(
        new CreateUserDetailsCommand({
          firstName: value.firstName.trim(),
          middleName: value.middleName.trim() || undefined,
          lastName: value.lastName.trim(),
          userType: value.userType,
          phone: value.phone.trim() || undefined,
        })
      )
      .subscribe({
        next: () => {
          this.busy.set(false);
          this.form.reset({ firstName: '', middleName: '', lastName: '', userType: UserType.Teacher, phone: '' });
          this.snackBar.open($localize`:@@people.added:User added.`, undefined, { duration: 3000 });
          this.load();
        },
        error: (error: unknown) => {
          this.busy.set(false);
          this.errors.set(problemMessages(error));
        },
      });
  }
}
