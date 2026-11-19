import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { CampusFilterComponent, CampusOption } from '../../shared/components/campus-filter/campus-filter.component';
import { DataTableColumn, DataTableComponent } from '../../shared/components/data-table/data-table.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { problemMessages } from '../../core/http/problem-details';
import { CampusDto, CampusesClient, ClassOfferingsClient, IClassOfferingDto } from '../../web-api-client';
import { rowsForCampus } from './class-offerings.logic';

/** A class offering as the table shows it, with its campus name resolved ("All campuses" when it has none). */
interface OfferingRow extends IClassOfferingDto {
  readonly campusName: string;
}

/**
 * The school's class offerings (School module), as a table. When the school has more than one campus, a dropdown above the
 * table filters the rows by campus (AC4.1, AC4.2); the server still decides which offerings the user may see.
 */
@Component({
  selector: 'app-class-offerings-page',
  imports: [MatCardModule, PageHeaderComponent, EmptyStateComponent, DataTableComponent, CampusFilterComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './class-offerings.page.html',
  styleUrl: './class-offerings.page.css',
})
export class ClassOfferingsPage {
  private readonly offeringsClient = inject(ClassOfferingsClient);
  private readonly campusesClient = inject(CampusesClient);

  protected readonly offerings = signal<readonly OfferingRow[] | null>(null);
  protected readonly campuses = signal<readonly CampusDto[]>([]);
  protected readonly loadError = signal<string | null>(null);

  /** The campus the list is filtered to, or null for all campuses. */
  protected readonly selectedCampusId = signal<string | null>(null);

  protected readonly campusOptions = computed<readonly CampusOption[]>(() =>
    this.campuses().map((campus) => ({ id: campus.id, name: campus.name }))
  );

  protected readonly visibleOfferings = computed(() =>
    rowsForCampus(this.offerings() ?? [], this.selectedCampusId())
  );

  protected readonly columns: DataTableColumn<OfferingRow>[] = [
    { key: 'campus', header: $localize`:@@classOfferings.colCampus:Campus`, valueFn: (o) => o.campusName },
    { key: 'level', header: $localize`:@@classOfferings.colLevel:Class`, valueFn: (o) => o.level },
    { key: 'year', header: $localize`:@@classOfferings.colYear:Academic year`, valueFn: (o) => o.academicYear },
    { key: 'fee', header: $localize`:@@classOfferings.colFee:Monthly fee`, valueFn: (o) => `${o.currencyCode} ${o.monthlyFee}` },
    { key: 'seats', header: $localize`:@@classOfferings.colSeats:Seats left`, valueFn: (o) => `${o.seatsAvailable} / ${o.seatsTotal}` },
    { key: 'status', header: $localize`:@@classOfferings.colStatus:Admissions`, valueFn: (o) => o.status },
  ];

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loadError.set(null);
    this.offerings.set(null);
    this.campusesClient.getCampuses().subscribe({
      next: (campuses) => {
        this.campuses.set(campuses);
        this.loadOfferings(campuses);
      },
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  private loadOfferings(campuses: readonly CampusDto[]): void {
    const names = new Map(campuses.map((campus) => [campus.id, campus.name]));
    this.offeringsClient.getClassOfferings(undefined).subscribe({
      next: (rows) =>
        this.offerings.set(
          rows.map((row) => ({
            ...row,
            campusName: row.campusId ? (names.get(row.campusId) ?? '') : $localize`:@@classOfferings.allCampuses:All campuses`,
          }))
        ),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }
}
