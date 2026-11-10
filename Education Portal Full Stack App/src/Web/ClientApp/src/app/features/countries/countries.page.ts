import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { Observable } from 'rxjs';
import { problemMessages } from '../../core/http/problem-details';
import { AppIconComponent } from '../../shared/components/app-icon/app-icon.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import {
  CountriesClient,
  CountryDto,
  CreateCountryCommand,
  LanguageDto,
  PaginatedListOfCountryDto,
  UpdateCountryCommand,
} from '../../web-api-client';
import { formatPreview } from './country-format';
import { CountryChanges, CountryFormComponent } from './country-form.component';

const PAGE_SIZE = 25;

/** A row of the table, with the names of its languages and a format preview worked out once. */
interface CountryRow {
  readonly country: CountryDto;
  readonly languageNames: string;
  readonly defaultLanguageName: string;
  readonly preview: string;
}

/**
 * Master portal: the countries schools can be assigned to, and for each the languages its schools offer, the default
 * language, the time zone and the currency. Master tenant administrators only; the server enforces it.
 */
@Component({
  selector: 'app-countries-page',
  imports: [
    MatCardModule,
    MatTableModule,
    MatPaginatorModule,
    MatButtonModule,
    AppIconComponent,
    PageHeaderComponent,
    LoadingStateComponent,
    EmptyStateComponent,
    CountryFormComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-page-header
      title="Countries and languages"
      i18n-title="@@countries.title"
      subtitle="Master portal: the languages, time zone and currency of each country. Assign a school to a country on the Schools page."
      i18n-subtitle="@@countries.subtitle"
    >
      <button mat-flat-button type="button" (click)="add()" [disabled]="languages().length === 0">
        <app-icon name="plus" [size]="18" />
        <span i18n="@@countries.add">Add country</span>
      </button>
    </app-page-header>

    @if (editing(); as state) {
      <div class="mm-countries__form">
        <app-country-form
          [country]="state.country"
          [languages]="languages()"
          [saving]="saving()"
          [errors]="errors()"
          (save)="save(state.country, $event)"
          (cancel)="closeForm()"
        />
      </div>
    }

    <mat-card class="mm-card mm-elevated">
      <mat-card-content>
        @if (loadError(); as message) {
          <app-empty-state
            icon="alert-triangle"
            title="Countries could not be loaded"
            i18n-title="@@countries.loadError"
            [description]="message"
            actionLabel="Try again"
            i18n-actionLabel="@@common.tryAgain"
            (action)="loadPage(pageIndex())"
          />
        } @else if (page(); as countries) {
          @if (countries.items.length === 0) {
            <app-empty-state
              icon="globe"
              title="No countries yet"
              i18n-title="@@countries.empty"
              description="Add a country to offer its schools more languages than English."
              i18n-description="@@countries.emptyDescription"
            />
          } @else {
            <div class="mm-countries__scroll">
              <table mat-table [dataSource]="rows()" class="mm-countries__table">
                <ng-container matColumnDef="name">
                  <th mat-header-cell *matHeaderCellDef i18n="@@countries.country">Country</th>
                  <td mat-cell *matCellDef="let row">{{ row.country.name }} <bdi dir="ltr">({{ row.country.code }})</bdi></td>
                </ng-container>

                <ng-container matColumnDef="languages">
                  <th mat-header-cell *matHeaderCellDef i18n="@@countries.languages">Languages</th>
                  <td mat-cell *matCellDef="let row">{{ row.languageNames }}</td>
                </ng-container>

                <ng-container matColumnDef="default">
                  <th mat-header-cell *matHeaderCellDef i18n="@@countries.defaultLanguage">Default language</th>
                  <td mat-cell *matCellDef="let row">{{ row.defaultLanguageName }}</td>
                </ng-container>

                <ng-container matColumnDef="region">
                  <th mat-header-cell *matHeaderCellDef i18n="@@countries.region">Time zone and currency</th>
                  <td mat-cell *matCellDef="let row">
                    <bdi dir="ltr">{{ row.country.timeZoneId }} · {{ row.country.currencyCode }}</bdi>
                  </td>
                </ng-container>

                <ng-container matColumnDef="preview">
                  <th mat-header-cell *matHeaderCellDef i18n="@@countries.preview">Example</th>
                  <td mat-cell *matCellDef="let row"><bdi>{{ row.preview }}</bdi></td>
                </ng-container>

                <ng-container matColumnDef="actions">
                  <th mat-header-cell *matHeaderCellDef><span class="mm-visually-hidden" i18n="@@common.actions">Actions</span></th>
                  <td mat-cell *matCellDef="let row">
                    <button mat-stroked-button type="button" (click)="edit(row.country)" i18n="@@common.edit">Edit</button>
                  </td>
                </ng-container>

                <tr mat-header-row *matHeaderRowDef="columns"></tr>
                <tr mat-row *matRowDef="let row; columns: columns"></tr>
              </table>
            </div>

            <mat-paginator
              [length]="countries.totalCount"
              [pageIndex]="pageIndex()"
              [pageSize]="pageSize"
              [hidePageSize]="true"
              (page)="changePage($event)"
              aria-label="Choose a page of countries"
              i18n-aria-label="@@countries.pages"
            />
          }
        } @else {
          <app-loading-state />
        }
      </mat-card-content>
    </mat-card>
  `,
  styles: `
    .mm-countries__form {
      margin-bottom: var(--mm-grid-gap);
    }

    .mm-countries__scroll {
      overflow-x: auto;
    }

    .mm-countries__table {
      width: 100%;
    }
  `,
})
export class CountriesPage {
  private readonly client = inject(CountriesClient);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly columns = ['name', 'languages', 'default', 'region', 'preview', 'actions'];
  protected readonly pageSize = PAGE_SIZE;

  protected readonly page = signal<PaginatedListOfCountryDto | null>(null);
  protected readonly pageIndex = signal(0);
  protected readonly loadError = signal<string | null>(null);
  protected readonly languages = signal<readonly LanguageDto[]>([]);

  /** The form: `country` is null when adding one. */
  protected readonly editing = signal<{ country: CountryDto | null } | null>(null);
  protected readonly saving = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  private readonly now = new Date();

  protected readonly rows = computed<readonly CountryRow[]>(() => {
    const names = new Map(this.languages().map((language) => [language.code, language.englishName]));
    return (this.page()?.items ?? []).map((country) => ({
      country,
      languageNames: country.languages.map((code) => names.get(code) ?? code).join(', '),
      defaultLanguageName: names.get(country.defaultLanguage) ?? country.defaultLanguage,
      preview: formatPreview(country, this.now),
    }));
  });

  constructor() {
    this.loadPage(0);
    this.client.getLanguages().subscribe({
      next: (languages) => this.languages.set(languages),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  protected loadPage(index: number): void {
    this.loadError.set(null);
    this.page.set(null);
    this.pageIndex.set(index);
    this.client.getCountries(index + 1, PAGE_SIZE).subscribe({
      next: (page) => this.page.set(page),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  protected changePage(event: PageEvent): void {
    this.loadPage(event.pageIndex);
  }

  protected add(): void {
    this.errors.set([]);
    this.editing.set({ country: null });
  }

  protected edit(country: CountryDto): void {
    this.errors.set([]);
    this.editing.set({ country });
  }

  protected closeForm(): void {
    this.editing.set(null);
  }

  protected save(country: CountryDto | null, changes: CountryChanges): void {
    this.saving.set(true);
    this.errors.set([]);

    const request: Observable<unknown> = country
      ? this.client.updateCountry(country.id, new UpdateCountryCommand({ ...changes, id: country.id }))
      : this.client.createCountry(new CreateCountryCommand(changes));

    request.subscribe({
      next: () => {
        this.saving.set(false);
        this.editing.set(null);
        this.snackBar.open($localize`:@@countries.saved:${changes.name}:COUNTRY: saved`, undefined, { duration: 3000 });
        this.loadPage(this.pageIndex());
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }
}
