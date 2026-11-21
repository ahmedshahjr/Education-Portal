import { ChangeDetectionStrategy, Component, computed, input, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconButton } from '@angular/material/button';
import { AppIconComponent } from '../app-icon/app-icon.component';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-form-input',
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatIconButton, AppIconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './form-input.component.html',
  styleUrl: './form-input.component.scss',
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

  /** One button changes its label, so the label is chosen here rather than by two `i18n-aria-label` buttons. */
  protected readonly revealLabel = computed(() =>
    this.revealed() ? $localize`:@@field.hidePassword:Hide password` : $localize`:@@field.showPassword:Show password`,
  );

  protected toggleReveal(): void {
    this.revealed.update((shown) => !shown);
  }
}
