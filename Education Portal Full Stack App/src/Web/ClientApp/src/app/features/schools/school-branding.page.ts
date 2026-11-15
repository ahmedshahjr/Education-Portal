import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BrandingService } from '../../core/branding/branding.service';
import { problemMessages } from '../../core/http/problem-details';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { BrandingDto, SchoolsClient, SchoolSummaryDto } from '../../web-api-client';
import { BrandingChanges, toFileParameter } from '../branding/branding-changes';
import { BrandingEditorComponent } from '../branding/branding-editor.component';

/** Master portal: one school's branding (colours, logo, favicon), reached from the schools list. */
@Component({
  selector: 'app-school-branding-page',
  imports: [RouterLink, MatButtonModule, BrandingEditorComponent, EmptyStateComponent, LoadingStateComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <a mat-button routerLink="/schools" class="mm-school-page__back" i18n="@@schoolPage.back">Back to schools</a>
    @if (loadError(); as message) {
      <app-empty-state
        icon="alert-triangle"
        title="The school could not be loaded"
        i18n-title="@@schoolPage.loadError"
        [description]="message"
        actionLabel="Try again"
        i18n-actionLabel="@@common.tryAgain"
        (action)="load()"
      />
    } @else if (school(); as current) {
      <h2 class="mm-school-page__title" i18n="@@schools.brandingOf">Branding of {{ current.name }}</h2>
      @if (branding(); as value) {
        <app-branding-editor
          [branding]="value"
          [imagesBase]="'api/Schools/' + current.id + '/branding/images'"
          [saving]="saving()"
          [errors]="errors()"
          (save)="save(current, $event)"
        />
      } @else {
        <app-loading-state />
      }
    } @else {
      <app-loading-state />
    }
  `,
  styles: `
    .mm-school-page__back {
      margin-bottom: var(--mm-space-3);
    }

    .mm-school-page__title {
      margin: 0 0 var(--mm-space-4);
      font-size: var(--mm-text-lg);
      font-weight: 600;
    }
  `,
})
export class SchoolBrandingPage {
  private readonly route = inject(ActivatedRoute);
  private readonly schools = inject(SchoolsClient);
  private readonly brandingService = inject(BrandingService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly school = signal<SchoolSummaryDto | null>(null);
  protected readonly branding = signal<BrandingDto | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  constructor() {
    this.load();
  }

  protected load(): void {
    const id = this.route.snapshot.paramMap.get('schoolId') ?? '';
    this.loadError.set(null);
    this.schools.getSchool(id).subscribe({
      next: (school) => {
        this.school.set(school);
        this.schools.getSchoolBranding(school.id).subscribe({
          next: (branding) => this.branding.set(branding),
          error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
        });
      },
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  protected save(school: SchoolSummaryDto, changes: BrandingChanges): void {
    this.saving.set(true);
    this.errors.set([]);
    this.schools
      .updateSchoolBranding(
        school.id,
        changes.accentColor,
        changes.sidebarColor,
        toFileParameter(changes.logo),
        changes.removeLogo,
        toFileParameter(changes.favicon),
        changes.removeFavicon
      )
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.snackBar.open($localize`:@@schools.brandingSaved:Branding of ${school.name}:SCHOOL: saved`, undefined, { duration: 3000 });
          this.load();
          if (school.isMaster) {
            this.brandingService.load().subscribe();
          }
        },
        error: (error: unknown) => {
          this.saving.set(false);
          this.errors.set(problemMessages(error));
        },
      });
  }
}
