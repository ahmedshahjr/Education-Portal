import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';
import { CountryDto } from '../../web-api-client';
import { NewSchool, SchoolFormComponent } from './school-form.component';

const countries = [
  new CountryDto({ id: '0190f3a0-0000-7000-8000-000000000001', code: 'PK', name: 'Pakistan', languages: ['en', 'ur'], defaultLanguage: 'en', timeZoneId: 'Asia/Karachi', currencyCode: 'PKR' }),
];

function create() {
  const fixture = TestBed.createComponent(SchoolFormComponent);
  fixture.componentRef.setInput('countries', countries);
  const saved: NewSchool[] = [];
  fixture.componentInstance.save.subscribe((school) => saved.push(school));
  return { fixture, saved, element: fixture.nativeElement as HTMLElement };
}

function fill(element: HTMLElement, name: string, value: string) {
  const input = element.querySelector<HTMLInputElement>(`input[formcontrolname="${name}"]`)!;
  input.value = value;
  input.dispatchEvent(new Event('input'));
}

describe('SchoolFormComponent', () => {
  it('sends a new school with its first administrator, domain in lower case', async () => {
    const { fixture, saved, element } = create();
    await fixture.whenStable();

    fill(element, 'name', '  New School ');
    fill(element, 'domain', 'New-School.Localhost');
    fill(element, 'administratorEmail', ' head@new-school.local ');
    fill(element, 'administratorPassword', 'violet harbour lantern 47');
    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();

    expect(saved).toEqual([
      {
        name: 'New School',
        domain: 'new-school.localhost',
        countryId: null,
        dataRegion: null,
        administratorEmail: 'head@new-school.local',
        administratorPassword: 'violet harbour lantern 47',
      },
    ]);
  });

  it('sends nothing until the required fields are filled', async () => {
    const { fixture, saved, element } = create();
    await fixture.whenStable();

    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();

    expect(saved).toEqual([]);
  });

  it.each(['https://school.com', 'school.com:8080', 'school.com/path', '-school.com', 'school..com'])(
    'refuses %s as a domain',
    async (domain) => {
      const { fixture, saved, element } = create();
      await fixture.whenStable();

      fill(element, 'name', 'New School');
      fill(element, 'domain', domain);
      fill(element, 'administratorEmail', 'head@new-school.local');
      fill(element, 'administratorPassword', 'violet harbour lantern 47');
      element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();

      expect(saved).toEqual([]);
    }
  );

  it('refuses a one-time password shorter than 15 characters', async () => {
    const { fixture, saved, element } = create();
    await fixture.whenStable();

    fill(element, 'name', 'New School');
    fill(element, 'domain', 'new.localhost');
    fill(element, 'administratorEmail', 'head@new-school.local');
    fill(element, 'administratorPassword', 'too-short');
    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();

    expect(saved).toEqual([]);
  });

  it('generates a random one-time password long enough for the policy', async () => {
    const { fixture, element } = create();
    await fixture.whenStable();

    const generate = [...element.querySelectorAll<HTMLButtonElement>('button')].find((b) => b.textContent?.includes('Generate'))!;
    generate.click();
    const first = element.querySelector<HTMLInputElement>('input[formcontrolname="administratorPassword"]')!.value;
    generate.click();
    const second = element.querySelector<HTMLInputElement>('input[formcontrolname="administratorPassword"]')!.value;

    expect(first.length).toBeGreaterThanOrEqual(15);
    expect(second).not.toBe(first);
  });
});
