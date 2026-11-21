import { NavItem, NavSection } from '../../shared/models/navigation.models';
import { MODULE_CODES } from '../modules/modules.service';
import type { AccessRight, ModuleCode } from '../../web-api-client';

/** Who sees a menu item. The server enforces the same rule on every request; the menu only hides what would be refused. */
export type NavAccess = 'everyone' | 'administrator' | 'school-administrator' | 'master-administrator';

export interface AppNavItem extends NavItem {
  readonly access: NavAccess;
  /** The module the item belongs to (a `MODULE_CODES` value). It is shown only when the module is on for the school. */
  readonly module?: string;
  /** The right the user needs in `module` to see the item (ADR-022). Checked only when the caller passes a rights check. */
  readonly right?: AccessRight;
  /** The items of a submenu. A submenu is shown when at least one of them is visible. */
  readonly children?: readonly AppNavItem[];
}

/** Asks whether the signed-in user has a right in a module. Supplied by the layout from `RightsService`. */
export type RightCheck = (module: ModuleCode, right: AccessRight) => boolean;

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
      { id: 'academic-calendar', label: $localize`:@@nav.academicCalendar:Academic calendar`, icon: 'calendar', route: '/academic-calendar', access: 'school-administrator', module: MODULE_CODES.academicSetup },
      { id: 'school-page', label: $localize`:@@nav.schoolPage:School page`, icon: 'file-text', route: '/school-page', access: 'school-administrator', module: MODULE_CODES.school },
      { id: 'academics', label: $localize`:@@nav.academics:Courses and enrolments`, icon: 'book-open', route: '/academics', access: 'school-administrator', module: MODULE_CODES.academicSetup },
      { id: 'student-admissions', label: $localize`:@@nav.studentAdmissions:Admit a student`, icon: 'user-plus', route: '/student-admissions', access: 'school-administrator', module: MODULE_CODES.students },
      { id: 'exams', label: $localize`:@@nav.exams:Exams`, icon: 'check-square', route: '/exams', access: 'school-administrator', module: MODULE_CODES.exams },
      { id: 'timetable', label: $localize`:@@nav.timetable:Timetable`, icon: 'clock', route: '/timetable', access: 'school-administrator', module: MODULE_CODES.timetable },
      { id: 'events', label: $localize`:@@nav.events:Events`, icon: 'calendar', route: '/events', access: 'school-administrator', module: MODULE_CODES.school },
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
          { id: 'school-details', label: $localize`:@@nav.schoolDetails:School details`, icon: 'file-text', route: '/settings/school-details', access: 'school-administrator', module: MODULE_CODES.school },
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
  enabledModules: ReadonlySet<string>,
  can?: RightCheck
): readonly AppNavSection[] {
  const allowed = (item: AppNavItem): boolean =>
    (item.module === undefined || enabledModules.has(item.module)) &&
    (item.right === undefined || can === undefined || can(item.module as ModuleCode, item.right)) &&
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
