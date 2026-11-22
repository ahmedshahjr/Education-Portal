import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/** The most characters an email address can have (ContactRules.EmailMaxLength). */
export const EMAIL_MAX_LENGTH = 254;

/**
 * A plain address: a local part, an @ and a domain of one or more labels, with no spaces, no display name and no quotes.
 * Mirrors ContactRules.IsValidEmail, which accepts an address only when System.Net.Mail.MailAddress reads it back unchanged.
 * The browser cannot run MailAddress, so this pattern covers the same addresses the server accepts for everyday use; the
 * server stays the final check.
 */
const EMAIL_PATTERN = /^[A-Za-z0-9.!#$%&'*+\/=?^_`{|}~-]+@[A-Za-z0-9](?:[A-Za-z0-9-]{0,61}[A-Za-z0-9])?(?:\.[A-Za-z0-9](?:[A-Za-z0-9-]{0,61}[A-Za-z0-9])?)*$/;

/** True when the address is one the server accepts (a single-label domain such as user@localhost is accepted too). A blank value is not checked here; use `Validators.required` for that. */
export function isValidEmail(value: string): boolean {
  return value.length <= EMAIL_MAX_LENGTH && EMAIL_PATTERN.test(value) && !value.startsWith('.') && !value.includes('..');
}

/** A form validator for an email address: the error `email` when the server would refuse the address. */
export const emailValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = (control.value ?? '') as string;
  if (value.trim() === '') {
    return null;
  }
  return isValidEmail(value.trim()) ? null : { email: true };
};
