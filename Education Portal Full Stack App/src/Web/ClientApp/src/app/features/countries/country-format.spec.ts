import { describe, expect, it } from 'vitest';
import { formatPreview } from './country-format';

describe('formatPreview', () => {
  const at = new Date('2026-11-10T18:00:00Z');

  it("shows the time in the country's time zone and the amount in its currency", () => {
    const preview = formatPreview({ code: 'PK', defaultLanguage: 'en', timeZoneId: 'Asia/Karachi', currencyCode: 'PKR' }, at);

    expect(preview).toContain('11:00'); // 18:00 UTC is 23:00 in Karachi (UTC+5)
    expect(preview).toMatch(/Rs|PKR/);
  });

  it('gives no preview for a currency the browser does not know', () => {
    expect(formatPreview({ code: 'PK', defaultLanguage: 'en', timeZoneId: 'Asia/Karachi', currencyCode: '???' }, at)).toBe('');
  });
});
