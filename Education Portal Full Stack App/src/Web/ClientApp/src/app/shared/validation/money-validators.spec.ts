import { FormControl } from '@angular/forms';
import { describe, expect, it } from 'vitest';
import { isValidPrice, MAX_PRICE, priceValidator } from './money-validators';

describe('isValidPrice (PriceBookItem.IsValidPrice)', () => {
  it('accepts zero and a price with two decimal places', () => {
    expect(isValidPrice(0)).toBe(true);
    expect(isValidPrice(12.5)).toBe(true);
    expect(isValidPrice(99.99)).toBe(true);
  });

  it('refuses a negative price and a third decimal place', () => {
    expect(isValidPrice(-0.01)).toBe(false);
    expect(isValidPrice(12.345)).toBe(false);
  });

  it('accepts the largest price and refuses anything above it', () => {
    expect(isValidPrice(MAX_PRICE)).toBe(true);
    expect(isValidPrice(MAX_PRICE * 10)).toBe(false);
  });

  it('refuses a value that is not a finite number', () => {
    expect(isValidPrice(Number.NaN)).toBe(false);
    expect(isValidPrice(Number.POSITIVE_INFINITY)).toBe(false);
  });
});

describe('priceValidator', () => {
  it('leaves an empty value to the required rule', () => {
    expect(priceValidator(new FormControl(null))).toBeNull();
  });

  it('reports the price error for a value the server would refuse', () => {
    expect(priceValidator(new FormControl(-5))).toEqual({ price: true });
    expect(priceValidator(new FormControl(1.001))).toEqual({ price: true });
    expect(priceValidator(new FormControl(20))).toBeNull();
  });
});
