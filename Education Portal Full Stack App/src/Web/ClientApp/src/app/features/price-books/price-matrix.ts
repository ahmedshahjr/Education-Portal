import { BillingCycle, PriceDto } from '../../web-api-client';

/** One row of the price matrix: a plan or a paid module of the catalogue. */
export interface PriceRow {
  readonly kind: 'plan' | 'module';
  readonly code: string;
  readonly name: string;
  readonly inactive: boolean;
}

/** The price of a row for one billing cycle, or null when the price book has none. */
export function priceFor(prices: readonly PriceDto[], row: PriceRow, cycle: BillingCycle): number | null {
  const found = prices.find(
    (price) => price.billingCycle === cycle && (row.kind === 'plan' ? price.planCode === row.code : price.moduleCode === row.code)
  );
  return found ? found.price : null;
}

/** How many paid modules have no monthly price: a school cannot be given such a module. */
export function unpricedModuleCount(rows: readonly PriceRow[], prices: readonly PriceDto[]): number {
  return rows.filter((row) => row.kind === 'module' && priceFor(prices, row, BillingCycle.Monthly) === null).length;
}
