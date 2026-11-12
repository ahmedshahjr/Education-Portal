import { ChangeDetectionStrategy, Component } from '@angular/core';
import { DeliveryAnalyticsCardComponent } from './widgets/delivery-analytics-card.component';
import { EarningReportsCardComponent } from './widgets/earning-reports-card.component';
import { KpiTrioRowComponent } from './widgets/kpi-trio-row.component';
import { MarketingReportCardComponent } from './widgets/marketing-report-card.component';
import { OverallBalanceCardComponent } from './widgets/overall-balance-card.component';
import { PaymentMethodsCardComponent } from './widgets/payment-methods-card.component';
import { RecentProjectsCardComponent } from './widgets/recent-projects-card.component';
import { RoiCardComponent } from './widgets/roi-card.component';

@Component({
  selector: 'app-dashboard3-page',
  standalone: true,
  imports: [
    OverallBalanceCardComponent,
    RoiCardComponent,
    KpiTrioRowComponent,
    MarketingReportCardComponent,
    PaymentMethodsCardComponent,
    RecentProjectsCardComponent,
    DeliveryAnalyticsCardComponent,
    EarningReportsCardComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="mm-dashboard3">
      <!-- Row 1: Overall Balance (8/12) + Return On Investment (4/12) -->
      <section class="mm-grid" aria-label="Balance and Return on Investment">
        <div class="mm-col-8">
          <app-overall-balance-card />
        </div>
        <div class="mm-col-4">
          <app-roi-card />
        </div>
      </section>

      <!-- Row 2: Total followers + Total Income + Current Balance (4/12 each) -->
      <section aria-label="Key Performance Metrics">
        <app-kpi-trio-row />
      </section>

      <!-- Row 3: Marketing Report (5/12) + Payment Methods & Transactions (7/12) -->
      <section class="mm-grid" aria-label="Marketing and Payments">
        <div class="mm-col-5">
          <app-marketing-report-card />
        </div>
        <div class="mm-col-7">
          <app-payment-methods-card />
        </div>
      </section>

      <!-- Row 4: Recent Projects Data Table (12/12) -->
      <section class="mm-grid" aria-label="Recent Projects">
        <div class="mm-col-12">
          <app-recent-projects-card />
        </div>
      </section>

      <!-- Row 5: Delivery Analytics (8/12) + Earning Reports (4/12) -->
      <section class="mm-grid" aria-label="Delivery and Earnings">
        <div class="mm-col-8">
          <app-delivery-analytics-card />
        </div>
        <div class="mm-col-4">
          <app-earning-reports-card />
        </div>
      </section>
    </div>
  `,
  styles: `
    .mm-dashboard3 {
      display: flex;
      flex-direction: column;
      gap: var(--mm-grid-gap);
    }
  `,
})
export class Dashboard3PageComponent {}
