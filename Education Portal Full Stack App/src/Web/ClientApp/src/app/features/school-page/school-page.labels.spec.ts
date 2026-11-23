import { describe, expect, it } from 'vitest';
// Gender policies and admission statuses have no screen yet. The other label sets are checked on the screens that show them.
import { ADMISSION_STATUSES, admissionStatusLabel, GENDER_POLICIES, genderPolicyLabel } from './school-page.labels';

/** A value added to a closed set on the server must get a label here, or the screens would show nothing for it. */
describe('school page labels', () => {
  const sets: [string, readonly string[], (value: never) => string][] = [
    ['gender policies', GENDER_POLICIES, genderPolicyLabel as (value: never) => string],
    ['admission statuses', ADMISSION_STATUSES, admissionStatusLabel as (value: never) => string],
  ];

  for (const [name, values, label] of sets) {
    it(`labels every one of the ${name}`, () => {
      expect(values.length).toBeGreaterThan(0);
      const labels = values.map((value) => label(value as never));
      expect(labels.every((text) => typeof text === 'string' && text.length > 0)).toBe(true);
      expect(new Set(labels).size).toBe(values.length);
    });
  }
});
