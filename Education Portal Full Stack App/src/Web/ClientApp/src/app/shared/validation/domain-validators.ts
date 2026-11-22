import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/** The most characters a host name can have (School.DomainMaxLength). */
export const DOMAIN_MAX_LENGTH = 253;

/** The most characters one label of a host name can have (RFC 1035). */
export const DOMAIN_LABEL_MAX_LENGTH = 63;

/**
 * The host name the server stores (School.NormalizeDomain): trimmed, trailing dots removed and lower-cased. Upper-case input
 * is therefore accepted, as on the server.
 */
export function normalizeDomain(value: string): string {
  return value.trim().replace(/\.+$/, '').toLowerCase();
}

/**
 * The host name rule the server applies (School.IsValidDomain) to the normalised name: dot-separated labels of lower-case
 * letters, digits and hyphens, each label 1 to 63 characters and neither starting nor ending with a hyphen; the whole name
 * up to 253 characters.
 */
export function isValidDomain(value: string): boolean {
  return (
    value.length > 0 &&
    value.length <= DOMAIN_MAX_LENGTH &&
    value.split('.').every(
      (label) =>
        label.length > 0 &&
        label.length <= DOMAIN_LABEL_MAX_LENGTH &&
        !label.startsWith('-') &&
        !label.endsWith('-') &&
        /^[a-z0-9-]+$/.test(label)
    )
  );
}

/** A form validator for a school domain: the error `domain` when the server would refuse the normalised name. */
export const domainValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = (control.value ?? '') as string;
  if (value.trim() === '') {
    return null;
  }
  return isValidDomain(normalizeDomain(value)) ? null : { domain: true };
};
