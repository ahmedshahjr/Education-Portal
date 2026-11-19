import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { problemMessages } from '../../core/http/problem-details';
import { LocalDatePipe } from '../../core/i18n/local-date.pipe';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { ChangeAuditClient, ChangeAuditDto } from '../../web-api-client';

/**
 * The school's recorded changes (parent links, account roles, subscriptions and the like), newest first. Read only, for the
 * school's administrators; the server filters it to the school and never lets a school see another's entries (ADR-016).
 */
@Component({
  selector: 'app-audit-page',
  imports: [MatCardModule, MatTableModule, PageHeaderComponent, LoadingStateComponent, EmptyStateComponent, LocalDatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './audit.page.html',
  styleUrl: './audit.page.css',
})
export class AuditPage {
  private readonly auditClient = inject(ChangeAuditClient);

  protected readonly columns = ['when', 'action', 'record', 'changes'];
  protected readonly entries = signal<readonly ChangeAuditDto[] | null>(null);
  protected readonly loadError = signal<string | null>(null);

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loadError.set(null);
    this.auditClient.getChangeAudit(100).subscribe({
      next: (entries) => this.entries.set(entries),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }
}
