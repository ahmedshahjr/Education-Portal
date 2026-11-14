import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { problemMessages } from '../../core/http/problem-details';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { SchoolsClient, SchoolSummaryDto } from '../../web-api-client';
import { SchoolEditComponent } from './school-edit.component';

/** Master portal: one school's own screen (profile, plan and modules, invoices), reached from the schools list. */
@Component({
  selector: 'app-school-edit-page',
  imports: [RouterLink, MatButtonModule, SchoolEditComponent, EmptyStateComponent, LoadingStateComponent],
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
      <h2 class="mm-school-page__title">{{ current.name }}</h2>
      <app-school-edit [school]="current" (saved)="school.set($event)" />
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
export class SchoolEditPage {
  private readonly route = inject(ActivatedRoute);
  private readonly schools = inject(SchoolsClient);

  protected readonly school = signal<SchoolSummaryDto | null>(null);
  protected readonly loadError = signal<string | null>(null);

  constructor() {
    this.load();
  }

  protected load(): void {
    const id = this.route.snapshot.paramMap.get('schoolId') ?? '';
    this.loadError.set(null);
    this.school.set(null);
    this.schools.getSchool(id).subscribe({
      next: (school) => this.school.set(school),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }
}
