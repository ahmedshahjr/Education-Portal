import { describe, expect, it } from 'vitest';
import {
  ADMISSION_STATUSES,
  admissionStatusLabel,
  CLASS_LEVELS,
  classLevelLabel,
  CURRICULUM_KINDS,
  curriculumLabel,
  FACILITY_KINDS,
  facilityLabel,
  GENDER_POLICIES,
  genderPolicyLabel,
  PHOTO_KINDS,
  photoKindLabel,
} from './school-page.labels';

/** A value added to a closed set on the server must get a label here, or the screens would show nothing for it. */
describe('school page labels', () => {
  const sets: [string, readonly string[], (value: never) => string][] = [
    ['curricula', CURRICULUM_KINDS, curriculumLabel as (value: never) => string],
    ['facilities', FACILITY_KINDS, facilityLabel as (value: never) => string],
    ['photo kinds', PHOTO_KINDS, photoKindLabel as (value: never) => string],
    ['gender policies', GENDER_POLICIES, genderPolicyLabel as (value: never) => string],
    ['admission statuses', ADMISSION_STATUSES, admissionStatusLabel as (value: never) => string],
    ['class levels', CLASS_LEVELS, classLevelLabel as (value: never) => string],
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
