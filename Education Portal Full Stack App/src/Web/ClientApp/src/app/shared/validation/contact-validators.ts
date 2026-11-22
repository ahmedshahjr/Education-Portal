import { AbstractControl, FormArray, FormControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * The phone rule the server applies (ContactRules.IsValidPhone, src/Domain/ValueObjects/ContactRules.cs). Mirrored here so a
 * wrong number is shown in the form before any request is sent (AGENTS.md section 6, rule: client mirrors server validation).
 * Keep the two in step: the same length, the same pattern and the same digit count.
 */
export const PHONE_MAX_LENGTH = 30;
export const PHONE_MIN_DIGITS = 7;
export const PHONE_MAX_DIGITS = 15;

const PHONE_PATTERN = /^\+?[0-9][0-9 ()-]*$/;

/** True when the number is one the server accepts. A blank value is not checked here; use `Validators.required` for that. */
export function isValidPhone(value: string): boolean {
  return (
    value.length <= PHONE_MAX_LENGTH &&
    PHONE_PATTERN.test(value) &&
    [...value].filter((ch) => ch >= '0' && ch <= '9').length >= PHONE_MIN_DIGITS &&
    [...value].filter((ch) => ch >= '0' && ch <= '9').length <= PHONE_MAX_DIGITS
  );
}

/** A form validator for a phone number: the error `phone` when the number is not one the server accepts. */
export const phoneValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = (control.value ?? '') as string;
  if (value.trim() === '') {
    return null;
  }
  return isValidPhone(value.trim()) ? null : { phone: true };
};

/** The most phone numbers a campus can list (ContactRules and the campus command allow five). */
export const MAX_PHONES = 5;

/** The most characters a postal code can have (ContactRules.PostalCodeMaxLength). */
export const POSTAL_CODE_MAX_LENGTH = 10;

const POSTAL_CODE_PATTERN = /^[A-Za-z0-9][A-Za-z0-9 -]*$/;

/**
 * The postal code rule the server applies (ContactRules.IsValidPostalCode): letters, digits, spaces and hyphens, up to 10
 * characters, starting with a letter or digit. A blank value is not checked here; use `Validators.required` for that.
 */
export function isValidPostalCode(value: string): boolean {
  return value.length <= POSTAL_CODE_MAX_LENGTH && POSTAL_CODE_PATTERN.test(value);
}

/** A form validator for a postal code: the error `postalCode` when the server would refuse it. */
export const postalCodeValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = (control.value ?? '') as string;
  if (value.trim() === '') {
    return null;
  }
  return isValidPostalCode(value.trim()) ? null : { postalCode: true };
};

/**
 * The campus form's phones: between one and five numbers, none blank. Mirrors the campus command's phone list rule, so a
 * list the server would refuse is shown in the form first.
 */
export function phonesValidator(control: AbstractControl): ValidationErrors | null {
  const array = control as FormArray<FormControl<string>>;
  const filled = array.controls.map((c) => c.value.trim()).filter((v) => v.length > 0);
  if (filled.length === 0) {
    return { required: true };
  }
  return filled.length > MAX_PHONES ? { maxPhones: true } : null;
}
