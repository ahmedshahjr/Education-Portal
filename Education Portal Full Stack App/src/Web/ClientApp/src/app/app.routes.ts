import { Routes } from '@angular/router';
import { administratorGuard, authGuard, masterAdministratorGuard, moduleGuard, signedInGuard } from './core/auth/auth.guards';
import { MODULE_CODES } from './core/modules/modules.service';

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
        path: 'settings/campus-codes',
        canActivate: [administratorGuard, moduleGuard(MODULE_CODES.campus)],
        loadComponent: () => import('./features/settings/campus-codes.page').then((m) => m.CampusCodesPage),
      },
      {
        path: 'campuses',
        canActivate: [administratorGuard, moduleGuard(MODULE_CODES.campus)],
        loadComponent: () => import('./features/campuses/campuses.page').then((m) => m.CampusesPage),
      },
      {
        path: 'subscription',
        canActivate: [administratorGuard],
        loadComponent: () => import('./features/subscription/my-subscription.page').then((m) => m.MySubscriptionPage),
      },
      {
        path: 'schools',
        canActivate: [masterAdministratorGuard],
        loadComponent: () => import('./features/schools/schools.page').then((m) => m.SchoolsPage),
      },
      {
        path: 'schools/:schoolId',
        canActivate: [masterAdministratorGuard],
        loadComponent: () => import('./features/schools/school-edit.page').then((m) => m.SchoolEditPage),
      },
      {
        path: 'schools/:schoolId/branding',
        canActivate: [masterAdministratorGuard],
        loadComponent: () => import('./features/schools/school-branding.page').then((m) => m.SchoolBrandingPage),
      },
      {
        path: 'add-on-requests',
        canActivate: [masterAdministratorGuard],
        loadComponent: () => import('./features/add-on-requests/add-on-requests.page').then((m) => m.AddOnRequestsPage),
      },
      {
        path: 'development-data',
        canActivate: [masterAdministratorGuard],
        loadComponent: () =>
          import('./features/development-data/development-data.page').then((m) => m.DevelopmentDataPage),
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
      {
        path: 'people',
        canActivate: [administratorGuard, moduleGuard(MODULE_CODES.staff)],
        loadComponent: () => import('./features/people/people.page').then((m) => m.PeoplePage),
      },
      {
        path: 'roles',
        canActivate: [administratorGuard, moduleGuard(MODULE_CODES.usersRoles)],
        loadComponent: () => import('./features/roles/roles.page').then((m) => m.RolesPage),
      },
      {
        path: 'class-offerings',
        canActivate: [administratorGuard, moduleGuard(MODULE_CODES.school)],
        loadComponent: () =>
          import('./features/class-offerings/class-offerings.page').then((m) => m.ClassOfferingsPage),
      },
      {
        path: 'school-requests',
        canActivate: [masterAdministratorGuard],
        loadComponent: () =>
          import('./features/school-requests/school-requests-queue.page').then((m) => m.SchoolRequestsQueuePage),
      },
      {
        path: 'pricing',
        canActivate: [masterAdministratorGuard],
        loadComponent: () => import('./features/pricing/pricing.page').then((m) => m.PricingPage),
      },
      {
        path: 'invoices',
        canActivate: [masterAdministratorGuard],
        loadComponent: () => import('./features/invoices/invoices.page').then((m) => m.InvoicesPage),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
