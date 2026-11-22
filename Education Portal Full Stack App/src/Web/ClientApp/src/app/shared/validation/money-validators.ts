import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/** The largest price a price book can hold (PriceBookItem.MaxPrice). */
export const MAX_PRICE = 9_999_999_999_999_999.99;

/** The most decimal places a price can have (PriceBookItem.PriceScale). */
export const PRICE_SCALE = 2;

/**
 * The price rule the server applies (PriceBookItem.IsValidPrice): zero or more, at most 9,999,999,999,999,999.99, with at
 * most two decimal places.
 */
export function isValidPrice(value: number): boolean {
  if (!Number.isFinite(value) || value < 0 || value > MAX_PRICE) {
    return false;
  }
  // The check is done on the value times 100. Doubles cannot hold cents exactly near the largest price, so a tiny relative
  // tolerance stands in for the decimal check; the server's decimal check stays the final rule.
  const scaled = value * 100;
  return Math.abs(scaled - Math.round(scaled)) <= 1e-9 * Math.max(1, scaled);
}

/** A form validator for a price: the error `price` when the server would refuse it. Blank is left to `Validators.required`. */
export const priceValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = control.value as number | null | undefined;
  if (value === null || value === undefined) {
    return null;
  }
  return isValidPrice(value) ? null : { price: true };
};
