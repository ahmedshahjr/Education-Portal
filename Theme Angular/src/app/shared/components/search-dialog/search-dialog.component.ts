import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { MATERIALM_NAV_SECTIONS } from '../../../core/layout/navigation.data';
import { DashboardDataService } from '../../../core/services/dashboard-data.service';
import { AppIconName, NavItem } from '../../models/navigation.models';
import { AppIconComponent } from '../app-icon/app-icon.component';

interface SearchEntry {
  readonly title: string;
  readonly subtitle: string;
  readonly route: string;
  readonly icon: AppIconName;
}

@Component({
  selector: 'app-search-dialog',
  standalone: true,
  imports: [FormsModule, AppIconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="mm-cmd">
      <div class="mm-cmd__header">
        <app-icon name="search" [size]="18" iconClass="mm-cmd__search-icon" />
        <input
          type="text"
          class="mm-cmd__input"
          placeholder="Search dashboards, projects, pages..."
          [ngModel]="query()"
          (ngModelChange)="query.set($event)"
          autofocus
        />
        <button type="button" class="mm-cmd__close" aria-label="Close search" (click)="close()">
          <app-icon name="x" [size]="18" />
        </button>
      </div>

      <div class="mm-cmd__results">
        <div class="mm-cmd__section-title">Quick Page Links</div>
        @for (item of filteredEntries(); track item.title + item.route) {
          <button type="button" class="mm-cmd__row" (click)="navigate(item.route)">
            <span class="mm-icon-pill mm-icon-pill--square mm-icon-pill--soft-primary">
              <app-icon [name]="item.icon" [size]="18" />
            </span>
            <span class="mm-cmd__text">
              <span class="mm-cmd__title">{{ item.title }}</span>
              <span class="mm-cmd__sub">{{ item.subtitle }}</span>
            </span>
          </button>
        } @empty {
          <div class="mm-cmd__empty">No matching destinations found for "{{ query() }}".</div>
        }
      </div>
    </div>
  `,
  styles: `
    .mm-cmd {
      display: flex;
      flex-direction: column;
      background-color: var(--mm-bg-surface);
      color: var(--mm-text-primary);
    }

    .mm-cmd__header {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 16px 20px;
      border-bottom: 1px solid var(--mm-border-color);
    }

    :host ::ng-deep .mm-cmd__search-icon {
      color: var(--mm-text-muted);
    }

    .mm-cmd__input {
      flex: 1;
      border: 0;
      background: transparent;
      color: var(--mm-text-primary);
      font-size: var(--mm-text-base);
      font-family: inherit;
      outline: none;
    }

    .mm-cmd__close {
      border: 0;
      background: transparent;
      color: var(--mm-text-muted);
      width: 32px;
      height: 32px;
      border-radius: var(--mm-radius-pill);
      cursor: pointer;
      display: inline-flex;
      align-items: center;
      justify-content: center;

      &:hover {
        background-color: var(--mm-bg-hover);
        color: var(--mm-primary);
      }
    }

    .mm-cmd__results {
      max-height: 360px;
      overflow-y: auto;
      padding: 12px 16px 20px;
      display: flex;
      flex-direction: column;
      gap: 6px;
    }

    .mm-cmd__section-title {
      padding: 6px 8px;
      font-size: var(--mm-text-xs);
      font-weight: 600;
      color: var(--mm-text-muted);
      text-transform: uppercase;
      letter-spacing: 0.04em;
    }

    .mm-cmd__row {
      appearance: none;
      border: 0;
      background: transparent;
      width: 100%;
      padding: 10px 12px;
      border-radius: var(--mm-radius-md);
      display: flex;
      align-items: center;
      gap: 12px;
      cursor: pointer;
      text-align: left;
      color: var(--mm-text-primary);

      &:hover {
        background-color: var(--mm-bg-hover);
      }
    }

    .mm-cmd__text {
      display: flex;
      flex-direction: column;
      gap: 2px;
    }

    .mm-cmd__title {
      font-size: var(--mm-text-sm);
      font-weight: 600;
    }

    .mm-cmd__sub {
      font-size: var(--mm-text-xs);
      color: var(--mm-text-muted);
    }

    .mm-cmd__empty {
      padding: 24px;
      text-align: center;
      color: var(--mm-text-muted);
      font-size: var(--mm-text-sm);
    }
  `,
})
export class SearchDialogComponent {
  private readonly router = inject(Router);
  private readonly dialogRef = inject(MatDialogRef<SearchDialogComponent>);
  private readonly dataService = inject(DashboardDataService);

  protected readonly query = signal('');

  private readonly entries = computed<readonly SearchEntry[]>(() => {
    const navEntries: SearchEntry[] = [];
    const flatten = (items: readonly NavItem[], group: string): void => {
      for (const item of items) {
        if (item.route) {
          navEntries.push({
            title: item.label,
            subtitle: `${group} · ${item.route}`,
            route: item.route,
            icon: item.icon,
          });
        }
        if (item.children) {
          flatten(item.children, item.label);
        }
      }
    };
    for (const sec of MATERIALM_NAV_SECTIONS) {
      flatten(sec.items, sec.title);
    }

    const projectEntries: SearchEntry[] = this.dataService.projects().map((p) => ({
      title: p.name,
      subtitle: `Project · Leader: ${p.leader} · ${p.category}`,
      route: '/apps/projects',
      icon: 'kanban',
    }));

    return [...navEntries, ...projectEntries];
  });

  protected readonly filteredEntries = computed(() => {
    const q = this.query().trim().toLowerCase();
    if (!q) return this.entries().slice(0, 8);
    return this.entries().filter(
      (e) => e.title.toLowerCase().includes(q) || e.subtitle.toLowerCase().includes(q)
    );
  });

  protected navigate(route: string): void {
    this.dialogRef.close();
    void this.router.navigateByUrl(route);
  }

  protected close(): void {
    this.dialogRef.close();
  }
}
