import { ChangeDetectionStrategy, Component, computed, effect, inject, input, output } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { CountryDto, LanguageDto } from '../../web-api-client';
import { supportedValues } from './country-format';

/** What the form asks to save. The code is only sent when a country is added. */
export interface CountryChanges {
  readonly code: string;
  readonly name: string;
  readonly languages: string[];
  readonly defaultLanguage: string;
  readonly timeZoneId: string;
  readonly currencyCode: string;
}

/**
 * Adds or changes a country: the languages its schools offer (from those the portal has translations for), the
 * default one, the time zone and the currency. The checks mirror the server's (SEC-INPUT-01), which has the final say.
 */
@Component({
  selector: 'app-country-form',
  imports: [ReactiveFormsModule, MatCardModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatSelectModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <mat-card class="mm-card mm-elevated">
      <mat-card-content>
        <form [formGroup]="form" (ngSubmit)="submit()" class="mm-country-form" novalidate>
          <h2 class="mm-country-form__title">
            @if (country(); as existing) {
              <ng-container i18n="@@countries.editTitle">Change {{ existing.name }}</ng-container>
            } @else {
              <ng-container i18n="@@countries.addTitle">Add a country</ng-container>
            }
          </h2>

          <div class="mm-country-form__grid">
            <mat-form-field appearance="outline">
              <mat-label i18n="@@countries.code">Country code</mat-label>
              <input matInput formControlName="code" maxlength="2" autocomplete="off" dir="ltr" />
              <mat-hint i18n="@@countries.codeHint">Two letters (ISO 3166-1), for example PK. Cannot be changed later.</mat-hint>
              @if (form.controls.code.invalid) {
                <mat-error i18n="@@countries.codeError">Enter two letters, for example PK.</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label i18n="@@countries.name">Name</mat-label>
              <input matInput formControlName="name" maxlength="100" autocomplete="off" />
              @if (form.controls.name.invalid) {
                <mat-error i18n="@@countries.nameError">Enter the country's name.</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label i18n="@@countries.languages">Languages</mat-label>
              <mat-select formControlName="languages" multiple>
                @for (language of languages(); track language.code) {
                  <mat-option [value]="language.code">
                    {{ language.englishName }} (<span [attr.lang]="language.code">{{ language.nativeName }}</span>)
                  </mat-option>
                }
              </mat-select>
              <mat-hint i18n="@@countries.languagesHint">The languages schools in this country can switch between.</mat-hint>
              @if (form.controls.languages.invalid) {
                <mat-error i18n="@@countries.languagesError">Choose at least one language.</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label i18n="@@countries.defaultLanguage">Default language</mat-label>
              <mat-select formControlName="defaultLanguage">
                @for (language of chosenLanguages(); track language.code) {
                  <mat-option [value]="language.code">{{ language.englishName }}</mat-option>
                }
              </mat-select>
              <mat-hint i18n="@@countries.defaultHint">Used until a visitor picks another one.</mat-hint>
              @if (form.controls.defaultLanguage.invalid) {
                <mat-error i18n="@@countries.defaultError">Choose one of the country's languages.</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label i18n="@@countries.timeZone">Time zone</mat-label>
              <mat-select formControlName="timeZoneId">
                @for (zone of timeZones; track zone) {
                  <mat-option [value]="zone">{{ zone }}</mat-option>
                }
              </mat-select>
              @if (form.controls.timeZoneId.invalid) {
                <mat-error i18n="@@countries.timeZoneError">Choose a time zone.</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label i18n="@@countries.currency">Currency</mat-label>
              <mat-select formControlName="currencyCode">
                @for (currency of currencies; track currency) {
                  <mat-option [value]="currency">{{ currency }}</mat-option>
                }
              </mat-select>
              @if (form.controls.currencyCode.invalid) {
                <mat-error i18n="@@countries.currencyError">Choose a currency.</mat-error>
              }
            </mat-form-field>
          </div>

          @for (message of errors(); track message) {
            <p class="mm-country-form__error" role="alert">{{ message }}</p>
          }

          <div class="mm-country-form__actions">
            <button mat-button type="button" (click)="cancel.emit()" i18n="@@common.cancel">Cancel</button>
            <button mat-flat-button type="submit" [disabled]="saving()" i18n="@@countries.save">Save country</button>
          </div>
        </form>
      </mat-card-content>
    </mat-card>
  `,
  styles: `
    .mm-country-form__title {
      margin: 0 0 var(--mm-space-4);
      font-size: var(--mm-text-lg);
      font-weight: 600;
    }

    .mm-country-form__grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
      gap: var(--mm-space-2) var(--mm-space-4);
    }

    .mm-country-form__error {
      margin: 0 0 var(--mm-space-2);
      color: var(--mat-sys-error);
      font-size: var(--mm-text-sm);
    }

    .mm-country-form__actions {
      display: flex;
      justify-content: flex-end;
      gap: var(--mm-space-2);
      margin-top: var(--mm-space-4);
    }
  `,
})
export class CountryFormComponent {
  /** The country to change, or null to add one. */
  readonly country = input<CountryDto | null>(null);
  readonly languages = input.required<readonly LanguageDto[]>();
  readonly saving = input(false);
  readonly errors = input<readonly string[]>([]);

  readonly save = output<CountryChanges>();
  readonly cancel = output<void>();

  protected readonly timeZones = supportedValues('timeZone');
  protected readonly currencies = supportedValues('currency');

  protected readonly form = inject(FormBuilder).nonNullable.group({
    code: ['', [Validators.required, Validators.pattern(/^[A-Za-z]{2}$/)]],
    name: ['', [Validators.required, Validators.maxLength(100)]],
    languages: [[] as string[], [Validators.required]],
    defaultLanguage: ['', [Validators.required]],
    timeZoneId: ['', [Validators.required]],
    currencyCode: ['', [Validators.required, Validators.pattern(/^[A-Z]{3}$/)]],
  });

  private readonly selectedCodes = toSignal(this.form.controls.languages.valueChanges, { initialValue: [] as string[] });

  /** The default language is picked from the languages chosen above. */
  protected readonly chosenLanguages = computed(() =>
    this.languages().filter((language) => this.selectedCodes().includes(language.code))
  );

  constructor() {
    // Fill the form from the country being changed; the code of an existing country is fixed.
    effect(() => {
      const country = this.country();
      this.form.reset({
        code: country?.code ?? '',
        name: country?.name ?? '',
        languages: [...(country?.languages ?? [])],
        defaultLanguage: country?.defaultLanguage ?? '',
        timeZoneId: country?.timeZoneId ?? '',
        currencyCode: country?.currencyCode ?? '',
      });
      if (country) {
        this.form.controls.code.disable();
      } else {
        this.form.controls.code.enable();
      }
    });

    // A default that is no longer among the languages is cleared, so the form can't send it.
    effect(() => {
      const codes = this.selectedCodes();
      const current = this.form.controls.defaultLanguage.value;
      if (current && !codes.includes(current)) {
        this.form.controls.defaultLanguage.setValue(codes.length === 1 ? codes[0] : '');
      }
    });
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.save.emit({ ...value, code: value.code.toUpperCase() });
  }
}
