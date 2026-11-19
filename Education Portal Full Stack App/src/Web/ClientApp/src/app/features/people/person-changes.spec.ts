import { describe, expect, it } from 'vitest';
import { UserType } from '../../web-api-client';
import { buildPersonChanges, needsAdmissionDetails, planChildLinks, PersonFormValue, sameRoles } from './person-changes';

const form: PersonFormValue = {
  firstName: '  Ayesha ',
  middleName: '',
  lastName: ' Khan ',
  userType: UserType.Teacher,
  phone: '  ',
  email: ' ayesha@school.local ',
  oneTimePassword: 'violet harbour lantern 47',
};

describe('buildPersonChanges', () => {
  it('trims the names and the email, and leaves out blank optional values', () => {
    const changes = buildPersonChanges(form, true);

    expect(changes).toEqual({
      firstName: 'Ayesha',
      middleName: undefined,
      lastName: 'Khan',
      userType: UserType.Teacher,
      phone: undefined,
      email: 'ayesha@school.local',
      oneTimePassword: 'violet harbour lantern 47',
    });
  });

  it('asks for a login only when a person is added', () => {
    const changes = buildPersonChanges(form, false);

    expect(changes).not.toHaveProperty('email');
    expect(changes).not.toHaveProperty('oneTimePassword');
    expect(changes.firstName).toBe('Ayesha');
  });

  it('keeps a middle name and a phone when they are given', () => {
    const changes = buildPersonChanges({ ...form, middleName: ' Noor ', phone: ' +92 300 1234567 ' }, true);

    expect(changes.middleName).toBe('Noor');
    expect(changes.phone).toBe('+92 300 1234567');
  });
});

describe('needsAdmissionDetails', () => {
  it('points a student to Admissions, and nobody else', () => {
    expect(needsAdmissionDetails(UserType.Student)).toBe(true);
    expect(needsAdmissionDetails(UserType.Teacher)).toBe(false);
    expect(needsAdmissionDetails(UserType.Principal)).toBe(false);
  });
});

describe('planChildLinks', () => {
  it('links the students that are new and unlinks the ones that are no longer wanted', () => {
    expect(planChildLinks(['a', 'b'], ['b', 'c'])).toEqual({ add: ['c'], remove: ['a'] });
  });

  it('does nothing when the children are the same', () => {
    expect(planChildLinks(['a', 'b'], ['b', 'a'])).toEqual({ add: [], remove: [] });
  });

  it('links every student of a new parent and removes none', () => {
    expect(planChildLinks([], ['a', 'a', 'b'])).toEqual({ add: ['a', 'b'], remove: [] });
  });
});

describe('sameRoles', () => {
  it('is true for the same roles in any order', () => {
    expect(sameRoles(['r1', 'r2'], ['r2', 'r1'])).toBe(true);
  });

  it('is false when a role is added or removed', () => {
    expect(sameRoles(['r1'], ['r1', 'r2'])).toBe(false);
    expect(sameRoles(['r1', 'r2'], ['r1'])).toBe(false);
  });
});
