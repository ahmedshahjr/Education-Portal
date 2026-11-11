import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { BrandingService } from '../../core/branding/branding.service';
import { problemMessages } from '../../core/http/problem-details';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import {
  BrandingDto,
  CountriesClient,
  CountryDto,
  PaginatedListOfSchoolSummaryDto,
  SchoolsClient,
  SchoolSummaryDto,
  UpdateSchoolCountryCommand,
} from '../../web-api-client';
import { BrandingChanges, toFileParameter } from '../branding/branding-changes';
import { BrandingEditorComponent } from '../branding/branding-editor.component';
import { SchoolModulesComponent } from './school-modules.component';

const PAGE_SIZE = 25;

/** Master portal: every school, its country (languages, time zone, currency) and its branding. Master tenant administrators only. */
@Component({
  selector: 'app-schools-page',
  imports: [
    MatCardModule,
    MatTableModule,
    MatPaginatorModule,
    MatButtonModule,
    MatFormFieldModule,
    MatSelectModule,
    PageHeaderComponent,
    LoadingStateComponent,
    EmptyStateComponent,
    BrandingEditorComponent,
    SchoolModulesComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-page-header
      title="Schools"
      i18n-title="@@schools.title"
      subtitle="Master portal: every school on the platform, its country, its modules and its branding."
      i18n-subtitle="@@schools.subtitle"
    />

    <mat-card class="mm-card mm-elevated mm-schools">
      <mat-card-content>
        @if (loadError(); as message) {
          <app-empty-state
            icon="alert-triangle"
            title="Schools could not be loaded"
            i18n-title="@@schools.loadError"
            [description]="message"
            actionLabel="Try again"
            i18n-actionLabel="@@common.tryAgain"
            (action)="loadPage(pageIndex())"
          />
        } @else if (page(); as schools) {
          @if (schools.items.length === 0) {
            <app-empty-state
              icon="layers"
              title="No schools yet"
              i18n-title="@@schools.empty"
              description="Schools appear here once they are added."
              i18n-description="@@schools.emptyDescription"
            />
          } @else {
            <table mat-table [dataSource]="schools.items" class="mm-schools__table">
              <ng-container matColumnDef="name">
                <th mat-header-cell *matHeaderCellDef i18n="@@schools.school">School</th>
                <td mat-cell *matCellDef="let school">
                  {{ school.name }}
                  @if (school.isMaster) {
                    <span i18n="@@schools.masterTenant">(master tenant)</span>
                  }
                </td>
              </ng-container>
              <ng-container matColumnDef="domain">
                <th mat-header-cell *matHeaderCellDef i18n="@@schools.domain">Domain</th>
                <td mat-cell *matCellDef="let school"><bdi dir="ltr">{{ school.domain }}</bdi></td>
              </ng-container>

              <ng-container matColumnDef="country">
                <th mat-header-cell *matHeaderCellDef i18n="@@schools.country">Country</th>
                <td mat-cell *matCellDef="let school">
                  <mat-form-field appearance="outline" subscriptSizing="dynamic" class="mm-schools__country">
                    <mat-select
                      aria-label="Country of {{ school.name }}"
                      i18n-aria-label="@@schools.countryOf"
                      [value]="school.countryId ?? null"
                      [disabled]="assigning() === school.id"
                      (selectionChange)="assignCountry(school, $event.value)"
                    >
                      <mat-option [value]="null" i18n="@@schools.noCountry">None (English only, UTC)</mat-option>
                      @for (country of countries(); track country.id) {
                        <mat-option [value]="country.id">{{ country.name }}</mat-option>
                      }
                    </mat-select>
                  </mat-form-field>
                </td>
              </ng-container>
              <ng-container matColumnDef="actions">
                <th mat-header-cell *matHeaderCellDef><span class="mm-visually-hidden" i18n="@@common.actions">Actions</span></th>
                <td mat-cell *matCellDef="let school">
                  <div class="mm-schools__actions">
                    @if (!school.isMaster) {
                      <button mat-stroked-button type="button" (click)="showModules(school)" i18n="@@schools.modules">Modules</button>
                    }
                    <button mat-stroked-button type="button" (click)="edit(school)" i18n="@@schools.editBranding">Edit branding</button>
                  </div>
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
              i18n-aria-label="@@schools.pages"
            />
          }
        } @else {
          <app-loading-state />
        }
      </mat-card-content>
    </mat-card>

    @if (modulesOf(); as school) {
      <h2 class="mm-schools__heading" i18n="@@schools.modulesOf">Modules of {{ school.name }}</h2>
      <div class="mm-schools__section">
        <app-school-modules [school]="school" />
      </div>
    }

    @if (selected(); as school) {
      <h2 class="mm-schools__heading" i18n="@@schools.brandingOf">Branding of {{ school.name }}</h2>
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

    .mm-schools__country {
      min-width: 200px;
      padding-block: 4px;
    }

    .mm-schools__actions {
      display: flex;
      flex-wrap: wrap;
      gap: var(--mm-space-2);
      padding-block: 4px;
    }

    .mm-schools__section {
      margin-bottom: var(--mm-grid-gap);
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
  private readonly countriesClient = inject(CountriesClient);
  private readonly brandingService = inject(BrandingService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly columns = ['name', 'domain', 'country', 'actions'];
  protected readonly pageSize = PAGE_SIZE;
  protected readonly page = signal<PaginatedListOfSchoolSummaryDto | null>(null);
  protected readonly pageIndex = signal(0);
  protected readonly loadError = signal<string | null>(null);
  protected readonly selected = signal<SchoolSummaryDto | null>(null);
  protected readonly modulesOf = signal<SchoolSummaryDto | null>(null);
  protected readonly selectedBranding = signal<BrandingDto | null>(null);
  protected readonly saving = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  /** The countries to pick from (one page of up to 100, which covers every country in use). */
  protected readonly countries = signal<readonly CountryDto[]>([]);
  protected readonly assigning = signal<number | null>(null);

  constructor() {
    this.loadPage(0);
    this.countriesClient.getCountries(1, 100).subscribe({
      next: (page) => this.countries.set(page.items),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  protected assignCountry(school: SchoolSummaryDto, countryId: number | null): void {
    this.assigning.set(school.id);
    this.client.updateSchoolCountry(school.id, new UpdateSchoolCountryCommand({ countryId: countryId ?? undefined })).subscribe({
      next: () => {
        this.assigning.set(null);
        this.snackBar.open($localize`:@@schools.countrySaved:Country of ${school.name}:SCHOOL: saved`, undefined, { duration: 3000 });
      },
      error: (error: unknown) => {
        this.assigning.set(null);
        this.snackBar.open(problemMessages(error)[0], undefined, { duration: 5000 });
        this.loadPage(this.pageIndex());
      },
    });
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

  protected showModules(school: SchoolSummaryDto): void {
    this.selected.set(null);
    this.modulesOf.set(school);
  }

  protected edit(school: SchoolSummaryDto): void {
    this.modulesOf.set(null);
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
          this.snackBar.open($localize`:@@schools.brandingSaved:Branding of ${school.name}:SCHOOL: saved`, undefined, { duration: 3000 });
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
