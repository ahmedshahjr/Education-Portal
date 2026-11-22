import { FormControl } from '@angular/forms';
import { describe, expect, it } from 'vitest';
import { isValidWebsite, websiteValidator, WEBSITE_MAX_LENGTH } from './website-validators';

describe('isValidWebsite (ContactRules.IsValidWebsite)', () => {
  it('accepts an absolute http or https address', () => {
    expect(isValidWebsite('https://school1.com')).toBe(true);
    expect(isValidWebsite('http://school1.com/about?x=1')).toBe(true);
  });

  it('rejects a relative address, another scheme and a bare host', () => {
    expect(isValidWebsite('school1.com')).toBe(false);
    expect(isValidWebsite('/about')).toBe(false);
    expect(isValidWebsite('ftp://school1.com')).toBe(false);
    expect(isValidWebsite('javascript:alert(1)')).toBe(false);
    expect(isValidWebsite('not a url')).toBe(false);
  });

  it('accepts exactly 2048 characters and refuses 2049', () => {
    const prefix = 'https://a.com/';
    expect(isValidWebsite(prefix + 'a'.repeat(WEBSITE_MAX_LENGTH - prefix.length))).toBe(true);
    expect(isValidWebsite(prefix + 'a'.repeat(WEBSITE_MAX_LENGTH - prefix.length + 1))).toBe(false);
  });
});

describe('websiteValidator', () => {
  it('leaves a blank value to the required rule', () => {
    expect(websiteValidator(new FormControl(''))).toBeNull();
    expect(websiteValidator(new FormControl('   '))).toBeNull();
  });

  it('reports the website error for a bad address and accepts a good one', () => {
    expect(websiteValidator(new FormControl('school1.com'))).toEqual({ website: true });
    expect(websiteValidator(new FormControl(' https://school1.com '))).toBeNull();
  });
});
