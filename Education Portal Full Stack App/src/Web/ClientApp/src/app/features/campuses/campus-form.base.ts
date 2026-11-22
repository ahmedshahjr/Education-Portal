import { FormBuilder, FormControl, Validators } from '@angular/forms';
import {
  phoneValidator,
  phonesValidator,
  POSTAL_CODE_MAX_LENGTH,
  postalCodeValidator,
} from '../../shared/validation/contact-validators';
import { websiteValidator, WEBSITE_MAX_LENGTH } from '../../shared/validation/website-validators';
import { CampusDetailsInput, SchoolGenderPolicy } from '../../web-api-client';

export { MAX_PHONES } from '../../shared/validation/contact-validators';

/** The campus form, shared by the add page and the list page's edit form. */
export function createCampusForm(formBuilder: FormBuilder) {
  return formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    localName: ['', [Validators.maxLength(200)]],
    genderPolicy: [SchoolGenderPolicy.CoEducation],
    address: ['', [Validators.required, Validators.maxLength(300)]],
    stateId: ['', [Validators.required]],
    cityId: ['', [Validators.required]],
    postalCode: ['', [Validators.required, Validators.maxLength(POSTAL_CODE_MAX_LENGTH), postalCodeValidator]],
    phones: formBuilder.array<FormControl<string>>(
      [formBuilder.nonNullable.control('', [phoneValidator])],
      { validators: [phonesValidator] }
    ),
    website: ['', [Validators.maxLength(WEBSITE_MAX_LENGTH), websiteValidator]],
    schoolNumber: ['', [Validators.required, Validators.maxLength(50)]],
    shortName: ['', [Validators.required, Validators.maxLength(25)]],
    principalId: [''],
    latitude: [null as number | null, [Validators.required, Validators.min(-90), Validators.max(90)]],
    longitude: [null as number | null, [Validators.required, Validators.min(-180), Validators.max(180)]],
  });
}

export type CampusForm = ReturnType<typeof createCampusForm>;

/** The profile the add and update requests send, read from the form. */
export function campusProfileOf(form: CampusForm) {
  const value = form.getRawValue();
  const details = new CampusDetailsInput({
    address: value.address.trim(),
    stateId: value.stateId,
    cityId: value.cityId,
    postalCode: value.postalCode.trim(),
    phones: form.controls.phones.controls.map((c) => c.value.trim()).filter((v) => v.length > 0),
    website: value.website.trim() || undefined,
    schoolNumber: value.schoolNumber.trim(),
    shortName: value.shortName.trim(),
    principalId: value.principalId || undefined,
    latitude: value.latitude ?? undefined,
    longitude: value.longitude ?? undefined,
  });
  return {
    name: value.name.trim(),
    localName: value.localName.trim() || undefined,
    genderPolicy: value.genderPolicy,
    details,
  };
}
