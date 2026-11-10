import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';
import { CountryDto, LanguageDto } from '../../web-api-client';
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
      new CountryDto({ id: 4, code: 'AE', name: 'UAE', languages: ['en', 'ar'], defaultLanguage: 'ar', timeZoneId: 'Asia/Dubai', currencyCode: 'AED' })
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
});
