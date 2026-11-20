import { ChangeDetectionStrategy, Component, computed, input, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconButton } from '@angular/material/button';
import { MatSuffix } from '@angular/material/form-field';
import { AppIconComponent } from '../app-icon/app-icon.component';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-form-input',
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatIconButton, MatSuffix, AppIconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="mm-field">
      <label class="mm-field__label" [attr.for]="fieldId()">{{ label() }}</label>
      <mat-form-field appearance="outline" class="mm-field__control">
        <input
          matInput
          [id]="fieldId()"
          [type]="effectiveType()"
          [placeholder]="placeholder()"
          [attr.autocomplete]="autocomplete() || null"
          [formControl]="control()"
        />
        @if (revealable() && type() === 'password') {
          @if (revealed()) {
            <button mat-icon-button matSuffix type="button" class="mm-field__reveal" (click)="toggleReveal()" i18n-aria-label="@@field.hidePassword" aria-label="Hide password">
              <app-icon name="eye-off" [size]="20" />
            </button>
          } @else {
            <button mat-icon-button matSuffix type="button" class="mm-field__reveal" (click)="toggleReveal()" i18n-aria-label="@@field.showPassword" aria-label="Show password">
              <app-icon name="eye" [size]="20" />
            </button>
          }
        }
        @if (hint()) {
          <mat-hint>{{ hint() }}</mat-hint>
        }
        @if (control().hasError('required')) {
          <mat-error i18n="@@field.required">{{ label() }} is required</mat-error>
        } @else if (control().hasError('email')) {
          <mat-error i18n="@@field.email">Enter a valid email address</mat-error>
        } @else if (control().hasError('min')) {
          <mat-error i18n="@@field.min">Value must be greater than or equal to 0</mat-error>
        } @else if (control().hasError('minlength')) {
          <mat-error i18n="@@field.minLength">{{ label() }} is too short</mat-error>
        }
      </mat-form-field>
    </div>
  `,
  styles: `
    .mm-field {
      display: flex;
      flex-direction: column;
      gap: 6px;
      width: 100%;
    }

    .mm-field__label {
      font-size: var(--mm-text-xs);
      font-weight: 600;
      color: var(--mm-text-primary);
    }

    .mm-field__control {
      width: 100%;
    }

    /* The reveal button sits inside the outlined field, on the same row as the input. */
    .mm-field__reveal {
      display: inline-flex;
      flex-shrink: 0;
      align-self: center;
    }
  `,
})
export class FormInputComponent {
  readonly label = input.required<string>();
  readonly control = input.required<FormControl<string>>();
  readonly type = input<'text' | 'email' | 'number' | 'password' | 'tel'>('text');
  readonly placeholder = input<string>('');
  readonly hint = input<string>('');
  readonly autocomplete = input<string>('');
  readonly fieldId = input<string>(`mm-input-${Math.random().toString(36).slice(2, 8)}`);

  /** Adds a button that shows or hides a password while it is typed. Only for password fields. */
  readonly revealable = input(false);

  protected readonly revealed = signal(false);

  /** A password shows as text only while the user has chosen to reveal it. */
  protected readonly effectiveType = computed(() =>
    this.type() === 'password' && this.revealable() && this.revealed() ? 'text' : this.type(),
  );

  protected toggleReveal(): void {
    this.revealed.update((shown) => !shown);
  }
}
