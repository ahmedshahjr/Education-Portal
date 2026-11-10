import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { Router } from '@angular/router';
import {
  ApexAxisChartSeries,
  ApexChart,
  ApexDataLabels,
  ApexFill,
  ApexGrid,
  ApexLegend,
  ApexStroke,
  ApexTooltip,
  ApexXAxis,
  ApexYAxis,
  ChartComponent,
} from 'ng-apexcharts';
import { LayoutService } from '../../../../core/layout/layout.service';
import { DashboardDataService } from '../../../../core/services/dashboard-data.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { ChartCardComponent } from '../../../../shared/components/chart-card/chart-card.component';
import { FilterOption } from '../../../../shared/components/filter-panel/filter-panel.component';
import { CardMenuAction } from '../../../../shared/components/stat-card/stat-card.component';
import { ProjectCategory } from '../../../../shared/models/dashboard.models';

@Component({
  selector: 'app-recent-projects-card',
  standalone: true,
  imports: [CurrencyPipe, MatCheckboxModule, ChartComponent, ChartCardComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-chart-card
      title="Recent Projects"
      [tabs]="tabs"
      [selectedTab]="selectedCategory()"
      (selectedTabChange)="onCategoryChange($event)"
      [menuItems]="menuItems"
      (menuAction)="onMenuAction($event)"
    >
      <div class="mm-proj-table-scroll">
        <table class="mm-proj-table">
          <thead>
            <tr>
              <th class="col-check">#</th>
              <th>Name</th>
              <th class="text-right">Budget</th>
              <th class="text-right">Team</th>
              <th class="text-right">Leader</th>
              <th class="text-right">Activity Log</th>
            </tr>
          </thead>
          <tbody>
            @for (project of projects(); track project.id) {
              <tr
                class="mm-proj-row"
                [class.is-selected]="isSelected(project.id)"
              >
                <td class="col-check">
                  <mat-checkbox
                    [checked]="isSelected(project.id)"
                    (change)="toggleRow(project.id)"
                    [attr.aria-label]="'Select ' + project.name"
                  />
                </td>
                <td>
                  <button
                    type="button"
                    class="mm-proj-name-btn"
                    (click)="openProjectsApp()"
                  >
                    <span
                      class="mm-proj-brand"
                      [style.background-color]="project.brandBg"
                      [style.color]="project.brandColor"
                    >
                      {{ project.brandMark }}
                    </span>
                    <span class="mm-proj-title">{{ project.name }}</span>
                  </button>
                </td>
                <td class="text-right mm-proj-budget">
                  {{ project.budget | currency : 'USD' : 'symbol' : project.budget < 10 ? '1.3-4' : '1.2-2' }}
                </td>
                <td class="text-right">
                  <div class="mm-avatar-stack">
                    @for (member of project.team; track member.initials) {
                      <span
                        class="mm-avatar-circle"
                        [style.background-color]="member.bg"
                        [style.color]="member.color"
                        [title]="member.name"
                      >
                        {{ member.initials }}
                      </span>
                    }
                    <span class="mm-avatar-circle mm-avatar-circle--more">
                      +{{ project.extraMembers }}
                    </span>
                  </div>
                </td>
                <td class="text-right mm-proj-leader">{{ project.leader }}</td>
                <td class="text-right col-spark">
                  <div class="mm-spark-wrap">
                    <apx-chart
                      [series]="sparkSeries(project.trendData)"
                      [chart]="sparkChart"
                      [colors]="[project.trendColor]"
                      [stroke]="sparkStroke"
                      [fill]="sparkFill"
                      [dataLabels]="noDataLabels"
                      [legend]="noLegend"
                      [grid]="noGrid"
                      [xaxis]="hiddenX"
                      [yaxis]="hiddenY"
                      [tooltip]="sparkTooltip()"
                    />
                  </div>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    </app-chart-card>
  `,
  styles: `
    .mm-proj-table-scroll {
      width: 100%;
      overflow-x: auto;
    }

    .mm-proj-table {
      width: 100%;
      min-width: 780px;
      border-collapse: collapse;

      th {
        padding: 12px 14px;
        font-size: var(--mm-text-sm);
        font-weight: 600;
        color: var(--mm-text-primary);
        border-bottom: 1px solid var(--mm-border-color);
        text-align: left;

        &.col-check {
          padding-left: 4px;
          width: 48px;
        }
      }

      td {
        padding: 16px 14px;
        border-bottom: 1px solid var(--mm-border-color);
        vertical-align: middle;

        &.col-check {
          padding-left: 4px;
        }
      }

      tbody tr:last-child td {
        border-bottom: 0;
      }

      .text-right {
        text-align: right;
      }
    }

    .mm-proj-row {
      transition: background-color var(--mm-duration) var(--mm-ease);

      &:hover,
      &.is-selected {
        background-color: var(--mm-bg-hover);
      }
    }

    .mm-proj-name-btn {
      appearance: none;
      border: 0;
      background: transparent;
      padding: 0;
      display: inline-flex;
      align-items: center;
      gap: 14px;
      cursor: pointer;
      color: var(--mm-text-body);
    }

    .mm-proj-brand {
      width: 44px;
      height: 44px;
      border-radius: var(--mm-radius-pill);
      display: inline-flex;
      align-items: center;
      justify-content: center;
      font-size: var(--mm-text-xs);
      font-weight: 700;
      flex-shrink: 0;
    }

    .mm-proj-title {
      font-size: var(--mm-text-base);
      font-weight: 500;
      color: var(--mm-text-body);
    }

    .mm-proj-budget {
      font-size: var(--mm-text-sm);
      font-weight: 500;
      color: var(--mm-text-body);
      white-space: nowrap;
    }

    .mm-avatar-stack {
      display: inline-flex;
      align-items: center;
      justify-content: flex-end;
    }

    .mm-avatar-circle {
      width: 30px;
      height: 30px;
      border-radius: var(--mm-radius-pill);
      border: 2px solid var(--mm-bg-surface);
      display: inline-flex;
      align-items: center;
      justify-content: center;
      font-size: 0.65rem;
      font-weight: 700;
      margin-left: -8px;

      &:first-child {
        margin-left: 0;
      }

      &--more {
        background-color: var(--mm-light-primary);
        color: var(--mm-primary);
      }
    }

    .mm-proj-leader {
      font-size: var(--mm-text-sm);
      color: var(--mm-text-muted);
    }

    .col-spark {
      width: 170px;
    }

    .mm-spark-wrap {
      display: inline-block;
      width: 144px;
      height: 28px;
      vertical-align: middle;
    }
  `,
})
export class RecentProjectsCardComponent {
  private readonly dataService = inject(DashboardDataService);
  private readonly layout = inject(LayoutService);
  private readonly notifications = inject(NotificationService);
  private readonly router = inject(Router);

  protected readonly selectedCategory = signal<ProjectCategory>('SaaS');
  protected readonly selectedIds = signal<Record<string, boolean>>({});

  protected readonly tabs: readonly FilterOption<ProjectCategory>[] = [
    { id: 'SaaS', label: 'SaaS' },
    { id: 'Mobile', label: 'Mobile' },
    { id: 'Others', label: 'Others' },
  ];

  protected readonly menuItems: readonly CardMenuAction[] = [
    { id: 'manage', label: 'Manage All Projects' },
    { id: 'clear', label: 'Clear Selection' },
  ];

  protected readonly projects = computed(() =>
    this.dataService.getProjectsForDashboardTab(this.selectedCategory())
  );

  protected readonly sparkChart: ApexChart = {
    type: 'area',
    height: 26,
    width: 142,
    sparkline: { enabled: true },
  };

  protected readonly sparkStroke: ApexStroke = { curve: 'smooth', width: 2 };
  protected readonly sparkFill: ApexFill = {
    type: 'gradient',
    gradient: { opacityFrom: 0.18, opacityTo: 0.01, stops: [0, 100] },
  };
  protected readonly noDataLabels: ApexDataLabels = { enabled: false };
  protected readonly noLegend: ApexLegend = { show: false };
  protected readonly noGrid: ApexGrid = { show: false };
  protected readonly hiddenX: ApexXAxis = { labels: { show: false } };
  protected readonly hiddenY: ApexYAxis = { show: false };

  protected readonly sparkTooltip = computed<ApexTooltip>(() => ({
    enabled: true,
    theme: this.layout.isDark() ? 'dark' : 'light',
    fixed: { enabled: false },
    x: { show: false },
    marker: { show: false },
  }));

  protected sparkSeries(data: readonly number[]): ApexAxisChartSeries {
    return [{ name: 'Activity', data: [...data] }];
  }

  protected onCategoryChange(cat: string): void {
    if (cat === 'SaaS' || cat === 'Mobile' || cat === 'Others') {
      this.selectedCategory.set(cat);
    }
  }

  protected isSelected(id: string): boolean {
    return !!this.selectedIds()[id];
  }

  protected toggleRow(id: string): void {
    this.selectedIds.update((curr) => ({ ...curr, [id]: !curr[id] }));
  }

  protected openProjectsApp(): void {
    void this.router.navigateByUrl('/apps/projects');
  }

  protected onMenuAction(action: string): void {
    if (action === 'manage') {
      this.openProjectsApp();
      return;
    }
    this.selectedIds.set({});
    this.notifications.notify('Project selection cleared', 'primary');
  }
}
