import { describe, expect, it } from 'vitest';
import { UserType } from '../../web-api-client';
import { buildPersonChanges, needsAdmissionDetails, PersonFormValue } from './person-changes';

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
