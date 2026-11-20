import { describe, expect, it } from 'vitest';
import { APP_NAV_SECTIONS, AppNavSection, visibleNavSections } from './navigation';

const ids = (isAdministrator: boolean, isMaster: boolean) =>
  visibleNavSections(APP_NAV_SECTIONS, isAdministrator, isMaster, new Set()).flatMap((s) => s.items.map((i) => i.id));

describe('visibleNavSections', () => {
  it('shows only the dashboard to users without a role', () => {
    expect(ids(false, true)).toEqual(['dashboard']);
  });

  it('shows branding and the subscription to a school administrator but not the master portal', () => {
    expect(ids(true, false)).toEqual(['dashboard', 'subscription', 'audit', 'branding']);
  });

  it('shows class offerings to a school administrator only when the School module is on', () => {
    const schoolItems = (enabled: string[]) =>
      visibleNavSections(APP_NAV_SECTIONS, true, false, new Set(enabled))
        .flatMap((s) => s.items.map((i) => i.id))
        .filter((id) => id === 'class-offerings');
    expect(schoolItems(['School'])).toEqual(['class-offerings']);
    expect(schoolItems([])).toEqual([]);
  });

  it('shows the academic calendar to a school administrator only when Academic setup is on', () => {
    const items = (enabled: string[]) =>
      visibleNavSections(APP_NAV_SECTIONS, true, false, new Set(enabled))
        .flatMap((s) => s.items.map((i) => i.id))
        .filter((id) => id === 'academic-calendar');
    expect(items(['AcademicSetup'])).toEqual(['academic-calendar']);
    expect(items(['School'])).toEqual([]);
    expect(visibleNavSections(APP_NAV_SECTIONS, true, true, new Set(['AcademicSetup'])).flatMap((s) => s.items.map((i) => i.id))).not.toContain('academic-calendar');
  });

  it('shows the master portal to master tenant administrators', () => {
    expect(ids(true, true)).toEqual([
      'dashboard',
      'branding',
      'schools',
      'countries',
      'add-on-requests',
      'modules',
      'school-requests',
      'pricing',
      'invoices',
      'development-data',
    ]);
  });

  it('shows an item of a module only when the module is on for the school', () => {
    const sections: AppNavSection[] = [
      {
        id: 'academics',
        title: 'Academics',
        items: [
          { id: 'students', label: 'Students', icon: 'users', route: '/students', access: 'everyone', module: 'Students' },
          { id: 'grades', label: 'Grades', icon: 'check-square', route: '/grades', access: 'everyone', module: 'Grades' },
        ],
      },
    ];

    const visible = visibleNavSections(sections, false, false, new Set(['Students'])).flatMap((s) => s.items.map((i) => i.id));
    expect(visible).toEqual(['students']);
    expect(visibleNavSections(sections, false, false, new Set())).toEqual([]);
  });
});
