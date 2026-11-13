import { NavItem, NavSection } from '../../shared/models/navigation.models';

/** Who sees a menu item. The server enforces the same rule on every request; the menu only hides what would be refused. */
export type NavAccess = 'everyone' | 'administrator' | 'school-administrator' | 'master-administrator';

export interface AppNavItem extends NavItem {
  readonly access: NavAccess;
  /** The module the item belongs to (a `MODULE_CODES` value). It is shown only when the module is on for the school. */
  readonly module?: string;
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
      { id: 'branding', label: $localize`:@@nav.branding:Branding`, icon: 'sliders-horizontal', route: '/settings/branding', access: 'administrator' },
      { id: 'subscription', label: $localize`:@@nav.subscription:My subscription`, icon: 'layers', route: '/subscription', access: 'school-administrator' },
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

  return sections
    .map((section) => ({ ...section, items: section.items.filter(allowed) }))
    .filter((section) => section.items.length > 0);
}
