import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';

export interface SelectOption<T = string> {
  readonly value: T;
  readonly label: string;
}

@Component({
  selector: 'app-select-input',
  standalone: true,
  imports: [ReactiveFormsModule, MatFormFieldModule, MatSelectModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="mm-field">
      <label class="mm-field__label" [attr.for]="fieldId()">{{ label() }}</label>
      <mat-form-field appearance="outline" class="mm-field__control">
        <mat-select [id]="fieldId()" [formControl]="control()" [placeholder]="placeholder()">
          @for (opt of options(); track opt.value) {
            <mat-option [value]="opt.value">{{ opt.label }}</mat-option>
          }
        </mat-select>
        @if (control().hasError('required')) {
          <mat-error>{{ label() }} is required</mat-error>
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
export class SelectInputComponent<T = string> {
  readonly label = input.required<string>();
  readonly control = input.required<FormControl<T>>();
  readonly options = input.required<readonly SelectOption<T>[]>();
  readonly placeholder = input<string>('Select an option');
  readonly fieldId = input<string>(`mm-select-${Math.random().toString(36).slice(2, 8)}`);
}
