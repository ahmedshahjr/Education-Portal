import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { LocaleService } from '../../../core/i18n/locale.service';
import { problemMessages } from '../../../core/http/problem-details';
import { AppIconComponent } from '../app-icon/app-icon.component';

/**
 * The language dropdown: the languages the school's country offers, each in its own script. Hidden when the school has
 * only one. Choosing one saves it for this domain and reloads the page in that language and direction.
 */
@Component({
  selector: 'app-language-menu',
  imports: [MatButtonModule, MatMenuModule, MatTooltipModule, AppIconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (locale.languages.length > 1) {
      <button
        mat-button
        type="button"
        class="mm-language-btn"
        [matMenuTriggerFor]="languageMenu"
        [disabled]="locale.changing()"
        aria-label="Change language"
        i18n-aria-label="@@language.change"
        matTooltip="Language"
        i18n-matTooltip="@@language.tooltip"
      >
        <app-icon name="globe" [size]="18" />
        <span class="mm-language-btn__name" [attr.lang]="locale.language">{{ current() }}</span>
      </button>

      <mat-menu #languageMenu="matMenu" xPosition="before">
        @for (language of locale.languages; track language.code) {
          <button
            mat-menu-item
            type="button"
            [attr.lang]="language.code"
            [attr.dir]="language.isRightToLeft ? 'rtl' : 'ltr'"
            [attr.aria-current]="language.code === locale.language ? 'true' : null"
            (click)="choose(language.code)"
          >
            @if (language.code === locale.language) {
              <app-icon name="check" [size]="16" />
            }
            <span>{{ language.nativeName }}</span>
          </button>
        }
      </mat-menu>

      @if (error(); as message) {
        <span class="mm-visually-hidden" role="alert">{{ message }}</span>
      }
    }
  `,
  styles: `
    .mm-language-btn {
      color: var(--mm-text-secondary);
    }

    .mm-language-btn__name {
      margin-inline-start: 6px;
      font-size: var(--mm-text-sm);
    }
  `,
})
export class LanguageMenuComponent {
  protected readonly locale = inject(LocaleService);

  protected readonly current = computed(
    () => this.locale.languages.find((l) => l.code === this.locale.language)?.nativeName ?? this.locale.language
  );

  protected readonly error = signal<string | null>(null);

  protected choose(code: string): void {
    if (code === this.locale.language) {
      return;
    }

    this.error.set(null);
    this.locale.changeLanguage(code).subscribe({
      error: (error: unknown) => this.error.set(problemMessages(error)[0]),
    });
  }
}
