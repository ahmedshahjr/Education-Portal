import { FormControl, FormGroup } from '@angular/forms';
import { describe, expect, it } from 'vitest';
import {
  defaultLanguageValidator,
  isValidPhonePattern,
  MAX_WEEKEND_DAYS,
  PHONE_PATTERN_MAX_LENGTH,
  phonePatternValidator,
  weekendDaysValidator,
} from './country-validators';

describe('isValidPhonePattern (Country.IsValidPhonePattern)', () => {
  it('accepts a regular expression that compiles', () => {
    expect(isValidPhonePattern('^\\+92\\d+$')).toBe(true);
  });

  it('refuses a pattern that does not compile and a blank one', () => {
    expect(isValidPhonePattern('(')).toBe(false);
    expect(isValidPhonePattern('   ')).toBe(false);
  });

  it('accepts 200 characters and refuses 201', () => {
    expect(isValidPhonePattern('a'.repeat(PHONE_PATTERN_MAX_LENGTH))).toBe(true);
    expect(isValidPhonePattern('a'.repeat(PHONE_PATTERN_MAX_LENGTH + 1))).toBe(false);
  });
});

describe('phonePatternValidator', () => {
  it('allows a blank pattern, because a country may have none', () => {
    expect(phonePatternValidator(new FormControl(''))).toBeNull();
  });

  it('reports phonePattern for a pattern that does not compile', () => {
    expect(phonePatternValidator(new FormControl('['))).toEqual({ phonePattern: true });
  });
});

describe('weekendDaysValidator', () => {
  it('accepts up to three different days, and none', () => {
    expect(weekendDaysValidator(new FormControl([]))).toBeNull();
    expect(weekendDaysValidator(new FormControl(['Sat', 'Sun']))).toBeNull();
    expect(weekendDaysValidator(new FormControl(['Fri', 'Sat', 'Sun']))).toBeNull();
  });

  it('refuses a fourth day, the limit being three', () => {
    expect(MAX_WEEKEND_DAYS).toBe(3);
    expect(weekendDaysValidator(new FormControl(['Fri', 'Sat', 'Sun', 'Mon']))).toEqual({ maxWeekendDays: true });
  });

  it('refuses the same day listed twice', () => {
    expect(weekendDaysValidator(new FormControl(['Sat', 'Sat']))).toEqual({ duplicateDay: true });
  });
});

describe('defaultLanguageValidator', () => {
  // The default is set after the group is built, so the validator sees the languages through the parent.
  const formWith = (languages: string[], defaultLanguage: string) => {
    const form = new FormGroup({
      languages: new FormControl(languages),
      defaultLanguage: new FormControl('', { validators: [defaultLanguageValidator] }),
    });
    form.controls.defaultLanguage.setValue(defaultLanguage);
    return form;
  };

  it('accepts a default that is one of the country languages, ignoring case', () => {
    expect(formWith(['en', 'ur'], 'ur').controls.defaultLanguage.errors).toBeNull();
    expect(formWith(['en', 'ur'], 'UR').controls.defaultLanguage.errors).toBeNull();
  });

  it('refuses a default the country does not offer', () => {
    expect(formWith(['en', 'ur'], 'ar').controls.defaultLanguage.errors).toEqual({ notOffered: true });
  });

  it('leaves an empty default to the required rule', () => {
    expect(formWith(['en'], '').controls.defaultLanguage.errors).toBeNull();
  });
});
