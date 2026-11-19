import { ChangeDetectionStrategy, Component, computed, inject, signal, TemplateRef, viewChild } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { LocalDatePipe } from '../../core/i18n/local-date.pipe';
import { problemMessages } from '../../core/http/problem-details';
import { DataTableCellContext, DataTableColumn, DataTableComponent } from '../../shared/components/data-table/data-table.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { InvoiceDto, InvoiceStatus, InvoicesClient, SchoolSummaryDto, SchoolsClient } from '../../web-api-client';

/** Every invoice of the platform, or of one school, for the master tenant. */
@Component({
  selector: 'app-invoices-page',
  imports: [LocalDatePipe, MatCardModule, MatFormFieldModule, MatSelectModule, PageHeaderComponent, EmptyStateComponent, StatusBadgeComponent, DataTableComponent],
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

  private readonly statusCell = viewChild.required<TemplateRef<DataTableCellContext<InvoiceDto>>>('statusCell');
  private readonly periodCell = viewChild.required<TemplateRef<DataTableCellContext<InvoiceDto>>>('periodCell');
  private readonly dueCell = viewChild.required<TemplateRef<DataTableCellContext<InvoiceDto>>>('dueCell');

  protected readonly columns = computed<DataTableColumn<InvoiceDto>[]>(() => [
    { key: 'number', header: $localize`:@@invoices.number:Number`, valueFn: (i) => i.number },
    { key: 'status', header: $localize`:@@invoices.status:Status`, valueFn: (i) => i.status, cell: this.statusCell() },
    { key: 'period', header: $localize`:@@invoices.period:Period`, valueFn: (i) => i.periodStart.toString(), cell: this.periodCell(), sortable: false },
    { key: 'due', header: $localize`:@@invoices.due:Due`, valueFn: (i) => i.dueDate.toString(), cell: this.dueCell() },
    { key: 'total', header: $localize`:@@invoices.total:Total`, valueFn: (i) => i.total, align: 'right' },
  ]);

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
