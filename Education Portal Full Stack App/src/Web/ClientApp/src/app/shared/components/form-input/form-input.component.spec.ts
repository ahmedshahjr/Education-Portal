import { Component, signal } from '@angular/core';
import { FormControl } from '@angular/forms';
import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';
import { FormInputComponent } from './form-input.component';

@Component({
  imports: [FormInputComponent],
  template: `<app-form-input label="Password" type="password" fieldId="pw" [revealable]="revealable()" [control]="control" />`,
})
class HostComponent {
  readonly control = new FormControl('secret', { nonNullable: true });
  readonly revealable = signal(true);
}

describe('FormInputComponent password reveal', () => {
  it('shows a password as dots and offers an eye button to show it', async () => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.detectChanges();
    const element: HTMLElement = fixture.nativeElement;

    expect(element.querySelector<HTMLInputElement>('input')!.type).toBe('password');
    expect(element.querySelector('button[aria-label="Show password"]')).not.toBeNull();
  });

  it('shows the password as text after the eye button is pressed, and hides it again', async () => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.detectChanges();
    const element: HTMLElement = fixture.nativeElement;

    element.querySelector<HTMLButtonElement>('button[aria-label="Show password"]')!.click();
    fixture.detectChanges();

    expect(element.querySelector<HTMLInputElement>('input')!.type).toBe('text');
    expect(element.querySelector('button[aria-label="Hide password"]')).not.toBeNull();

    element.querySelector<HTMLButtonElement>('button[aria-label="Hide password"]')!.click();
    fixture.detectChanges();

    expect(element.querySelector<HTMLInputElement>('input')!.type).toBe('password');
  });

  it('puts the eye button inside the input box, in the field suffix slot', async () => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.detectChanges();
    const element: HTMLElement = fixture.nativeElement;

    const button = element.querySelector('button[aria-label="Show password"]')!;
    expect(button.closest('.mat-mdc-form-field-icon-suffix')).not.toBeNull();
    expect(button.closest('.mat-mdc-form-field-infix')).toBeNull();
  });

  it('shows no eye button when the field is not revealable', async () => {
    const fixture = TestBed.createComponent(HostComponent);
    fixture.componentInstance.revealable.set(false);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('button[aria-label="Show password"]')).toBeNull();
  });
});
