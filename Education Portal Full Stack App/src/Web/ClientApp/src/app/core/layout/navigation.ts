import { NavItem, NavSection } from '../../shared/models/navigation.models';

/** Who sees a menu item. The server enforces the same rule on every request; the menu only hides what would be refused. */
export type NavAccess = 'everyone' | 'administrator' | 'master-administrator';

export interface AppNavItem extends NavItem {
  readonly access: NavAccess;
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
    items: [{ id: 'branding', label: $localize`:@@nav.branding:Branding`, icon: 'sliders-horizontal', route: '/settings/branding', access: 'administrator' }],
  },
  {
    id: 'master',
    title: $localize`:@@nav.master:Master portal`,
    items: [
      { id: 'schools', label: $localize`:@@nav.schools:Schools`, icon: 'layers', route: '/schools', access: 'master-administrator' },
      { id: 'countries', label: $localize`:@@nav.countries:Countries and languages`, icon: 'globe', route: '/countries', access: 'master-administrator' },
    ],
  },
];

/** The sections and items the user may see, without empty sections. */
export function visibleNavSections(
  sections: readonly AppNavSection[],
  isAdministrator: boolean,
  isMaster: boolean
): readonly AppNavSection[] {
  const allowed = (access: NavAccess): boolean =>
    access === 'everyone' ||
    (access === 'administrator' && isAdministrator) ||
    (access === 'master-administrator' && isAdministrator && isMaster);

  return sections
    .map((section) => ({ ...section, items: section.items.filter((item) => allowed(item.access)) }))
    .filter((section) => section.items.length > 0);
}
