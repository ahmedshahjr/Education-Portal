import { FormControl } from '@angular/forms';
import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';
import { CountryDto, DayOfWeek, LanguageDto } from '../../web-api-client';
import { CountryChanges, CountryFormComponent } from './country-form.component';

const languages = [
  new LanguageDto({ code: 'en', englishName: 'English', nativeName: 'English', isRightToLeft: false }),
  new LanguageDto({ code: 'ar', englishName: 'Arabic', nativeName: 'العربية', isRightToLeft: true }),
];

function create(country: CountryDto | null) {
  const fixture = TestBed.createComponent(CountryFormComponent);
  fixture.componentRef.setInput('languages', languages);
  fixture.componentRef.setInput('country', country);
  const saved: CountryChanges[] = [];
  fixture.componentInstance.save.subscribe((changes) => saved.push(changes));
  return { fixture, saved };
}

describe('CountryFormComponent', () => {
  it('sends a changed country with its fixed code', async () => {
    const { fixture, saved } = create(
      new CountryDto({ id: '0190f3a0-0000-7000-8000-000000000004', code: 'AE', name: 'UAE', languages: ['en', 'ar'], defaultLanguage: 'ar', timeZoneId: 'Asia/Dubai', currencyCode: 'AED', currencyDecimals: 2, phonePattern: undefined, weekStartDay: DayOfWeek.Monday, weekendDays: [DayOfWeek.Saturday, DayOfWeek.Sunday] })
    );
    await fixture.whenStable();
    const element: HTMLElement = fixture.nativeElement;

    expect(element.querySelector<HTMLInputElement>('input[formcontrolname="code"]')!.disabled).toBe(true);
    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();

    expect(saved).toEqual([
      { code: 'AE', name: 'UAE', languages: ['en', 'ar'], defaultLanguage: 'ar', timeZoneId: 'Asia/Dubai', currencyCode: 'AED' },
    ]);
  });

  it('sends nothing until the required fields are filled', async () => {
    const { fixture, saved } = create(null);
    await fixture.whenStable();

    fixture.nativeElement.querySelector('button[type="submit"]').click();
    await fixture.whenStable();

    expect(saved).toEqual([]);
    expect(fixture.nativeElement.querySelectorAll('mat-error').length).toBeGreaterThan(0);
  });

  it('gives every field room for its own hint so long hints never sit under the next row', async () => {
    const { fixture } = create(null);
    await fixture.whenStable();
    const element: HTMLElement = fixture.nativeElement;

    const fields = Array.from(element.querySelectorAll('.mm-country-form__grid > mat-form-field'));
    expect(fields.length).toBe(6);
    for (const field of fields) {
      expect(field.classList).toContain('mm-col-4');
      expect(field.querySelector('.mat-mdc-form-field-subscript-dynamic-size')).not.toBeNull();
    }
  });

  it('takes the default language only from the chosen languages, ignoring case, and clears one the country does not offer, so it is never sent', async () => {
    const { fixture, saved } = create(null);
    await fixture.whenStable();
    const component = fixture.componentInstance as unknown as { form: { controls: { languages: FormControl<string[]>; defaultLanguage: FormControl<string> } } };
    const element: HTMLElement = fixture.nativeElement;

    component.form.controls.languages.setValue(['en', 'ar']);
    component.form.controls.defaultLanguage.setValue('AR');
    expect(component.form.controls.defaultLanguage.errors).toBeNull();

    // 'ur' is not among the chosen languages: the form clears it, so it can't be sent.
    component.form.controls.defaultLanguage.setValue('ur');
    fixture.detectChanges();
    expect(component.form.controls.defaultLanguage.valid).toBe(false);
    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
    expect(saved).toEqual([]);
  });
});
