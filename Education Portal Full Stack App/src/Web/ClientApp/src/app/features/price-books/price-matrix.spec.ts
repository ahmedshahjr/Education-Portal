import { describe, expect, it } from 'vitest';
import { BillingCycle, PriceDto } from '../../web-api-client';
import { PriceRow, priceFor, unpricedModuleCount } from './price-matrix';

const plan: PriceRow = { kind: 'plan', code: 'STANDARD', name: 'Standard', inactive: false };
const module: PriceRow = { kind: 'module', code: 'FEES', name: 'Fees', inactive: false };

const prices = [
  new PriceDto({ planCode: 'STANDARD', billingCycle: BillingCycle.Monthly, price: 65000 }),
  new PriceDto({ planCode: 'STANDARD', billingCycle: BillingCycle.Yearly, price: 650000 }),
  new PriceDto({ moduleCode: 'FEES', billingCycle: BillingCycle.Yearly, price: 80000 }),
];

describe('price matrix', () => {
  it('finds the price of a plan for its billing cycle', () => {
    expect(priceFor(prices, plan, BillingCycle.Monthly)).toBe(65000);
    expect(priceFor(prices, plan, BillingCycle.Yearly)).toBe(650000);
  });

  it('gives null when the cycle has no price for the row', () => {
    expect(priceFor(prices, module, BillingCycle.Monthly)).toBeNull();
  });

  it('does not match a plan price to a module with the same code', () => {
    const sameCode: PriceRow = { kind: 'module', code: 'STANDARD', name: 'Standard', inactive: false };

    expect(priceFor(prices, sameCode, BillingCycle.Monthly)).toBeNull();
  });

  it('counts only the paid modules with no monthly price', () => {
    const unpriced = unpricedModuleCount([plan, module], prices);

    expect(unpriced).toBe(1);
  });

  it('counts no unpriced modules when every paid module has a monthly price', () => {
    const monthly = [...prices, new PriceDto({ moduleCode: 'FEES', billingCycle: BillingCycle.Monthly, price: 8000 })];

    expect(unpricedModuleCount([plan, module], monthly)).toBe(0);
  });
});
