import { FormControl } from '@angular/forms';
import { describe, expect, it } from 'vitest';
import { EMAIL_MAX_LENGTH, emailValidator, isValidEmail } from './email-validators';

describe('isValidEmail (ContactRules.IsValidEmail)', () => {
  it('accepts a plain address, with sub-domains and symbols in the local part', () => {
    expect(isValidEmail('name@example.com')).toBe(true);
    expect(isValidEmail('a.b+c@sub.example.org')).toBe(true);
    expect(isValidEmail('user@localhost')).toBe(true);
  });

  it('rejects a missing @, a missing domain, spaces and a display name', () => {
    expect(isValidEmail('no-at-sign')).toBe(false);
    expect(isValidEmail('name@')).toBe(false);
    expect(isValidEmail('@example.com')).toBe(false);
    expect(isValidEmail('a b@example.com')).toBe(false);
    expect(isValidEmail('Display Name <a@example.com>')).toBe(false);
  });

  it('rejects consecutive dots and a leading dot in the local part', () => {
    expect(isValidEmail('a..b@example.com')).toBe(false);
    expect(isValidEmail('.a@example.com')).toBe(false);
  });

  it('accepts exactly 254 characters and refuses 255', () => {
    const suffix = '@example.com';
    expect(isValidEmail('a'.repeat(EMAIL_MAX_LENGTH - suffix.length) + suffix)).toBe(true);
    expect(isValidEmail('a'.repeat(EMAIL_MAX_LENGTH - suffix.length + 1) + suffix)).toBe(false);
  });
});

describe('emailValidator', () => {
  it('leaves a blank value to the required rule', () => {
    expect(emailValidator(new FormControl(''))).toBeNull();
  });

  it('reports the email error for a bad address and accepts a good one', () => {
    expect(emailValidator(new FormControl('name@'))).toEqual({ email: true });
    expect(emailValidator(new FormControl(' name@example.com '))).toBeNull();
  });
});
