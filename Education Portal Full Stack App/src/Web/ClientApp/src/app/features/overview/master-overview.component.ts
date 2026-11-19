import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { problemMessages } from '../../core/http/problem-details';
import { DataTableColumn, DataTableComponent } from '../../shared/components/data-table/data-table.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { CampusKind, OverviewClient, SchoolCampusOverviewDto } from '../../web-api-client';

/** One row of the grid: a campus, with the school it belongs to. */
export interface SchoolCampusRow {
  readonly schoolName: string;
  readonly domain: string;
  readonly country: string;
  readonly schoolActive: boolean;
  readonly campusName: string;
  readonly code: string;
  readonly kind: CampusKind;
  readonly province: string;
  readonly city: string;
}

/**
 * The master tenant's dashboard: every school with its campuses, one row per campus. The master is the software seller and
 * owns no school, so this is what it manages. The search finds a school or a campus by any of its columns.
 */
@Component({
  selector: 'app-master-overview',
  imports: [MatCardModule, PageHeaderComponent, EmptyStateComponent, DataTableComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './master-overview.component.html',
  styleUrl: './master-overview.component.css',
})
export class MasterOverviewComponent {
  private readonly overview = inject(OverviewClient);

  protected readonly schools = signal<readonly SchoolCampusOverviewDto[] | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly kinds = CampusKind;

  /** One row per campus of every school, in the order the server gives (school by name, main campus first). */
  protected readonly rows = computed<SchoolCampusRow[]>(() =>
    (this.schools() ?? []).flatMap((school) =>
      school.campuses.map((campus) => ({
        schoolName: school.name,
        domain: school.domain,
        country: school.countryName ?? '—',
        schoolActive: school.isActive,
        campusName: campus.name,
        code: campus.code,
        kind: campus.kind,
        province: campus.stateName ?? '',
        city: campus.cityName ?? '',
      }))
    )
  );

  protected readonly schoolCount = computed(() => this.schools()?.length ?? 0);
  protected readonly campusCount = computed(() => this.rows().length);

  /** One sentence with both counts, so a translation can order them. */
  protected readonly subtitle = computed(
    () => $localize`:@@overview.subtitle:${this.schoolCount()}:schools: schools and ${this.campusCount()}:campuses: campuses`
  );

  protected readonly columns: DataTableColumn<SchoolCampusRow>[] = [
    { key: 'schoolName', header: $localize`:@@overview.colSchool:School`, valueFn: (r) => r.schoolName },
    { key: 'domain', header: $localize`:@@overview.colDomain:Domain`, valueFn: (r) => r.domain },
    { key: 'country', header: $localize`:@@overview.colCountry:Country`, valueFn: (r) => r.country },
    { key: 'campusName', header: $localize`:@@overview.colCampus:Campus`, valueFn: (r) => r.campusName },
    { key: 'code', header: $localize`:@@overview.colCode:Code`, valueFn: (r) => r.code },
    { key: 'kind', header: $localize`:@@overview.colType:Type`, valueFn: (r) => r.kind },
    { key: 'province', header: $localize`:@@overview.colProvince:State or province`, valueFn: (r) => r.province },
    { key: 'city', header: $localize`:@@overview.colCity:City`, valueFn: (r) => r.city },
  ];

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loadError.set(null);
    this.schools.set(null);
    this.overview.getSchoolCampusOverview().subscribe({
      next: (schools) => this.schools.set(schools),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }
}
