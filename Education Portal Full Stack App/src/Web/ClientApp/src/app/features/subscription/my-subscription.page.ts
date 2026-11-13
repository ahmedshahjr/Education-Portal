import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatSnackBar } from '@angular/material/snack-bar';
import { problemMessages } from '../../core/http/problem-details';
import { LocalDatePipe } from '../../core/i18n/local-date.pipe';
import { ModuleTextPipe } from '../../core/i18n/module-text.pipe';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { BadgeTone, StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { AddOnRequestsClient, AvailableAddOnDto, MySubscriptionDto, RequestAddOnCommand, SubscriptionsClient } from '../../web-api-client';

/**
 * The school administrator's own subscription: what the school gets, its price and period, the add-on modules it could
 * add with their prices, and its requests. Asking for a module changes nothing by itself; the platform owner decides.
 * The school comes from the host, never from the page.
 */
@Component({
  selector: 'app-my-subscription-page',
  imports: [
    DecimalPipe,
    LocalDatePipe,
    ModuleTextPipe,
    MatCardModule,
    MatButtonModule,
    PageHeaderComponent,
    LoadingStateComponent,
    EmptyStateComponent,
    StatusBadgeComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './my-subscription.page.html',
  styleUrl: './my-subscription.page.css',
})
export class MySubscriptionPage {
  private readonly client = inject(SubscriptionsClient);
  private readonly requestsClient = inject(AddOnRequestsClient);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly data = signal<MySubscriptionDto | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly busy = signal(false);

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loadError.set(null);
    this.data.set(null);
    this.client.getMySubscription().subscribe({
      next: (data) => this.data.set(data),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  protected tone(status: string): BadgeTone {
    switch (status) {
      case 'Active':
      case 'Approved':
        return 'success';
      case 'Trial':
      case 'Pending':
        return 'primary';
      case 'PastDue':
      case 'Suspended':
        return 'warning';
      default:
        return 'error';
    }
  }

  protected request(addOn: AvailableAddOnDto): void {
    this.busy.set(true);
    this.requestsClient.requestAddOn(new RequestAddOnCommand({ moduleCode: addOn.code })).subscribe({
      next: () => {
        this.busy.set(false);
        this.snackBar.open($localize`:@@mySubscription.requested:Request sent. The platform owner will decide.`, undefined, { duration: 3000 });
        this.load();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.snackBar.open(problemMessages(error)[0], undefined, { duration: 5000 });
      },
    });
  }
}
