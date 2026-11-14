import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable } from 'rxjs';
import { problemMessages } from '../../core/http/problem-details';
import { LocalDatePipe } from '../../core/i18n/local-date.pipe';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { BadgeTone, StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import {
  CreateInvoiceCommand,
  InvoiceDto,
  InvoicesClient,
  InvoiceStatus,
  PaymentMethod,
  RecordPaymentCommand,
  SchoolSummaryDto,
} from '../../web-api-client';

/**
 * Master portal: one school's invoices, on their own tab. Each invoice shows its balance due, dates and, on
 * request, its totals, lines and payments. The master issues invoices, records a manual payment for the whole total and
 * voids an open invoice; the server checks each action (ADR-013).
 */
@Component({
  selector: 'app-school-invoices',
  imports: [DecimalPipe, LocalDatePipe, ReactiveFormsModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatSelectModule, LoadingStateComponent, EmptyStateComponent, StatusBadgeComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './school-invoices.component.html',
  styleUrl: './school-invoices.component.css',
})
export class SchoolInvoicesComponent {
  private readonly invoicesClient = inject(InvoicesClient);
  private readonly snackBar = inject(MatSnackBar);
  private readonly formBuilder = inject(FormBuilder);

  readonly school = input.required<SchoolSummaryDto>();

  protected readonly invoices = signal<readonly InvoiceDto[] | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly expandedId = signal<string | null>(null);
  protected readonly payingInvoice = signal<InvoiceDto | null>(null);

  protected readonly methods = Object.values(PaymentMethod);
  protected readonly invoiceStatuses = InvoiceStatus;

  protected readonly paymentForm = this.formBuilder.nonNullable.group({
    method: [PaymentMethod.BankTransfer, Validators.required],
    reference: ['', [Validators.required, Validators.maxLength(100)]],
  });

  constructor() {
    effect(() => {
      this.school();
      this.load();
    });
  }

  protected load(): void {
    this.loadError.set(null);
    this.invoices.set(null);
    this.payingInvoice.set(null);
    this.invoicesClient.getInvoices(this.school().id).subscribe({
      next: (invoices) => this.invoices.set(invoices),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  /** What the school still owes: the total less the payments recorded so far, never below zero, to the cent. */
  protected balanceDue(invoice: InvoiceDto): number {
    const paid = invoice.payments.reduce((sum, payment) => sum + payment.amount, 0);
    return Math.max(0, Math.round((invoice.total - paid) * 100) / 100);
  }

  protected tone(status: InvoiceStatus): BadgeTone {
    switch (status) {
      case InvoiceStatus.Paid:
        return 'success';
      case InvoiceStatus.Open:
        return 'primary';
      default:
        return 'error';
    }
  }

  protected toggleDetails(invoice: InvoiceDto): void {
    this.expandedId.set(this.expandedId() === invoice.id ? null : invoice.id);
  }

  protected issue(nextPeriod: boolean): void {
    this.run(
      this.invoicesClient.createInvoice(new CreateInvoiceCommand({ schoolId: this.school().id, nextPeriod })),
      $localize`:@@invoices.issued:Invoice issued.`
    );
  }

  protected startPayment(invoice: InvoiceDto): void {
    this.paymentForm.reset({ method: PaymentMethod.BankTransfer, reference: '' });
    this.payingInvoice.set(invoice);
  }

  protected cancelPayment(): void {
    this.payingInvoice.set(null);
  }

  /** Records a payment for the invoice's whole total, received now; the server refuses anything else. */
  protected recordPayment(): void {
    const invoice = this.payingInvoice();
    if (invoice === null || this.paymentForm.invalid) {
      return;
    }
    const value = this.paymentForm.getRawValue();
    this.run(
      this.invoicesClient.recordPayment(
        invoice.id,
        new RecordPaymentCommand({ invoiceId: invoice.id, amount: invoice.total, method: value.method, reference: value.reference.trim(), receivedDate: new Date() })
      ),
      $localize`:@@invoices.paymentRecorded:Payment recorded.`
    );
  }

  protected voidInvoice(invoice: InvoiceDto): void {
    this.run(this.invoicesClient.voidInvoice(invoice.id), $localize`:@@invoices.voided:Invoice voided.`);
  }

  /** Runs one change, tells the master what happened and reloads, so the list always shows what the server holds. */
  private run(request: Observable<unknown>, done: string): void {
    this.busy.set(true);
    request.subscribe({
      next: () => {
        this.busy.set(false);
        this.snackBar.open(done, undefined, { duration: 3000 });
        this.load();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.snackBar.open(problemMessages(error)[0], undefined, { duration: 5000 });
      },
    });
  }
}
