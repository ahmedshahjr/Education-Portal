import { NavSection } from '../../shared/models/navigation.models';

export const MATERIALM_NAV_SECTIONS: readonly NavSection[] = [
  {
    id: 'dashboards',
    title: 'Dashboards',
    items: [
      {
        id: 'dashboard-1',
        label: 'Dashboard 1',
        icon: 'layout-grid',
        route: '/dashboards/dashboard1',
      },
      {
        id: 'dashboard-2',
        label: 'Dashboard 2',
        icon: 'bar-chart-3',
        route: '/dashboards/dashboard2',
      },
      {
        id: 'dashboard-3',
        label: 'Dashboard 3',
        icon: 'monitor',
        route: '/dashboards/dashboard3',
      },
      {
        id: 'front-pages',
        label: 'Front Pages',
        icon: 'home',
        children: [
          {
            id: 'front-landing',
            label: 'Analytics Showcase',
            icon: 'layers',
            route: '/dashboards/dashboard3',
          },
          {
            id: 'front-pricing',
            label: 'Pricing Overview',
            icon: 'dollar-sign',
            route: '/pages/reports',
          },
        ],
      },
    ],
  },
  {
    id: 'apps',
    title: 'Apps',
    items: [
      {
        id: 'projects',
        label: 'Projects',
        icon: 'kanban',
        route: '/apps/projects',
        badge: 'New',
        badgeVariant: 'primary',
      },
      {
        id: 'transactions',
        label: 'Transactions',
        icon: 'wallet',
        route: '/apps/transactions',
      },
      {
        id: 'analytics-reports',
        label: 'Earning Reports',
        icon: 'trending-up',
        route: '/pages/reports',
        badge: 'New',
        badgeVariant: 'primary',
      },
      {
        id: 'user-profile',
        label: 'User Profile',
        icon: 'user-circle',
        children: [
          {
            id: 'account-settings-sub',
            label: 'Account Setting',
            icon: 'settings',
            route: '/pages/account-settings',
          },
          {
            id: 'profile-overview',
            label: 'Profile Overview',
            icon: 'users',
            route: '/pages/account-settings',
          },
        ],
      },
    ],
  },
  {
    id: 'pages',
    title: 'Pages',
    items: [
      {
        id: 'account-setting',
        label: 'Account Setting',
        icon: 'settings',
        route: '/pages/account-settings',
      },
      {
        id: 'ui-components',
        label: 'UI Components',
        icon: 'blocks',
        route: '/pages/ui-components',
        badge: '18',
        badgeVariant: 'outlined',
      },
      {
        id: 'tables',
        label: 'Data Tables',
        icon: 'table',
        route: '/apps/projects',
        queryParams: { view: 'table' },
      },
      {
        id: 'charts',
        label: 'Charts & Radar',
        icon: 'pie-chart',
        route: '/pages/reports',
        queryParams: { view: 'radar' },
      },
    ],
  },
  {
    id: 'auth',
    title: 'Auth',
    items: [
      {
        id: 'login',
        label: 'Login',
        icon: 'lock',
        route: '/auth/login',
      },
      {
        id: 'register',
        label: 'Register',
        icon: 'user-plus',
        route: '/auth/register',
      },
      {
        id: 'forgot-password',
        label: 'Forgot Password',
        icon: 'key-round',
        route: '/auth/forgot-password',
      },
    ],
  },
];
