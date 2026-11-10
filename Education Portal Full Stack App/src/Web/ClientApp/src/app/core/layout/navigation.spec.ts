import { describe, expect, it } from 'vitest';
import { APP_NAV_SECTIONS, visibleNavSections } from './navigation';

const ids = (isAdministrator: boolean, isMaster: boolean) =>
  visibleNavSections(APP_NAV_SECTIONS, isAdministrator, isMaster).flatMap((s) => s.items.map((i) => i.id));

describe('visibleNavSections', () => {
  it('shows only the dashboard to users without a role', () => {
    expect(ids(false, true)).toEqual(['dashboard']);
  });

  it('shows branding to a school administrator but not the master portal', () => {
    expect(ids(true, false)).toEqual(['dashboard', 'branding']);
  });

  it('shows the master portal to master tenant administrators', () => {
    expect(ids(true, true)).toEqual(['dashboard', 'branding', 'schools', 'countries']);
  });
});
