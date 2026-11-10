import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'auth',
    children: [
      {
        path: 'login',
        title: 'Sign In - MaterialM Angular 22',
        data: { mode: 'login' },
        loadComponent: () =>
          import('./features/authentication/auth.page').then((m) => m.AuthPageComponent),
      },
      {
        path: 'register',
        title: 'Register - MaterialM Angular 22',
        data: { mode: 'register' },
        loadComponent: () =>
          import('./features/authentication/auth.page').then((m) => m.AuthPageComponent),
      },
      {
        path: 'forgot-password',
        title: 'Forgot Password - MaterialM Angular 22',
        data: { mode: 'forgot' },
        loadComponent: () =>
          import('./features/authentication/auth.page').then((m) => m.AuthPageComponent),
      },
      { path: '', redirectTo: 'login', pathMatch: 'full' },
    ],
  },
  {
    path: '',
    loadComponent: () =>
      import('./shared/components/app-layout/app-layout.component').then(
        (m) => m.AppLayoutComponent
      ),
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboards/dashboard3', pathMatch: 'full' },
      {
        path: 'dashboards',
        children: [
          {
            path: 'dashboard1',
            title: 'Dashboard 1 - MaterialM Angular 22',
            data: { variant: '1' },
            loadComponent: () =>
              import('./features/dashboard/dashboard-overview/dashboard-overview.page').then(
                (m) => m.DashboardOverviewPageComponent
              ),
          },
          {
            path: 'dashboard2',
            title: 'Dashboard 2 - MaterialM Angular 22',
            data: { variant: '2' },
            loadComponent: () =>
              import('./features/dashboard/dashboard-overview/dashboard-overview.page').then(
                (m) => m.DashboardOverviewPageComponent
              ),
          },
          {
            path: 'dashboard3',
            title: 'Dashboard 3 - MaterialM Angular 22',
            loadComponent: () =>
              import('./features/dashboard/dashboard3/dashboard3.page').then(
                (m) => m.Dashboard3PageComponent
              ),
          },
          { path: '', redirectTo: 'dashboard3', pathMatch: 'full' },
        ],
      },
      {
        path: 'apps',
        children: [
          {
            path: 'projects',
            title: 'Projects - MaterialM Angular 22',
            loadComponent: () =>
              import('./features/projects/projects.page').then((m) => m.ProjectsPageComponent),
          },
          {
            path: 'transactions',
            title: 'Transactions - MaterialM Angular 22',
            loadComponent: () =>
              import('./features/transactions/transactions.page').then(
                (m) => m.TransactionsPageComponent
              ),
          },
        ],
      },
      {
        path: 'pages',
        children: [
          {
            path: 'reports',
            title: 'Earning Reports - MaterialM Angular 22',
            loadComponent: () =>
              import('./features/reports/reports.page').then((m) => m.ReportsPageComponent),
          },
          {
            path: 'account-settings',
            title: 'Account Settings - MaterialM Angular 22',
            loadComponent: () =>
              import('./features/settings/account-settings.page').then(
                (m) => m.AccountSettingsPageComponent
              ),
          },
          {
            path: 'ui-components',
            title: 'UI Components - MaterialM Angular 22',
            loadComponent: () =>
              import('./features/ui-showcase/ui-showcase.page').then(
                (m) => m.UiShowcasePageComponent
              ),
          },
        ],
      },
    ],
  },
  { path: '**', redirectTo: 'dashboards/dashboard3' },
];
