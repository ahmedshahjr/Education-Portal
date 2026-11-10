import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { BrandingService } from '../../core/branding/branding.service';
import { problemMessages } from '../../core/http/problem-details';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { BrandingDto, PaginatedListOfSchoolSummaryDto, SchoolsClient, SchoolSummaryDto } from '../../web-api-client';
import { BrandingChanges, toFileParameter } from '../branding/branding-changes';
import { BrandingEditorComponent } from '../branding/branding-editor.component';

const PAGE_SIZE = 25;

/** Master portal: every school, and each school's branding. Master tenant administrators only. */
@Component({
  selector: 'app-schools-page',
  imports: [
    MatCardModule,
    MatTableModule,
    MatPaginatorModule,
    MatButtonModule,
    PageHeaderComponent,
    LoadingStateComponent,
    EmptyStateComponent,
    BrandingEditorComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-page-header title="Schools" subtitle="Master portal: every school on the platform and its branding." />

    <mat-card class="mm-card mm-elevated mm-schools">
      <mat-card-content>
        @if (loadError(); as message) {
          <app-empty-state icon="alert-triangle" title="Schools could not be loaded" [description]="message" actionLabel="Try again" (action)="loadPage(pageIndex())" />
        } @else if (page(); as schools) {
          @if (schools.items.length === 0) {
            <app-empty-state icon="layers" title="No schools yet" description="Schools appear here once they are added." />
          } @else {
            <table mat-table [dataSource]="schools.items" class="mm-schools__table">
              <ng-container matColumnDef="name">
                <th mat-header-cell *matHeaderCellDef>School</th>
                <td mat-cell *matCellDef="let school">{{ school.name }}@if (school.isMaster) { (master tenant) }</td>
              </ng-container>
              <ng-container matColumnDef="domain">
                <th mat-header-cell *matHeaderCellDef>Domain</th>
                <td mat-cell *matCellDef="let school">{{ school.domain }}</td>
              </ng-container>
              <ng-container matColumnDef="actions">
                <th mat-header-cell *matHeaderCellDef><span class="mm-visually-hidden">Actions</span></th>
                <td mat-cell *matCellDef="let school">
                  <button mat-stroked-button type="button" (click)="edit(school)">Edit branding</button>
                </td>
              </ng-container>
              <tr mat-header-row *matHeaderRowDef="columns"></tr>
              <tr mat-row *matRowDef="let row; columns: columns"></tr>
            </table>
            <mat-paginator
              [length]="schools.totalCount"
              [pageIndex]="pageIndex()"
              [pageSize]="pageSize"
              [hidePageSize]="true"
              (page)="changePage($event)"
              aria-label="Choose a page of schools"
            />
          }
        } @else {
          <app-loading-state />
        }
      </mat-card-content>
    </mat-card>

    @if (selected(); as school) {
      <h2 class="mm-schools__heading">Branding of {{ school.name }}</h2>
      @if (selectedBranding(); as branding) {
        <app-branding-editor
          [branding]="branding"
          [showCurrentImages]="school.isMaster"
          [saving]="saving()"
          [errors]="errors()"
          (save)="save(school, $event)"
        />
      } @else {
        <app-loading-state />
      }
    }
  `,
  styles: `
    .mm-schools {
      margin-bottom: var(--mm-grid-gap);
    }

    .mm-schools__table {
      width: 100%;
    }

    .mm-schools__heading {
      margin: 0 0 var(--mm-space-4);
      font-size: var(--mm-text-lg);
      font-weight: 600;
    }
  `,
})
export class SchoolsPage {
  private readonly client = inject(SchoolsClient);
  private readonly brandingService = inject(BrandingService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly columns = ['name', 'domain', 'actions'];
  protected readonly pageSize = PAGE_SIZE;
  protected readonly page = signal<PaginatedListOfSchoolSummaryDto | null>(null);
  protected readonly pageIndex = signal(0);
  protected readonly loadError = signal<string | null>(null);
  protected readonly selected = signal<SchoolSummaryDto | null>(null);
  protected readonly selectedBranding = signal<BrandingDto | null>(null);
  protected readonly saving = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  constructor() {
    this.loadPage(0);
  }

  protected loadPage(index: number): void {
    this.loadError.set(null);
    this.page.set(null);
    this.pageIndex.set(index);
    this.client.getSchools(index + 1, PAGE_SIZE).subscribe({
      next: (page) => this.page.set(page),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  protected changePage(event: PageEvent): void {
    this.loadPage(event.pageIndex);
  }

  protected edit(school: SchoolSummaryDto): void {
    this.selected.set(school);
    this.selectedBranding.set(null);
    this.errors.set([]);
    this.client.getSchoolBranding(school.id).subscribe({
      next: (branding) => this.selectedBranding.set(branding),
      error: (error: unknown) => this.errors.set(problemMessages(error)),
    });
  }

  protected save(school: SchoolSummaryDto, changes: BrandingChanges): void {
    this.saving.set(true);
    this.errors.set([]);

    this.client
      .updateSchoolBranding(
        school.id,
        changes.themePreset,
        changes.sidebarColor,
        toFileParameter(changes.logo),
        changes.removeLogo,
        toFileParameter(changes.favicon),
        changes.removeFavicon
      )
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.snackBar.open(`Branding of ${school.name} saved`, undefined, { duration: 3000 });
          this.edit(school);
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
