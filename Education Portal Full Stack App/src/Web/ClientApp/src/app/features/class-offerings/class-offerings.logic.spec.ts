import { describe, expect, it } from 'vitest';
import { rowsForCampus } from './class-offerings.logic';

// AC4.2: choosing a campus filters the rows; "All campuses" shows every row.
describe('rowsForCampus (AC4.2)', () => {
  const school = { id: 'school', campusId: null };
  const north = { id: 'north', campusId: 'campus-north' };
  const south = { id: 'south', campusId: 'campus-south' };
  const rows = [school, north, south];

  it('shows every row for "All campuses"', () => {
    expect(rowsForCampus(rows, null)).toEqual(rows);
  });

  it('shows only the chosen campus and the school-wide rows', () => {
    expect(rowsForCampus(rows, 'campus-north')).toEqual([school, north]);
  });

  it('treats an undefined campus as school-wide', () => {
    const undefinedCampus = { id: 'undef', campusId: undefined };
    expect(rowsForCampus([undefinedCampus, south], 'campus-north')).toEqual([undefinedCampus]);
  });

  it('shows nothing but school-wide rows for a campus with no offerings', () => {
    expect(rowsForCampus([north, south], 'campus-east')).toEqual([]);
  });

  it('returns an empty list for no rows', () => {
    expect(rowsForCampus([], 'campus-north')).toEqual([]);
    expect(rowsForCampus([], null)).toEqual([]);
  });
});
