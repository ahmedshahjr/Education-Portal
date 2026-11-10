import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { BreadcrumbItem } from '../../models/navigation.models';
import { BreadcrumbsComponent } from '../breadcrumbs/breadcrumbs.component';

@Component({
  selector: 'app-page-header',
  imports: [BreadcrumbsComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="mm-page-header mm-card">
      <div class="mm-page-header__left">
        <h1 class="mm-page-header__title">{{ title() }}</h1>
        @if (subtitle()) {
          <p class="mm-page-header__subtitle">{{ subtitle() }}</p>
        }
        @if (breadcrumbs().length) {
          <app-breadcrumbs [items]="breadcrumbs()" />
        }
      </div>
      <div class="mm-page-header__actions">
        <ng-content />
      </div>
    </div>
  `,
  styles: `
    .mm-page-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      flex-wrap: wrap;
      gap: var(--mm-space-4);
      padding: 18px 24px;
      margin-bottom: var(--mm-space-6);
      background-color: var(--mm-bg-surface);
      border-radius: var(--mm-radius-card);
      box-shadow: var(--mm-shadow-card);
    }

    .mm-page-header__left {
      display: flex;
      flex-direction: column;
      gap: 6px;
    }

    .mm-page-header__title {
      margin: 0;
      font-size: var(--mm-text-lg);
      font-weight: 600;
      color: var(--mm-text-primary);
    }

    .mm-page-header__subtitle {
      margin: 0;
      font-size: var(--mm-text-sm);
      color: var(--mm-text-muted);
    }

    .mm-page-header__actions {
      display: flex;
      align-items: center;
      gap: var(--mm-space-3);
      flex-wrap: wrap;
    }
  `,
})
export class PageHeaderComponent {
  readonly title = input.required<string>();
  readonly subtitle = input<string>('');
  readonly breadcrumbs = input<readonly BreadcrumbItem[]>([]);
}
