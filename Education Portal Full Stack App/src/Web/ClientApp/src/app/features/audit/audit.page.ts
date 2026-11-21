import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { problemMessages } from '../../core/http/problem-details';
import { LocalDatePipe } from '../../core/i18n/local-date.pipe';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { ChangeAuditClient, ChangeAuditDto } from '../../web-api-client';

/** The marker the server stores in place of a personal value (ADR-023). */
const HIDDEN = '(hidden)';

/** One recorded value change, as the server stores it: the property and its old and new value. */
interface ValueChange {
  readonly property: string;
  readonly old: string | null;
  readonly new: string | null;
}

/** The entity types the change history can show (the server's audited types). */
export const AUDITED_ENTITY_TYPES = [
  'Campus',
  'UserDetails',
  'SchoolRole',
  'RolePermission',
  'AccountRole',
  'UserCampus',
  'ParentStudent',
  'Course',
  'Enrolment',
  'Assessment',
  'Score',
  'GradingScale',
  'GradingBand',
  'TimetableSlot',
  'SchoolEvent',
] as const;

const PAGE_SIZE = 25;

/**
 * The school's recorded changes, newest first, with filters by record type and date and paging (ADR-023). A reader sees the
 * changes of the modules their roles give ViewHistory for; the server filters it. Personal values are shown as "changed",
 * never as the old or new value. The sentences are built here, in the reader's language.
 */
@Component({
  selector: 'app-audit-page',
  imports: [FormsModule, MatCardModule, MatTableModule, MatButtonModule, PageHeaderComponent, LoadingStateComponent, EmptyStateComponent, LocalDatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './audit.page.html',
  styleUrl: './audit.page.css',
})
export class AuditPage {
  private readonly auditClient = inject(ChangeAuditClient);

  protected readonly columns = ['when', 'sentence'];
  protected readonly entityTypes = AUDITED_ENTITY_TYPES;
  protected readonly pageSize = PAGE_SIZE;

  protected readonly entries = signal<readonly ChangeAuditDto[] | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly page = signal(0);
  protected readonly hasNext = signal(false);

  /** The filters as the form holds them: '' means no filter. Dates are yyyy-mm-dd. */
  protected entityType = '';
  protected fromDate = '';
  protected toDate = '';

  protected readonly firstShown = computed(() => (this.entries()?.length ? this.page() * PAGE_SIZE + 1 : 0));

  constructor() {
    this.load();
  }

  /** Loads one page with the filters. One extra row tells whether there is a next page. */
  protected load(): void {
    this.loadError.set(null);
    this.entries.set(null);
    this.auditClient
      .getChangeAudit(
        this.page() * PAGE_SIZE,
        PAGE_SIZE + 1,
        this.entityType || undefined,
        this.fromDate ? new Date(`${this.fromDate}T00:00:00Z`) : undefined,
        this.toDate ? new Date(`${this.toDate}T23:59:59Z`) : undefined
      )
      .subscribe({
        next: (rows) => {
          this.hasNext.set(rows.length > PAGE_SIZE);
          this.entries.set(rows.slice(0, PAGE_SIZE));
        },
        error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
      });
  }

  protected applyFilters(): void {
    this.page.set(0);
    this.load();
  }

  protected previous(): void {
    this.page.update((p) => Math.max(0, p - 1));
    this.load();
  }

  protected next(): void {
    this.page.update((p) => p + 1);
    this.load();
  }

  /** Who made the change, as the reader sees it. */
  protected actor(entry: ChangeAuditDto): string {
    return entry.actorName ?? $localize`:@@audit.system:The system`;
  }

  /** The verb of the change: added, changed or removed. */
  protected verb(entry: ChangeAuditDto): string {
    switch (String(entry.action)) {
      case 'Added':
        return $localize`:@@audit.verbAdded:added`;
      case 'Removed':
        return $localize`:@@audit.verbRemoved:removed`;
      default:
        return $localize`:@@audit.verbChanged:changed`;
    }
  }

  /** The changes of the row as readable lines. A personal value is shown as "changed", never as its value. */
  protected changeLines(entry: ChangeAuditDto): string[] {
    return this.parse(entry.changes).map((change) => {
      if (change.old === HIDDEN || change.new === HIDDEN) {
        return $localize`:@@audit.hiddenChange:${change.property}:property: was changed`;
      }
      return $localize`:@@audit.change:${change.property}:property: from ${change.old ?? '—'}:old: to ${change.new ?? '—'}:new:`;
    });
  }

  private parse(json: string): ValueChange[] {
    try {
      const parsed: unknown = JSON.parse(json);
      return Array.isArray(parsed) ? (parsed as ValueChange[]) : [];
    } catch {
      return [];
    }
  }
}
