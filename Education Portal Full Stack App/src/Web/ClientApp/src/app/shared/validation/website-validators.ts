import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/** The most characters a website address can have (ContactRules.WebsiteMaxLength). */
export const WEBSITE_MAX_LENGTH = 2048;

/**
 * The website rule the server applies (ContactRules.IsValidWebsite): an absolute http or https address, up to 2048
 * characters. Mirrored here so a bad address is shown in the form before any request is sent. A blank value is not checked
 * here; use `Validators.required` for that.
 */
export function isValidWebsite(value: string): boolean {
  if (value.length > WEBSITE_MAX_LENGTH) {
    return false;
  }
  try {
    const uri = new URL(value);
    return uri.protocol === 'http:' || uri.protocol === 'https:';
  } catch {
    return false;
  }
}

/** A form validator for a website: the error `website` when the server would refuse the address. */
export const websiteValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = (control.value ?? '') as string;
  if (value.trim() === '') {
    return null;
  }
  return isValidWebsite(value.trim()) ? null : { website: true };
};
