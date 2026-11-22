import { Routes } from '@angular/router';
import { administratorGuard, anyRightGuard, authGuard, masterAdministratorGuard, moduleGuard, parentGuard, signedInGuard } from './core/auth/auth.guards';
import { AccessRight } from './web-api-client';
import { MODULE_CODES } from './core/modules/modules.service';

export const routes: Routes = [
  {
    path: 'login',
    title: $localize`:@@login.pageTitle:Sign in`,
    loadComponent: () => import('./features/auth/login.page').then((m) => m.LoginPage),
  },
  {
    path: 'register-school',
    title: $localize`:@@registerSchool.pageTitle:Register your school`,
    loadComponent: () => import('./features/school-requests/register-school.page').then((m) => m.RegisterSchoolPage),
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
        // Adding a campus has its own page (like roles and people, ADR-024); it comes before any campus id route.
        path: 'campuses/new',
        canActivate: [administratorGuard, moduleGuard(MODULE_CODES.campus)],
        loadComponent: () => import('./features/campuses/campus-page.component').then((m) => m.CampusPageComponent),
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
        // A person's own page: `new` adds one, an id changes one (large entities get pages, ADR-024).
        path: 'people/:id',
        canActivate: [administratorGuard, moduleGuard(MODULE_CODES.staff)],
        loadComponent: () => import('./features/people/person-page.component').then((m) => m.PersonPageComponent),
      },
      {
        path: 'roles',
        canActivate: [administratorGuard, moduleGuard(MODULE_CODES.usersRoles)],
        loadComponent: () => import('./features/roles/roles.page').then((m) => m.RolesPage),
      },
      {
        // Changing a role has its own page too (its rights grid is too large for a dialog, ADR-024).
        path: 'roles/:id',
        canActivate: [administratorGuard, moduleGuard(MODULE_CODES.usersRoles)],
        loadComponent: () => import('./features/roles/role-page.component').then((m) => m.RolePageComponent),
      },
      {
        // Adding a role has its own page, not a dialog (requested).
        path: 'roles/new',
        canActivate: [administratorGuard, moduleGuard(MODULE_CODES.usersRoles)],
        loadComponent: () => import('./features/roles/role-page.component').then((m) => m.RolePageComponent),
      },
      {
        path: 'academic-calendar',
        canActivate: [administratorGuard, moduleGuard(MODULE_CODES.academicSetup)],
        loadComponent: () =>
          import('./features/academic-calendar/academic-calendar.page').then((m) => m.AcademicCalendarPage),
      },
      {
        path: 'settings/school-details',
        canActivate: [administratorGuard, moduleGuard(MODULE_CODES.school)],
        loadComponent: () =>
          import('./features/school-details/school-details.page').then((m) => m.SchoolDetailsPage),
      },
      {
        path: 'class-offerings',
        canActivate: [administratorGuard, moduleGuard(MODULE_CODES.school)],
        loadComponent: () =>
          import('./features/class-offerings/class-offerings.page').then((m) => m.ClassOfferingsPage),
      },
      {
        path: 'school-page',
        canActivate: [administratorGuard, moduleGuard(MODULE_CODES.school)],
        loadComponent: () => import('./features/school-page/school-page.page').then((m) => m.SchoolPagePage),
      },
      {
        path: 'academics',
        canActivate: [administratorGuard, moduleGuard(MODULE_CODES.academicSetup)],
        loadComponent: () => import('./features/academics/academics.page').then((m) => m.AcademicsPage),
      },
      {
        path: 'student-admissions',
        canActivate: [administratorGuard, moduleGuard(MODULE_CODES.students)],
        loadComponent: () => import('./features/academics/student-admission.page').then((m) => m.StudentAdmissionPage),
      },
      {
        path: 'exams',
        canActivate: [administratorGuard, moduleGuard(MODULE_CODES.exams)],
        loadComponent: () => import('./features/exams/exams.page').then((m) => m.ExamsPage),
      },
      {
        path: 'timetable',
        canActivate: [administratorGuard, moduleGuard(MODULE_CODES.timetable)],
        loadComponent: () => import('./features/timetable/timetable.page').then((m) => m.TimetablePage),
      },
      {
        path: 'events',
        canActivate: [administratorGuard, moduleGuard(MODULE_CODES.school)],
        loadComponent: () => import('./features/school-events/school-events.page').then((m) => m.SchoolEventsPage),
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
      {
        path: 'audit',
        canActivate: [anyRightGuard(AccessRight.ViewHistory)],
        loadComponent: () => import('./features/audit/audit.page').then((m) => m.AuditPage),
      },
      {
        // The full-screen month calendar (R5.1): the school's holidays, events and school events for any signed-in user.
        path: 'calendar',
        canActivate: [authGuard, moduleGuard(MODULE_CODES.school)],
        loadComponent: () => import('./features/calendar/calendar-month.page').then((m) => m.CalendarMonthPage),
      },
      {
        path: 'my-children',
        canActivate: [authGuard, parentGuard, moduleGuard(MODULE_CODES.students)],
        loadComponent: () => import('./features/my-children/my-children.page').then((m) => m.MyChildrenPage),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
