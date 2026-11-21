import { describe, expect, it } from 'vitest';
import { ClassLevel } from '../../web-api-client';
import { offeringLabel, personName } from './academics.logic';

describe('offeringLabel', () => {
  it('names the class and the academic year', () => {
    expect(offeringLabel({ level: ClassLevel.Class1, academicYear: '2026-27' })).toBe('Class 1 · 2026-27');
    expect(offeringLabel({ level: ClassLevel.Playgroup, academicYear: '2026' })).toBe('Playgroup · 2026');
  });
});

describe('personName', () => {
  it('joins the first, middle and last name', () => {
    expect(personName({ firstName: 'Sara', middleName: 'Bano', lastName: 'Ali' })).toBe('Sara Bano Ali');
  });

  it('leaves out a missing or blank middle name', () => {
    expect(personName({ firstName: 'Sara', middleName: undefined, lastName: 'Ali' })).toBe('Sara Ali');
    expect(personName({ firstName: 'Sara', middleName: null, lastName: 'Ali' })).toBe('Sara Ali');
    expect(personName({ firstName: 'Sara', middleName: '  ', lastName: 'Ali' })).toBe('Sara Ali');
  });
});
