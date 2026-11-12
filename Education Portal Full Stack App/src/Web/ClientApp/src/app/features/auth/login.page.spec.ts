import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { BrandingService } from '../../core/branding/branding.service';
import { PAGE_RELOAD } from '../../core/i18n/locale.service';
import { BrandingDto } from '../../web-api-client';
import { localReturnUrl, LoginPage } from './login.page';

describe('LoginPage', () => {
  const reload = vi.fn();

  beforeEach(() => {
    reload.mockReset();
    TestBed.configureTestingModule({
      imports: [LoginPage],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting(), { provide: PAGE_RELOAD, useValue: reload }],
    });
    TestBed.inject(BrandingService).apply(
      new BrandingDto({ schoolName: 'Demo School', isMaster: false, accentColor: '#00a1ff', sidebarColor: undefined, logoVersion: undefined, faviconVersion: undefined })
    );
  });

  it("welcomes the visitor with the school's name before sign-in", async () => {
    const fixture = TestBed.createComponent(LoginPage);
    await fixture.whenStable();

    expect(fixture.nativeElement.querySelector('h1').textContent).toContain('Welcome to Demo School');
  });

  it('shows the same error for any refused sign-in', async () => {
    const fixture = TestBed.createComponent(LoginPage);
    await fixture.whenStable();
    const element: HTMLElement = fixture.nativeElement;

    setValue(element, '#email', 'someone@demo.localhost');
    setValue(element, '#password', 'not the password');
    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
    TestBed.inject(HttpTestingController)
      .expectOne('/api/Users/login')
      .flush(new Blob([JSON.stringify({ detail: 'Invalid email or password.' })], { type: 'application/problem+json' }), { status: 401, statusText: 'Unauthorized' });

    // The error body arrives as a Blob and is read asynchronously.
    await vi.waitFor(async () => {
      await fixture.whenStable();
      expect(element.querySelector('#login-error')?.textContent).toContain('Invalid email or password.');
    });
  });
});

describe('LoginPage language after sign-in', () => {
  const reload = vi.fn();

  beforeEach(() => {
    reload.mockReset();
    TestBed.configureTestingModule({
      imports: [LoginPage],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting(), { provide: PAGE_RELOAD, useValue: reload }],
    });
  });

  for (const [saved, reloads] of [['ur', true], ['en', false]] as const) {
    it(`${reloads ? 'reloads' : 'does not reload'} when the account's saved language is ${saved}`, async () => {
      const fixture = TestBed.createComponent(LoginPage);
      await fixture.whenStable();
      const element: HTMLElement = fixture.nativeElement;
      const backend = TestBed.inject(HttpTestingController);

      setValue(element, '#email', 'teacher@demo.localhost');
      setValue(element, '#password', 'a long enough password');
      element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
      backend.expectOne('/api/Users/login').flush(new Blob([JSON.stringify(true)], { type: 'application/json' }));

      await vi.waitFor(() => {
        for (const request of backend.match('/api/Users/me')) {
          request.flush(new Blob([JSON.stringify({ userName: 'teacher@demo.localhost', roles: [] })], { type: 'application/json' }));
        }
        for (const request of backend.match('/api/Modules/enabled')) {
          request.flush(new Blob([JSON.stringify([])], { type: 'application/json' }));
        }
        backend
          .expectOne('/api/Localization')
          .flush(new Blob([JSON.stringify({ language: saved, culture: saved, languages: [] })], { type: 'application/json' }));
      });

      if (reloads) {
        await vi.waitFor(() => expect(reload).toHaveBeenCalledOnce());
      } else {
        await new Promise((resolve) => setTimeout(resolve, 20));
        expect(reload).not.toHaveBeenCalled();
      }
    });
  }
});

describe('localReturnUrl', () => {
  it('keeps a path inside the app', () => {
    expect(localReturnUrl('/settings/branding')).toBe('/settings/branding');
  });

  it('refuses other sites', () => {
    expect(localReturnUrl('https://evil.example')).toBe('/');
    expect(localReturnUrl('//evil.example')).toBe('/');
    expect(localReturnUrl(null)).toBe('/');
  });
});

function setValue(root: HTMLElement, selector: string, value: string): void {
  const input = root.querySelector<HTMLInputElement>(selector)!;
  input.value = value;
  input.dispatchEvent(new Event('input'));
}
