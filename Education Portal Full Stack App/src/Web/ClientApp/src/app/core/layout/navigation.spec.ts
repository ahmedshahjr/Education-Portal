import { describe, expect, it } from 'vitest';
import { APP_NAV_SECTIONS, AppNavSection, visibleNavSections } from './navigation';

const ids = (isAdministrator: boolean, isMaster: boolean) =>
  visibleNavSections(APP_NAV_SECTIONS, isAdministrator, isMaster, new Set()).flatMap((s) => s.items.map((i) => i.id));

describe('visibleNavSections', () => {
  it('shows only the dashboard to users without a role', () => {
    expect(ids(false, true)).toEqual(['dashboard']);
  });

  it('shows branding and the subscription to a school administrator but not the master portal', () => {
    expect(ids(true, false)).toEqual(['dashboard', 'branding', 'subscription']);
  });

  it('shows the master portal to master tenant administrators', () => {
    expect(ids(true, true)).toEqual(['dashboard', 'branding', 'schools', 'countries', 'add-on-requests', 'modules']);
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
