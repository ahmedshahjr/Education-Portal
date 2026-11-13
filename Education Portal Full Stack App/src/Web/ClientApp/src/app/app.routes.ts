import { Routes } from '@angular/router';
import { administratorGuard, authGuard, masterAdministratorGuard, signedInGuard } from './core/auth/auth.guards';

export const routes: Routes = [
  {
    path: 'login',
    title: $localize`:@@login.pageTitle:Sign in`,
    loadComponent: () => import('./features/auth/login.page').then((m) => m.LoginPage),
  },
  {
    path: 'request-school',
    title: $localize`:@@requestSchool.pageTitle:Request a school`,
    loadComponent: () => import('./features/school-requests/request-school.page').then((m) => m.RequestSchoolPage),
  },
  {
    path: 'change-password',
    title: $localize`:@@changePassword.pageTitle:Choose your password`,
    canActivate: [signedInGuard],
    loadComponent: () => import('./features/account/change-password.page').then((m) => m.ChangePasswordPage),
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
      {
        path: 'countries',
        canActivate: [masterAdministratorGuard],
        loadComponent: () => import('./features/countries/countries.page').then((m) => m.CountriesPage),
      },
      {
        path: 'modules',
        canActivate: [masterAdministratorGuard],
        loadComponent: () => import('./features/modules/modules.page').then((m) => m.ModulesPage),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
