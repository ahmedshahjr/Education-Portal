import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject, signal } from '@angular/core';
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
import { SearchInputComponent } from '../../shared/components/search-input/search-input.component';
import {
  BrandingDto,
  CountriesClient,
  CreateSchoolCommand,
  DataRegion2,
  CountryDto,
  PaginatedListOfSchoolSummaryDto,
  SchoolsClient,
  SchoolSummaryDto,
  UpdateSchoolCountryCommand,
} from '../../web-api-client';
import { BrandingChanges, toFileParameter } from '../branding/branding-changes';
import { BrandingEditorComponent } from '../branding/branding-editor.component';
import { NewSchool, SchoolFormComponent } from './school-form.component';
import { SchoolModulesComponent } from './school-modules.component';

const PAGE_SIZE = 25;

/** How long the search box waits after the last key press before asking the server, in milliseconds. */
export const SEARCH_DELAY_MS = 300;

/** Master portal: every school (searchable by name and limited to one country on request), adding a school with its first administrator, its country (languages, time zone, currency) and its branding. Master tenant administrators only. */
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
    SearchInputComponent,
    BrandingEditorComponent,
    SchoolModulesComponent,
    SchoolFormComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-page-header
      title="Schools"
      i18n-title="@@schools.title"
      subtitle="Master portal: every school on the platform, its country, its modules and its branding."
      i18n-subtitle="@@schools.subtitle"
    >
      @if (!adding()) {
        <button mat-flat-button type="button" (click)="startAdding()" i18n="@@schools.add">Add school</button>
      }
    </app-page-header>

    @if (adding()) {
      <div class="mm-schools__section">
        <app-school-form
          [countries]="countries()"
          [saving]="creating()"
          [errors]="createErrors()"
          (save)="create($event)"
          (cancel)="adding.set(false)"
        />
      </div>
    }

    <mat-card class="mm-card mm-elevated mm-schools">
      <mat-card-content>
        <div class="mm-table-toolbar">
          <div class="mm-table-toolbar__search">
            <app-search-input
              [value]="search()"
              (valueChange)="changeSearch($event)"
              placeholder="Search schools by name"
              i18n-placeholder="@@schools.searchPlaceholder"
              ariaLabel="Search schools by name"
              i18n-ariaLabel="@@schools.searchLabel"
            />
          </div>
          <div class="mm-table-toolbar__extra">
            <mat-form-field appearance="outline" subscriptSizing="dynamic" class="mm-schools__country-filter">
              <mat-label i18n="@@schools.countryFilter">Country</mat-label>
              <mat-select [value]="countryFilter()" (selectionChange)="changeCountryFilter($event.value)">
                <mat-option [value]="null" i18n="@@schools.allCountries">All countries</mat-option>
                @for (country of countries(); track country.id) {
                  <mat-option [value]="country.id">{{ country.name }}</mat-option>
                }
              </mat-select>
            </mat-form-field>
          </div>
        </div>

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
          @if (schools.items.length === 0 && isFiltered()) {
            <app-empty-state
              icon="search"
              title="No schools match"
              i18n-title="@@schools.noMatch"
              description="No school matches the search and country you picked."
              i18n-description="@@schools.noMatchDescription"
              actionLabel="Clear filters"
              i18n-actionLabel="@@schools.clearFilters"
              (action)="clearFilters()"
            />
          } @else if (schools.items.length === 0) {
            <app-empty-state
              icon="layers"
              title="No schools yet"
              i18n-title="@@schools.empty"
              description="Schools appear here once they are added."
              i18n-description="@@schools.emptyDescription"
              actionLabel="Add school"
              i18n-actionLabel="@@schools.add"
              (action)="startAdding()"
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
          [imagesBase]="'api/Schools/' + school.id + '/branding/images'"
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

    .mm-table-toolbar {
      display: flex;
      align-items: center;
      justify-content: space-between;
      flex-wrap: wrap;
      gap: 12px;
      margin-bottom: var(--mm-space-4);
    }

    .mm-table-toolbar__search {
      width: 280px;
      max-width: 100%;
    }

    .mm-table-toolbar__extra {
      display: flex;
      align-items: center;
      gap: 10px;
      flex-wrap: wrap;
    }

    .mm-schools__country-filter {
      min-width: 200px;
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
  protected readonly assigning = signal<string | null>(null);

  /** The "Add school" form: whether it is open, being sent, and the server's messages. */
  protected readonly adding = signal(false);
  protected readonly creating = signal(false);
  protected readonly createErrors = signal<readonly string[]>([]);

  /** The name search and the country the table is limited to; the server filters, so paging stays correct. */
  protected readonly search = signal('');
  protected readonly countryFilter = signal<string | null>(null);
  protected readonly isFiltered = computed(() => this.search().trim() !== '' || this.countryFilter() !== null);
  private searchTimer: ReturnType<typeof setTimeout> | undefined;

  constructor() {
    inject(DestroyRef).onDestroy(() => clearTimeout(this.searchTimer));
    this.loadPage(0);
    this.countriesClient.getCountries(1, 100).subscribe({
      next: (page) => this.countries.set(page.items),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  protected startAdding(): void {
    this.createErrors.set([]);
    this.adding.set(true);
  }

  protected create(school: NewSchool): void {
    this.creating.set(true);
    this.createErrors.set([]);
    const command = new CreateSchoolCommand({
      name: school.name,
      domain: school.domain,
      countryId: school.countryId ?? undefined,
      // NSwag types a nullable enum as an empty class (DataRegion2); on the wire it is the enum's name, as here.
      dataRegion: (school.dataRegion ?? undefined) as unknown as DataRegion2 | undefined,
      administratorEmail: school.administratorEmail,
      administratorPassword: school.administratorPassword,
    });

    this.client.createSchool(command).subscribe({
      next: () => {
        this.creating.set(false);
        this.adding.set(false);
        this.snackBar.open(
          $localize`:@@schools.created:${school.name}:SCHOOL: added. Its administrator signs in on ${school.domain}:DOMAIN: and must change the one-time password.`,
          undefined,
          { duration: 6000 }
        );
        // Show the new school: search for it, so it is on the first page whatever the sort order.
        clearTimeout(this.searchTimer);
        this.search.set(school.name);
        this.countryFilter.set(null);
        this.loadPage(0);
      },
      error: (error: unknown) => {
        this.creating.set(false);
        this.createErrors.set(problemMessages(error));
      },
    });
  }

  protected assignCountry(school: SchoolSummaryDto, countryId: string | null): void {
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
    const search = this.search().trim();
    this.client.getSchools(index + 1, PAGE_SIZE, search || undefined, this.countryFilter() ?? undefined).subscribe({
      next: (page) => this.page.set(page),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  protected changeSearch(search: string): void {
    this.search.set(search);
    clearTimeout(this.searchTimer);
    this.searchTimer = setTimeout(() => this.loadPage(0), SEARCH_DELAY_MS);
  }

  protected changeCountryFilter(countryId: string | null): void {
    this.countryFilter.set(countryId);
    this.loadPage(0);
  }

  protected clearFilters(): void {
    clearTimeout(this.searchTimer);
    this.search.set('');
    this.countryFilter.set(null);
    this.loadPage(0);
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
