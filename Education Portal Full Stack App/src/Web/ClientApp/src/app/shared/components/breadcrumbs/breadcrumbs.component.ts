import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BreadcrumbItem } from '../../models/navigation.models';
import { AppIconComponent } from '../app-icon/app-icon.component';

@Component({
  selector: 'app-breadcrumbs',
  imports: [RouterLink, AppIconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <nav class="mm-breadcrumbs" aria-label="Breadcrumb" i18n-aria-label="@@breadcrumbs.label">
      <ol class="mm-breadcrumbs__list">
        @for (item of items(); track item.label; let isLast = $last) {
          <li class="mm-breadcrumbs__item">
            @if (item.route && !isLast) {
              <a [routerLink]="item.route" class="mm-breadcrumbs__link">{{ item.label }}</a>
            } @else {
              <span class="mm-breadcrumbs__current" [attr.aria-current]="isLast ? 'page' : null">
                {{ item.label }}
              </span>
            }
            @if (!isLast) {
              <app-icon name="chevron-right" [size]="14" iconClass="mm-breadcrumbs__sep" />
            }
          </li>
        }
      </ol>
    </nav>
  `,
  styles: `
    .mm-breadcrumbs__list {
      display: flex;
      align-items: center;
      flex-wrap: wrap;
      gap: 6px;
      list-style: none;
      margin: 0;
      padding: 0;
      font-size: var(--mm-text-xs);
    }

    .mm-breadcrumbs__item {
      display: inline-flex;
      align-items: center;
      gap: 6px;
    }

    .mm-breadcrumbs__link {
      color: var(--mm-text-muted);
      font-weight: 500;
      transition: color var(--mm-duration) var(--mm-ease);

      &:hover {
        color: var(--mm-primary);
      }
    }

    .mm-breadcrumbs__current {
      color: var(--mm-primary);
      font-weight: 600;
      background-color: var(--mm-light-primary);
      padding: 2px 10px;
      border-radius: var(--mm-radius-pill);
    }

    :host ::ng-deep .mm-breadcrumbs__sep {
      color: var(--mm-text-muted);
    }
  `,
})
export class BreadcrumbsComponent {
  readonly items = input.required<readonly BreadcrumbItem[]>();
}
