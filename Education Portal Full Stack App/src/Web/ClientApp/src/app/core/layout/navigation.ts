import { NavItem, NavSection } from '../../shared/models/navigation.models';
import { MODULE_CODES } from '../modules/modules.service';

/** Who sees a menu item. The server enforces the same rule on every request; the menu only hides what would be refused. */
export type NavAccess = 'everyone' | 'administrator' | 'school-administrator' | 'master-administrator';

export interface AppNavItem extends NavItem {
  readonly access: NavAccess;
  /** The module the item belongs to (a `MODULE_CODES` value). It is shown only when the module is on for the school. */
  readonly module?: string;
  /** The items of a submenu. A submenu is shown when at least one of them is visible. */
  readonly children?: readonly AppNavItem[];
}

export interface AppNavSection extends NavSection {
  readonly items: readonly AppNavItem[];
}

export const APP_NAV_SECTIONS: readonly AppNavSection[] = [
  {
    id: 'home',
    title: $localize`:@@nav.home:Home`,
    items: [{ id: 'dashboard', label: $localize`:@@nav.dashboard:Dashboard`, icon: 'home', route: '/', access: 'everyone' }],
  },
  {
    id: 'school',
    title: $localize`:@@nav.school:School`,
    items: [
      { id: 'subscription', label: $localize`:@@nav.subscription:My subscription`, icon: 'layers', route: '/subscription', access: 'school-administrator' },
      { id: 'campuses', label: $localize`:@@nav.campuses:Campuses`, icon: 'git-branch', route: '/campuses', access: 'school-administrator', module: MODULE_CODES.campus },
      { id: 'people', label: $localize`:@@nav.people:Users`, icon: 'users', route: '/people', access: 'school-administrator', module: MODULE_CODES.staff },
      { id: 'roles', label: $localize`:@@nav.roles:Roles`, icon: 'key-round', route: '/roles', access: 'school-administrator', module: MODULE_CODES.usersRoles },
      { id: 'class-offerings', label: $localize`:@@nav.classOfferings:Class offerings`, icon: 'book-open', route: '/class-offerings', access: 'school-administrator', module: MODULE_CODES.school },
      { id: 'audit', label: $localize`:@@nav.audit:Change history`, icon: 'file-text', route: '/audit', access: 'school-administrator' },
      { id: 'my-children', label: $localize`:@@nav.myChildren:My children`, icon: 'users', route: '/my-children', access: 'everyone', module: MODULE_CODES.students },
    ],
  },
  {
    id: 'customization',
    title: $localize`:@@nav.customization:Customization`,
    items: [
      {
        id: 'customization-menu',
        label: $localize`:@@nav.customization:Customization`,
        icon: 'sliders-horizontal',
        access: 'administrator',
        children: [
          { id: 'branding', label: $localize`:@@nav.branding:Branding`, icon: 'sliders-horizontal', route: '/settings/branding', access: 'administrator' },
          { id: 'campus-codes', label: $localize`:@@nav.campusCodes:Campus codes`, icon: 'git-branch', route: '/settings/campus-codes', access: 'school-administrator', module: MODULE_CODES.campus },
        ],
      },
    ],
  },
  {
    id: 'master',
    title: $localize`:@@nav.master:Master portal`,
    items: [
      { id: 'schools', label: $localize`:@@nav.schools:Schools`, icon: 'layers', route: '/schools', access: 'master-administrator' },
      { id: 'countries', label: $localize`:@@nav.countries:Countries and languages`, icon: 'globe', route: '/countries', access: 'master-administrator' },
      { id: 'add-on-requests', label: $localize`:@@nav.addOnRequests:Add-on requests`, icon: 'inbox', route: '/add-on-requests', access: 'master-administrator' },
      { id: 'modules', label: $localize`:@@nav.modules:Modules`, icon: 'blocks', route: '/modules', access: 'master-administrator' },
      { id: 'school-requests', label: $localize`:@@nav.schoolRequests:School requests`, icon: 'inbox', route: '/school-requests', access: 'master-administrator' },
      { id: 'pricing', label: $localize`:@@nav.pricing:Plans and prices`, icon: 'wallet', route: '/pricing', access: 'master-administrator' },
      { id: 'invoices', label: $localize`:@@nav.invoices:Invoices`, icon: 'receipt', route: '/invoices', access: 'master-administrator' },
      { id: 'development-data', label: $localize`:@@nav.developmentData:Development data`, icon: 'wrench', route: '/development-data', access: 'master-administrator' },
    ],
  },
];

/**
 * The sections and items the user may see, without empty sections. An item of a module is shown only when the module is
 * in `enabledModules` (the modules switched on for the school).
 */
export function visibleNavSections(
  sections: readonly AppNavSection[],
  isAdministrator: boolean,
  isMaster: boolean,
  enabledModules: ReadonlySet<string>
): readonly AppNavSection[] {
  const allowed = (item: AppNavItem): boolean =>
    (item.module === undefined || enabledModules.has(item.module)) &&
    (item.access === 'everyone' ||
      (item.access === 'administrator' && isAdministrator) ||
      (item.access === 'school-administrator' && isAdministrator && !isMaster) ||
      (item.access === 'master-administrator' && isAdministrator && isMaster));

  const visibleItem = (item: AppNavItem): AppNavItem | null => {
    if (!allowed(item)) {
      return null;
    }
    if (item.children) {
      const children = item.children.filter(allowed);
      return children.length > 0 ? { ...item, children } : null;
    }
    return item;
  };

  return sections
    .map((section) => ({ ...section, items: section.items.map(visibleItem).filter((item): item is AppNavItem => item !== null) }))
    .filter((section) => section.items.length > 0);
}
