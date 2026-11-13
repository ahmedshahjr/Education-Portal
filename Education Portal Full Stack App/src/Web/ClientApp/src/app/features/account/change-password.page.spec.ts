import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ChangePasswordPage, passwordsMatch } from './change-password.page';
import { FormBuilder } from '@angular/forms';

const blob = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

function setValue(element: HTMLElement, selector: string, value: string) {
  const input = element.querySelector<HTMLInputElement>(selector)!;
  input.value = value;
  input.dispatchEvent(new Event('input'));
}

describe('ChangePasswordPage', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [ChangePasswordPage],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
  });

  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('replaces the password, reloads the account and opens the app', async () => {
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    const fixture = TestBed.createComponent(ChangePasswordPage);
    await fixture.whenStable();
    const element: HTMLElement = fixture.nativeElement;
    const http = TestBed.inject(HttpTestingController);

    setValue(element, '#currentPassword', 'violet harbour lantern 47');
    setValue(element, '#newPassword', 'quiet orchard ladder 93');
    setValue(element, '#repeatPassword', 'quiet orchard ladder 93');
    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();

    const change = http.expectOne('/api/Users/me/password');
    expect(JSON.parse(change.request.body as string)).toEqual({
      currentPassword: 'violet harbour lantern 47',
      newPassword: 'quiet orchard ladder 93',
    });
    change.flush(null, { status: 204, statusText: 'No Content' });

    await vi.waitFor(() =>
      http.expectOne('/api/Users/me').flush(blob({ id: 'u1', email: 'head@new.local', roles: ['Administrator'], mustChangePassword: false }))
    );
    await vi.waitFor(() => http.expectOne('/api/Modules/enabled').flush(blob([])));
    await vi.waitFor(() => expect(navigate).toHaveBeenCalledWith('/'));
  });

  it('sends nothing when the new passwords differ', async () => {
    const fixture = TestBed.createComponent(ChangePasswordPage);
    await fixture.whenStable();
    const element: HTMLElement = fixture.nativeElement;

    setValue(element, '#currentPassword', 'violet harbour lantern 47');
    setValue(element, '#newPassword', 'quiet orchard ladder 93');
    setValue(element, '#repeatPassword', 'quiet orchard ladder 94');
    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
    await fixture.whenStable();

    expect(element.textContent).toContain('The new passwords do not match.');
  });

  it("shows the server's message when the password is refused", async () => {
    const fixture = TestBed.createComponent(ChangePasswordPage);
    await fixture.whenStable();
    const element: HTMLElement = fixture.nativeElement;

    setValue(element, '#currentPassword', 'not the right one at all');
    setValue(element, '#newPassword', 'quiet orchard ladder 93');
    setValue(element, '#repeatPassword', 'quiet orchard ladder 93');
    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
    TestBed.inject(HttpTestingController)
      .expectOne('/api/Users/me/password')
      .flush(blob({ status: 400, title: 'Bad Request', errors: { Password: ['Incorrect password.'] } }), { status: 400, statusText: 'Bad Request' });

    await vi.waitFor(async () => {
      await fixture.whenStable();
      expect(element.textContent).toContain('Incorrect password.');
    });
  });
});

describe('passwordsMatch', () => {
  it('accepts equal passwords and refuses different ones', () => {
    const fb = new FormBuilder().nonNullable;
    expect(passwordsMatch(fb.group({ newPassword: 'a', repeatPassword: 'a' }))).toBeNull();
    expect(passwordsMatch(fb.group({ newPassword: 'a', repeatPassword: 'b' }))).toEqual({ mismatch: true });
  });
});
