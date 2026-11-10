/** The settings a format preview needs; matches the API's CountryDto. */
export interface CountryFormatSettings {
  readonly code: string;
  readonly defaultLanguage: string;
  readonly timeZoneId: string;
  readonly currencyCode: string;
}

/**
 * How a date and an amount look in a country's default language, for example `ur-PK`. The browser's Intl API formats
 * them with the same CLDR data the server uses, in the country's time zone, so the master tenant can check the settings.
 */
export function formatPreview(country: CountryFormatSettings, at: Date, amount = 1234.5): string {
  const culture = `${country.defaultLanguage}-${country.code}`;

  try {
    const date = new Intl.DateTimeFormat(culture, { dateStyle: 'medium', timeStyle: 'short', timeZone: country.timeZoneId }).format(at);
    const money = new Intl.NumberFormat(culture, { style: 'currency', currency: country.currencyCode }).format(amount);
    return `${date} · ${money}`;
  } catch {
    // An unknown time zone or currency in this browser: no preview rather than a broken page.
    return '';
  }
}

/** Every IANA time zone and ISO 4217 currency this browser knows, for the pickers. */
export function supportedValues(key: 'timeZone' | 'currency'): readonly string[] {
  try {
    return Intl.supportedValuesOf(key);
  } catch {
    return [];
  }
}
