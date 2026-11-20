import { SchoolGenderPolicy } from '../../web-api-client';

/** A campus's gender policy with its translated label, in the order the form lists them. */
export interface GenderPolicyOption {
  readonly value: SchoolGenderPolicy;
  readonly label: string;
}

/** The gender policies a campus can have: co-education first, because it is the default. */
export function genderPolicyOptions(): readonly GenderPolicyOption[] {
  return [
    { value: SchoolGenderPolicy.CoEducation, label: $localize`:@@campuses.genderCoEducation:Co-education` },
    { value: SchoolGenderPolicy.BoysOnly, label: $localize`:@@campuses.genderBoys:Boys only` },
    { value: SchoolGenderPolicy.GirlsOnly, label: $localize`:@@campuses.genderGirls:Girls only` },
  ];
}

/** The label of a gender policy; an unknown value (a newer server) shows as it came. */
export function genderPolicyLabel(policy: SchoolGenderPolicy | undefined): string {
  return genderPolicyOptions().find((option) => option.value === policy)?.label ?? policy ?? '';
}
