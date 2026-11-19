import { ChangeDetectionStrategy, Component, computed, inject, signal, TemplateRef, viewChild } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { ToastService } from '../../shared/services/toast.service';
import { Observable } from 'rxjs';
import { problemMessages } from '../../core/http/problem-details';
import { AppIconComponent } from '../../shared/components/app-icon/app-icon.component';
import { DataTableCellContext, DataTableColumn, DataTableComponent } from '../../shared/components/data-table/data-table.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import {
  CountriesClient,
  CountryDto,
  CreateCountryCommand,
  LanguageDto,
  UpdateCountryCommand,
} from '../../web-api-client';
import { formatPreview } from './country-format';
import { CountryChanges, CountryFormComponent } from './country-form.component';

/** The countries are listed in one page: the table pages and searches them. */
const ALL_COUNTRIES = 100;

/** A row of the table, with the names of its languages and a format preview worked out once. */
interface CountryRow {
  readonly country: CountryDto;
  readonly languageNames: string;
  readonly defaultLanguageName: string;
  readonly preview: string;
}

/**
 * Master portal: the countries schools can be assigned to, and for each the languages its schools offer, the default
 * language, the time zone and the currency. A row's edit icon changes it. Master tenant administrators only; the server
 * enforces it.
 */
@Component({
  selector: 'app-countries-page',
  imports: [MatCardModule, MatButtonModule, AppIconComponent, PageHeaderComponent, EmptyStateComponent, CountryFormComponent, DataTableComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './countries.page.html',
  styleUrl: './countries.page.css',
})
export class CountriesPage {
  private readonly client = inject(CountriesClient);
  private readonly toast = inject(ToastService);

  protected readonly countries = signal<readonly CountryDto[] | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly languages = signal<readonly LanguageDto[]>([]);

  /** The form: `country` is null when adding one. */
  protected readonly editing = signal<{ country: CountryDto | null } | null>(null);
  protected readonly saving = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  private readonly now = new Date();

  private readonly actionsCell = viewChild.required<TemplateRef<DataTableCellContext<CountryRow>>>('actionsCell');

  protected readonly rows = computed<readonly CountryRow[]>(() => {
    const names = new Map(this.languages().map((language) => [language.code, language.englishName]));
    return (this.countries() ?? []).map((country) => ({
      country,
      languageNames: country.languages.map((code) => names.get(code) ?? code).join(', '),
      defaultLanguageName: names.get(country.defaultLanguage) ?? country.defaultLanguage,
      preview: formatPreview(country, this.now),
    }));
  });

  protected readonly columns = computed<DataTableColumn<CountryRow>[]>(() => [
    { key: 'name', header: $localize`:@@countries.country:Country`, valueFn: (r) => r.country.name },
    { key: 'languages', header: $localize`:@@countries.languages:Languages`, valueFn: (r) => r.languageNames },
    { key: 'default', header: $localize`:@@countries.defaultLanguage:Default language`, valueFn: (r) => r.defaultLanguageName },
    { key: 'region', header: $localize`:@@countries.region:Time zone and currency`, valueFn: (r) => `${r.country.timeZoneId} · ${r.country.currencyCode}` },
    { key: 'preview', header: $localize`:@@countries.preview:Example`, valueFn: (r) => r.preview, sortable: false },
    { key: 'actions', header: $localize`:@@common.actions:Actions`, sortable: false, align: 'right', cell: this.actionsCell() },
  ]);

  constructor() {
    this.load();
    this.client.getLanguages().subscribe({
      next: (languages) => this.languages.set(languages),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  protected load(): void {
    this.loadError.set(null);
    this.countries.set(null);
    this.client.getCountries(1, ALL_COUNTRIES).subscribe({
      next: (page) => this.countries.set(page.items),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
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
        this.toast.success($localize`:@@countries.saved:${changes.name}:COUNTRY: saved`);
        this.load();
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }
}
