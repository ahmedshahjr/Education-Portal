import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { DashboardDataService } from '../../core/services/dashboard-data.service';
import { NotificationService } from '../../core/services/notification.service';
import { AppIconComponent } from '../../shared/components/app-icon/app-icon.component';
import {
  ConfirmationDialogComponent,
  ConfirmationDialogData,
} from '../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { FilterOption, FilterPanelComponent } from '../../shared/components/filter-panel/filter-panel.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { SearchInputComponent } from '../../shared/components/search-input/search-input.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { ProjectCategory, ProjectItem } from '../../shared/models/dashboard.models';
import { BreadcrumbItem } from '../../shared/models/navigation.models';
import { ProjectFormDialogComponent } from './project-form-dialog.component';

type CategoryFilter = 'All' | ProjectCategory;

@Component({
  selector: 'app-projects-page',
  standalone: true,
  imports: [
    CurrencyPipe,
    MatCardModule,
    MatButtonModule,
    MatDialogModule,
    PageHeaderComponent,
    FilterPanelComponent,
    SearchInputComponent,
    StatusBadgeComponent,
    EmptyStateComponent,
    AppIconComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-page-header
      title="Projects Directory"
      subtitle="Manage SaaS, Mobile, and Design System initiatives with live budget tracking"
      [breadcrumbs]="breadcrumbs"
    >
      <button mat-flat-button color="primary" type="button" (click)="openCreateModal()">
        <app-icon name="plus" [size]="16" />
        <span>New Project</span>
      </button>
    </app-page-header>

    <mat-card class="mm-card mm-elevated">
      <mat-card-content>
        <div class="mm-projects-toolbar">
          <div class="mm-projects-search">
            <app-search-input
              [value]="searchQuery()"
              (valueChange)="searchQuery.set($event)"
              placeholder="Search projects or leaders..."
            />
          </div>

          <app-filter-panel
            [options]="categoryTabs"
            [selected]="selectedCategory()"
            (selectedChange)="selectedCategory.set($event)"
          />
        </div>

        @if (filteredProjects().length === 0) {
          <app-empty-state
            title="No matching projects"
            description="No projects match your current filter or search query."
            actionLabel="Reset Filters"
            (action)="resetFilters()"
          />
        } @else {
          <div class="mm-table-scroll">
            <table class="mm-crud-table">
              <thead>
                <tr>
                  <th>Project</th>
                  <th>Category</th>
                  <th>Leader</th>
                  <th>Status</th>
                  <th class="text-right">Budget</th>
                  <th class="text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (item of filteredProjects(); track item.id) {
                  <tr>
                    <td>
                      <div class="mm-crud-name">
                        <span
                          class="mm-crud-badge"
                          [style.background-color]="item.brandBg"
                          [style.color]="item.brandColor"
                        >
                          {{ item.brandMark }}
                        </span>
                        <div>
                          <div class="mm-crud-title">{{ item.name }}</div>
                          <div class="mm-crud-sub">Updated {{ item.updatedAt }}</div>
                        </div>
                      </div>
                    </td>
                    <td>{{ item.category }}</td>
                    <td>{{ item.leader }}</td>
                    <td>
                      <app-status-badge
                        [label]="item.status"
                        [tone]="item.status === 'Active' ? 'success' : item.status === 'In Review' ? 'warning' : 'primary'"
                      />
                    </td>
                    <td class="text-right font-medium">
                      {{ item.budget | currency : 'USD' : 'symbol' : item.budget < 10 ? '1.3-4' : '1.2-2' }}
                    </td>
                    <td class="text-right">
                      <div class="mm-crud-actions">
                        <button
                          mat-icon-button
                          type="button"
                          [attr.aria-label]="'Edit ' + item.name"
                          (click)="openEditModal(item)"
                        >
                          <app-icon name="edit-3" [size]="16" />
                        </button>
                        <button
                          mat-icon-button
                          type="button"
                          class="mm-delete-btn"
                          [attr.aria-label]="'Delete ' + item.name"
                          (click)="confirmDelete(item)"
                        >
                          <app-icon name="trash-2" [size]="16" />
                        </button>
                      </div>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      </mat-card-content>
    </mat-card>
  `,
  styles: `
    .mm-projects-toolbar {
      display: flex;
      align-items: center;
      justify-content: space-between;
      flex-wrap: wrap;
      gap: 14px;
      margin-bottom: 20px;
    }

    .mm-projects-search {
      width: 300px;
      max-width: 100%;
    }

    .mm-table-scroll {
      overflow-x: auto;
    }

    .mm-crud-table {
      width: 100%;
      min-width: 720px;
      border-collapse: collapse;

      th {
        padding: 12px 14px;
        text-align: left;
        font-size: var(--mm-text-sm);
        font-weight: 600;
        color: var(--mm-text-primary);
        border-bottom: 1px solid var(--mm-border-color);
      }

      td {
        padding: 16px 14px;
        font-size: var(--mm-text-sm);
        color: var(--mm-text-body);
        border-bottom: 1px solid var(--mm-border-color);
      }

      .text-right {
        text-align: right;
      }
    }

    .mm-crud-name {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .mm-crud-badge {
      width: 40px;
      height: 40px;
      border-radius: var(--mm-radius-pill);
      display: inline-flex;
      align-items: center;
      justify-content: center;
      font-size: var(--mm-text-xs);
      font-weight: 700;
    }

    .mm-crud-title {
      font-weight: 600;
      color: var(--mm-text-primary);
    }

    .mm-crud-sub {
      font-size: var(--mm-text-xs);
      color: var(--mm-text-muted);
    }

    .mm-crud-actions {
      display: inline-flex;
      align-items: center;
      gap: 4px;
    }

    .mm-delete-btn {
      color: var(--mm-error);
    }
  `,
})
export class ProjectsPageComponent {
  private readonly dataService = inject(DashboardDataService);
  private readonly notifications = inject(NotificationService);
  private readonly dialog = inject(MatDialog);

  protected readonly breadcrumbs: readonly BreadcrumbItem[] = [
    { label: 'Dashboard 3', route: '/dashboards/dashboard3' },
    { label: 'Projects' },
  ];

  protected readonly categoryTabs: readonly FilterOption<CategoryFilter>[] = [
    { id: 'All', label: 'All' },
    { id: 'SaaS', label: 'SaaS' },
    { id: 'Mobile', label: 'Mobile' },
    { id: 'Others', label: 'Others' },
  ];

  protected readonly selectedCategory = signal<CategoryFilter>('All');
  protected readonly searchQuery = signal('');

  protected readonly filteredProjects = computed(() => {
    const cat = this.selectedCategory();
    const q = this.searchQuery().trim().toLowerCase();

    return this.dataService.projects().filter((item) => {
      const catMatch = cat === 'All' || item.category === cat;
      const queryMatch =
        !q ||
        item.name.toLowerCase().includes(q) ||
        item.leader.toLowerCase().includes(q) ||
        item.category.toLowerCase().includes(q);
      return catMatch && queryMatch;
    });
  });

  protected resetFilters(): void {
    this.selectedCategory.set('All');
    this.searchQuery.set('');
  }

  protected openCreateModal(): void {
    const ref = this.dialog.open(ProjectFormDialogComponent, {
      data: {},
      maxWidth: '94vw',
    });

    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      const initials = result.name.slice(0, 2).toUpperCase();
      this.dataService.addProject({
        name: result.name,
        budget: Number(result.budget),
        leader: result.leader,
        category: result.category,
        status: result.status,
        brandMark: initials,
        brandColor: '#ffffff',
        brandBg: '#00a1ff',
        extraMembers: 2,
        trendColor: '#00ceb6',
      });
      this.notifications.notify(`Project "${result.name}" created`, 'success');
    });
  }

  protected openEditModal(project: ProjectItem): void {
    const ref = this.dialog.open(ProjectFormDialogComponent, {
      data: { project },
      maxWidth: '94vw',
    });

    ref.afterClosed().subscribe((result) => {
      if (!result) return;
      this.dataService.updateProject(project.id, {
        name: result.name,
        budget: Number(result.budget),
        leader: result.leader,
        category: result.category,
        status: result.status,
      });
      this.notifications.notify(`Project "${result.name}" updated`, 'success');
    });
  }

  protected confirmDelete(project: ProjectItem): void {
    const dialogData: ConfirmationDialogData = {
      title: 'Delete Project',
      message: `Are you sure you want to remove "${project.name}"? This action cannot be undone.`,
      confirmText: 'Delete',
      destructive: true,
    };

    const ref = this.dialog.open(ConfirmationDialogComponent, {
      data: dialogData,
      width: '420px',
      maxWidth: '92vw',
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.dataService.deleteProject(project.id);
        this.notifications.notify(`Removed "${project.name}"`, 'warning');
      }
    });
  }
}
