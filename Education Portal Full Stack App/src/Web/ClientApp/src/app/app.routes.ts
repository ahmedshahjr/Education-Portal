import { Routes } from '@angular/router';
import { administratorGuard, authGuard, masterAdministratorGuard } from './core/auth/auth.guards';

export const routes: Routes = [
  {
    path: 'login',
    title: 'Sign in',
    loadComponent: () => import('./features/auth/login.page').then((m) => m.LoginPage),
  },
  {
    path: '',
    loadComponent: () => import('./shared/components/app-layout/app-layout.component').then((m) => m.AppLayoutComponent),
    canActivate: [authGuard],
    children: [
      {
        path: '',
        pathMatch: 'full',
        loadComponent: () => import('./features/home/home.page').then((m) => m.HomePage),
      },
      {
        path: 'settings/branding',
        canActivate: [administratorGuard],
        loadComponent: () =>
          import('./features/branding/branding-settings.page').then((m) => m.BrandingSettingsPage),
      },
      {
        path: 'schools',
        canActivate: [masterAdministratorGuard],
        loadComponent: () => import('./features/schools/schools.page').then((m) => m.SchoolsPage),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
