import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Observable } from 'rxjs';
import { problemMessages } from '../../core/http/problem-details';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { ToastService } from '../../shared/services/toast.service';
import { SchoolGenderPolicy, SchoolProfileClient, UpdateSchoolProfileCommand } from '../../web-api-client';
import { GENDER_POLICIES, genderPolicyLabel } from './school-page.labels';

export const TAGLINE_MAX_LENGTH = 200;
export const ABOUT_MAX_LENGTH = 4000;
export const VISITING_HOURS_MAX_LENGTH = 200;
export const FIRST_ESTABLISHED_YEAR = 1800;

/**
 * The text of the school's public page: tagline, about, year opened, who it teaches, visiting hours and a virtual tour link.
 * Saving creates the page as a draft; the school chooses when to publish it, and can take it down again.
 */
@Component({
  selector: 'app-profile-panel',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    StatusBadgeComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './profile-panel.component.html',
  styleUrl: './school-page.css',
})
export class ProfilePanelComponent implements OnInit {
  private readonly client = inject(SchoolProfileClient);
  private readonly toast = inject(ToastService);

  protected readonly policies = GENDER_POLICIES;
  protected readonly policyLabel = genderPolicyLabel;
  protected readonly taglineMax = TAGLINE_MAX_LENGTH;
  protected readonly aboutMax = ABOUT_MAX_LENGTH;
  protected readonly hoursMax = VISITING_HOURS_MAX_LENGTH;
  protected readonly currentYear = new Date().getFullYear();

  protected readonly form = inject(FormBuilder).group({
    tagline: ['', [Validators.required, Validators.maxLength(TAGLINE_MAX_LENGTH)]],
    about: ['', [Validators.required, Validators.maxLength(ABOUT_MAX_LENGTH)]],
    establishedYear: [null as number | null, [Validators.min(FIRST_ESTABLISHED_YEAR), Validators.max(new Date().getFullYear())]],
    genderPolicy: [SchoolGenderPolicy.CoEducation, Validators.required],
    visitingHours: ['', Validators.maxLength(VISITING_HOURS_MAX_LENGTH)],
    virtualTourUrl: [''],
  });

  protected readonly loading = signal(true);
  protected readonly busy = signal(false);
  protected readonly published = signal(false);
  /** True once the page exists on the server: publishing needs a saved page. */
  protected readonly saved = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  ngOnInit(): void {
    this.client.getSchoolProfile(undefined).subscribe({
      next: (profile) => {
        this.form.reset({
          tagline: profile.tagline,
          about: profile.about,
          establishedYear: profile.establishedYear ?? null,
          genderPolicy: profile.genderPolicy,
          visitingHours: profile.visitingHours ?? '',
          virtualTourUrl: profile.virtualTourUrl ?? '',
        });
        this.published.set(profile.isPublished);
        // A school that has not written its page yet gets an empty one back: nothing is saved until the tagline is.
        this.saved.set(profile.tagline !== '');
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const command = new UpdateSchoolProfileCommand({
      tagline: value.tagline?.trim() ?? '',
      about: value.about?.trim() ?? '',
      establishedYear: value.establishedYear ?? undefined,
      genderPolicy: value.genderPolicy ?? SchoolGenderPolicy.CoEducation,
      visitingHours: value.visitingHours?.trim() || undefined,
      virtualTourUrl: value.virtualTourUrl?.trim() || undefined,
    });
    this.run(this.client.updateSchoolProfile(undefined, command), $localize`:@@schoolPage.profileSaved:School page saved.`, () => this.saved.set(true));
  }

  protected publish(): void {
    this.run(this.client.publishSchoolProfile(undefined), $localize`:@@schoolPage.published:School page published.`, () => this.published.set(true));
  }

  protected unpublish(): void {
    this.run(this.client.unpublishSchoolProfile(undefined), $localize`:@@schoolPage.unpublished:School page taken down.`, () => this.published.set(false));
  }

  private run(request: Observable<unknown>, done: string, after: () => void): void {
    this.busy.set(true);
    this.errors.set([]);
    request.subscribe({
      next: () => {
        this.busy.set(false);
        this.toast.success(done);
        after();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }
}
