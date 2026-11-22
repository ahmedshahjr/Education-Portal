import { FormControl } from '@angular/forms';
import { describe, expect, it } from 'vitest';
import { DOMAIN_LABEL_MAX_LENGTH, DOMAIN_MAX_LENGTH, domainValidator, isValidDomain, normalizeDomain } from './domain-validators';

describe('normalizeDomain (School.NormalizeDomain)', () => {
  it('trims, removes trailing dots and lower-cases', () => {
    expect(normalizeDomain('  School1.COM. ')).toBe('school1.com');
    expect(normalizeDomain('school1.com..')).toBe('school1.com');
  });
});

describe('isValidDomain (School.IsValidDomain)', () => {
  it('accepts host names such as school1.com and localhost', () => {
    expect(isValidDomain('school1.com')).toBe(true);
    expect(isValidDomain('demo.localhost')).toBe(true);
    expect(isValidDomain('localhost')).toBe(true);
  });

  it('rejects a scheme, a path, a port, underscores, spaces and empty labels', () => {
    expect(isValidDomain('http://school1.com')).toBe(false);
    expect(isValidDomain('school1.com/path')).toBe(false);
    expect(isValidDomain('school1.com:8080')).toBe(false);
    expect(isValidDomain('under_score.com')).toBe(false);
    expect(isValidDomain('has space.com')).toBe(false);
    expect(isValidDomain('a..b')).toBe(false);
    expect(isValidDomain('')).toBe(false);
  });

  it('rejects a label that starts or ends with a hyphen', () => {
    expect(isValidDomain('-school.com')).toBe(false);
    expect(isValidDomain('school-.com')).toBe(false);
    expect(isValidDomain('school-1.com')).toBe(true);
  });

  it('accepts a label of exactly 63 characters and refuses 64', () => {
    expect(isValidDomain('a'.repeat(DOMAIN_LABEL_MAX_LENGTH) + '.com')).toBe(true);
    expect(isValidDomain('a'.repeat(DOMAIN_LABEL_MAX_LENGTH + 1) + '.com')).toBe(false);
  });

  it('accepts a name of exactly 253 characters and refuses 254', () => {
    const name = (last: number) => `${'a'.repeat(63)}.${'a'.repeat(63)}.${'a'.repeat(63)}.${'a'.repeat(last)}`;
    expect(name(61).length).toBe(DOMAIN_MAX_LENGTH);
    expect(isValidDomain(name(61))).toBe(true);
    expect(isValidDomain(name(62))).toBe(false);
  });
});

describe('domainValidator', () => {
  it('leaves a blank value to the required rule', () => {
    expect(domainValidator(new FormControl('  '))).toBeNull();
  });

  it('accepts upper case as the server does, because it lower-cases the name first', () => {
    expect(domainValidator(new FormControl('School1.COM'))).toBeNull();
  });

  it('reports the domain error for a bad host name', () => {
    expect(domainValidator(new FormControl('http://school1.com'))).toEqual({ domain: true });
  });
});
