import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { LocalDatePipe } from '../../core/i18n/local-date.pipe';
import { problemMessages } from '../../core/http/problem-details';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { InvoiceDto, InvoiceStatus, InvoicesClient, SchoolSummaryDto, SchoolsClient } from '../../web-api-client';

/** Every invoice of the platform, or of one school, for the master tenant. */
@Component({
  selector: 'app-invoices-page',
  imports: [LocalDatePipe, MatCardModule, MatFormFieldModule, MatSelectModule, PageHeaderComponent, EmptyStateComponent, LoadingStateComponent, StatusBadgeComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './invoices.page.html',
  styleUrl: './invoices.page.css',
})
export class InvoicesPage {
  private readonly invoicesClient = inject(InvoicesClient);
  private readonly schoolsClient = inject(SchoolsClient);

  protected readonly invoices = signal<readonly InvoiceDto[] | null>(null);
  protected readonly schools = signal<readonly SchoolSummaryDto[]>([]);
  protected readonly loadError = signal<string | null>(null);

  /** The school filter: a school's public id, or empty for every school. */
  protected readonly schoolFilter = signal<string>('');

  protected readonly statuses = InvoiceStatus;

  constructor() {
    this.schoolsClient.getSchools(1, 100, undefined, undefined).subscribe({
      next: (page) => this.schools.set(page.items),
    });
    this.load();
  }

  protected load(): void {
    this.loadError.set(null);
    this.invoices.set(null);
    this.invoicesClient.getInvoices(this.schoolFilter() || undefined).subscribe({
      next: (invoices) => this.invoices.set(invoices),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  protected setSchool(id: string): void {
    this.schoolFilter.set(id);
    this.load();
  }
}
