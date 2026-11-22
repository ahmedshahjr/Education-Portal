import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/** The longest phone pattern a country can have (Country.PhonePatternMaxLength). */
export const PHONE_PATTERN_MAX_LENGTH = 200;

/** The most weekend days a country can have (Country.MaxWeekendDays). */
export const MAX_WEEKEND_DAYS = 3;

/**
 * The phone pattern rule the server applies (Country.IsValidPhonePattern): at most 200 characters and a regular expression
 * that compiles. The server uses .NET regular expressions; a pattern that compiles here but uses .NET-only syntax is still
 * checked by the server on save. A blank pattern is allowed (the country has none).
 */
export function isValidPhonePattern(pattern: string): boolean {
  if (pattern.trim() === '' || pattern.length > PHONE_PATTERN_MAX_LENGTH) {
    return false;
  }
  try {
    return new RegExp(pattern) instanceof RegExp;
  } catch {
    return false;
  }
}

/** A form validator for a phone pattern: the error `phonePattern` when it is too long or does not compile. Blank is allowed. */
export const phonePatternValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = (control.value ?? '') as string;
  if (value.trim() === '') {
    return null;
  }
  return isValidPhonePattern(value.trim()) ? null : { phonePattern: true };
};

/** A form validator for weekend days: no more than three, each day once (the server checks both). */
export const weekendDaysValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const days = (control.value ?? []) as unknown[];
  if (days.length > MAX_WEEKEND_DAYS) {
    return { maxWeekendDays: true };
  }
  return new Set(days).size === days.length ? null : { duplicateDay: true };
};

/**
 * The default language must be one of the country's languages (CountryFieldsValidator: DefaultLanguageNotOffered). The
 * comparison ignores case, as the server's does. Reads the `languages` control of the same form.
 */
export const defaultLanguageValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = (control.value ?? '') as string;
  if (value === '') {
    return null;
  }
  const languages = (control.parent?.get('languages')?.value ?? []) as string[];
  const offered = languages.some((language) => language.toLowerCase() === value.toLowerCase());
  return offered ? null : { notOffered: true };
};
