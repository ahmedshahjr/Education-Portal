import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-form-input',
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="mm-field">
      <label class="mm-field__label" [attr.for]="fieldId()">{{ label() }}</label>
      <mat-form-field appearance="outline" class="mm-field__control">
        <input
          matInput
          [id]="fieldId()"
          [type]="type()"
          [placeholder]="placeholder()"
          [attr.autocomplete]="autocomplete() || null"
          [formControl]="control()"
        />
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
}
