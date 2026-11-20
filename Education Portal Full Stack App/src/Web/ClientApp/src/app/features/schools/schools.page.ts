import { ChangeDetectionStrategy, Component, computed, inject, signal, TemplateRef, viewChild } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { ToastService } from '../../shared/services/toast.service';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Router } from '@angular/router';
import { problemMessages } from '../../core/http/problem-details';
import { AppIconComponent } from '../../shared/components/app-icon/app-icon.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { DataTableCellContext, DataTableColumn, DataTableComponent } from '../../shared/components/data-table/data-table.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import {
  CountriesClient,
  CountryDto,
  CreateSchoolCommand,
  SchoolsClient,
  SchoolSummaryDto,
  UpdateSchoolCountryCommand,
} from '../../web-api-client';
import { NewSchool, SchoolFormComponent } from './school-form.component';

/** How long a search waits after the last key press, in milliseconds (kept for the search box's timing). */
export const SEARCH_DELAY_MS = 300;

/** Every school and country is loaded in one page: the table filters, searches and pages them on the client. */
const ALL_ITEMS = 100;

/**
 * Master portal: every school on the platform, as a table. The country filter and the search narrow it; each row can change its
 * country, open its modules, open its branding, or be deleted. Adding a school takes its first administrator. Master tenant
 * administrators only.
 */
@Component({
  selector: 'app-schools-page',
  imports: [
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatSelectModule,
    MatTooltipModule,
    PageHeaderComponent,
    EmptyStateComponent,
    AppIconComponent,
    DataTableComponent,
    SchoolFormComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './schools.page.html',
  styleUrl: './schools.page.css',
})
export class SchoolsPage {
  private readonly client = inject(SchoolsClient);
  private readonly countriesClient = inject(CountriesClient);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly dialog = inject(MatDialog);

  protected readonly schools = signal<readonly SchoolSummaryDto[] | null>(null);
  protected readonly loadError = signal<string | null>(null);

  /** The countries to pick from. */
  protected readonly countries = signal<readonly CountryDto[]>([]);
  protected readonly assigning = signal<string | null>(null);

  /** The "Add school" form: whether it is open, being sent, and the server's messages. */
  protected readonly adding = signal(false);
  protected readonly creating = signal(false);
  protected readonly createErrors = signal<readonly string[]>([]);

  /** The country the table is limited to, or null for every country. */
  protected readonly countryFilter = signal<string | null>(null);

  /** The schools the country filter lets through. */
  protected readonly visibleSchools = computed(() => {
    const country = this.countryFilter();
    return (this.schools() ?? []).filter((school) => country === null || school.countryId === country);
  });

  private readonly countryCell = viewChild.required<TemplateRef<DataTableCellContext<SchoolSummaryDto>>>('countryCell');
  private readonly nameCell = viewChild.required<TemplateRef<DataTableCellContext<SchoolSummaryDto>>>('nameCell');
  private readonly actionsCell = viewChild.required<TemplateRef<DataTableCellContext<SchoolSummaryDto>>>('actionsCell');

  protected readonly columns = computed<DataTableColumn<SchoolSummaryDto>[]>(() => [
    { key: 'name', header: $localize`:@@schools.school:School`, valueFn: (s) => s.name, cell: this.nameCell() },
    { key: 'domain', header: $localize`:@@schools.domain:Domain`, valueFn: (s) => s.domain },
    { key: 'country', header: $localize`:@@schools.country:Country`, valueFn: (s) => this.countryName(s.countryId), cell: this.countryCell(), sortable: false },
    { key: 'actions', header: $localize`:@@common.actions:Actions`, sortable: false, align: 'right', cell: this.actionsCell() },
  ]);

  constructor() {
    this.load();
    this.countriesClient.getCountries(1, ALL_ITEMS).subscribe({
      next: (page) => this.countries.set(page.items),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  protected load(): void {
    this.loadError.set(null);
    this.schools.set(null);
    this.client.getSchools(1, ALL_ITEMS, undefined, undefined).subscribe({
      next: (page) => this.schools.set(page.items),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  /** The name of a school's country, for the table's text (the row shows the country picker). */
  protected countryName(countryId: string | null | undefined): string {
    return this.countries().find((c) => c.id === countryId)?.name ?? '';
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
      administratorEmail: school.administratorEmail,
      administratorPassword: school.administratorPassword,
    });

    this.client.createSchool(command).subscribe({
      next: () => {
        this.creating.set(false);
        this.adding.set(false);
        this.toast.success($localize`:@@schools.created:${school.name}:SCHOOL: added. Its administrator signs in on ${school.domain}:DOMAIN: and must change the one-time password.`);
        this.load();
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
        this.toast.success($localize`:@@schools.countrySaved:Country of ${school.name}:SCHOOL: saved`);
        this.load();
      },
      error: (error: unknown) => {
        this.assigning.set(null);
        this.toast.error(error);
        this.load();
      },
    });
  }

  protected changeCountryFilter(countryId: string | null): void {
    this.countryFilter.set(countryId);
  }

  /** Asks first: a deleted school disappears everywhere, but its rows are kept. */
  protected confirmDeleteSchool(school: SchoolSummaryDto): void {
    const data: ConfirmDialogData = {
      title: $localize`:@@schools.deleteTitle:Delete this school?`,
      message: $localize`:@@schools.deleteMessage:The school disappears everywhere: its domain stops working and its subscription stops running. Its data is kept.`,
      confirmLabel: $localize`:@@schools.delete:Delete`,
    };
    this.dialog
      .open(ConfirmDialogComponent, { data, autoFocus: 'dialog' })
      .afterClosed()
      .subscribe((confirmed: boolean | undefined) => {
        if (confirmed === true) {
          this.deleteSchool(school);
        }
      });
  }

  private deleteSchool(school: SchoolSummaryDto): void {
    this.client.deleteSchool(school.id).subscribe({
      next: () => {
        this.toast.success($localize`:@@schools.deleted:School deleted.`);
        this.load();
      },
      error: (error: unknown) => this.toast.error(error),
    });
  }

  protected showModules(school: SchoolSummaryDto): void {
    this.router.navigate(['/schools', school.id]);
  }

  protected edit(school: SchoolSummaryDto): void {
    this.router.navigate(['/schools', school.id, 'branding']);
  }
}
