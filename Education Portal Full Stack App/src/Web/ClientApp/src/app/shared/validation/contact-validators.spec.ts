import { FormArray, FormControl } from '@angular/forms';
import { describe, expect, it } from 'vitest';
import {
  isValidPhone,
  isValidPostalCode,
  MAX_PHONES,
  phonesValidator,
  POSTAL_CODE_MAX_LENGTH,
  postalCodeValidator,
} from './contact-validators';

describe('isValidPostalCode (ContactRules.IsValidPostalCode)', () => {
  it('accepts letters, digits, spaces and hyphens starting with a letter or digit', () => {
    expect(isValidPostalCode('SW1A 1AA')).toBe(true);
    expect(isValidPostalCode('75008')).toBe(true);
    expect(isValidPostalCode('AB-12')).toBe(true);
  });

  it('refuses a leading space or hyphen and other symbols', () => {
    expect(isValidPostalCode('-AB')).toBe(false);
    expect(isValidPostalCode(' 1AB')).toBe(false);
    expect(isValidPostalCode('AB#1')).toBe(false);
  });

  it('accepts exactly 10 characters and refuses 11', () => {
    expect(isValidPostalCode('A'.repeat(POSTAL_CODE_MAX_LENGTH))).toBe(true);
    expect(isValidPostalCode('A'.repeat(POSTAL_CODE_MAX_LENGTH + 1))).toBe(false);
  });
});

describe('postalCodeValidator', () => {
  it('leaves a blank value to the required rule', () => {
    expect(postalCodeValidator(new FormControl(''))).toBeNull();
  });

  it('reports postalCode for a bad code and accepts a good one', () => {
    expect(postalCodeValidator(new FormControl('AB#1'))).toEqual({ postalCode: true });
    expect(postalCodeValidator(new FormControl('75008'))).toBeNull();
  });
});

describe('isValidPhone (ContactRules.IsValidPhone)', () => {
  it('accepts 7 to 15 digits with an optional leading plus and separators', () => {
    expect(isValidPhone('+92 300 1234567')).toBe(true);
    expect(isValidPhone('021 (555) 1234')).toBe(true);
  });

  it('refuses a letter and fewer than 7 or more than 15 digits', () => {
    expect(isValidPhone('0300-ABC1234')).toBe(false);
    expect(isValidPhone('123456')).toBe(false);
    expect(isValidPhone('1'.repeat(16))).toBe(false);
  });
});

describe('phonesValidator', () => {
  const phones = (values: string[]) => new FormArray(values.map((v) => new FormControl(v)));

  it('needs at least one filled number', () => {
    expect(phonesValidator(phones(['', '  ']))).toEqual({ required: true });
  });

  it('accepts up to five numbers and refuses six', () => {
    expect(MAX_PHONES).toBe(5);
    expect(phonesValidator(phones(['1', '2', '3', '4', '5']))).toBeNull();
    expect(phonesValidator(phones(['1', '2', '3', '4', '5', '6']))).toEqual({ maxPhones: true });
  });
});
