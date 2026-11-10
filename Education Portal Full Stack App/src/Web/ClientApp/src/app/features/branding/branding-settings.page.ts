import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BrandingService } from '../../core/branding/branding.service';
import { problemMessages } from '../../core/http/problem-details';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { BrandingClient } from '../../web-api-client';
import { BrandingChanges, toFileParameter } from './branding-changes';
import { BrandingEditorComponent } from './branding-editor.component';

/** A school administrator edits their own school's branding. The server picks the school from the domain. */
@Component({
  selector: 'app-branding-settings-page',
  imports: [PageHeaderComponent, LoadingStateComponent, BrandingEditorComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-page-header
      title="Branding"
      [subtitle]="'How ' + brandingService.schoolName() + ' looks to everyone who signs in on this domain.'"
    />

    @if (brandingService.branding(); as branding) {
      <app-branding-editor [branding]="branding" [saving]="saving()" [errors]="errors()" (save)="save($event)" />
    } @else {
      <app-loading-state />
    }
  `,
})
export class BrandingSettingsPage {
  protected readonly brandingService = inject(BrandingService);
  private readonly client = inject(BrandingClient);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly saving = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  protected save(changes: BrandingChanges): void {
    this.saving.set(true);
    this.errors.set([]);

    this.client
      .updateBranding(
        changes.themePreset,
        changes.sidebarColor,
        toFileParameter(changes.logo),
        changes.removeLogo,
        toFileParameter(changes.favicon),
        changes.removeFavicon
      )
      .subscribe({
        next: () => {
          this.brandingService.load().subscribe(() => {
            this.saving.set(false);
            this.snackBar.open('Branding saved', undefined, { duration: 3000 });
          });
        },
        error: (error: unknown) => {
          this.saving.set(false);
          this.errors.set(problemMessages(error));
        },
      });
  }
}
