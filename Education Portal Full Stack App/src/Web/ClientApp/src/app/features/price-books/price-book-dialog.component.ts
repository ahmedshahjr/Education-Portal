import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { PriceBookDto } from '../../web-api-client';

/** What the dialog returns: the country and currency when a book is added; the currency, tax and switch when it is changed. */
export interface PriceBookChanges {
  readonly countryCode: string;
  readonly currencyCode: string;
  readonly taxRate: number;
  readonly isActive: boolean;
}

export interface PriceBookDialogData {
  /** The book being changed, or null when a book is added. */
  readonly book: PriceBookDto | null;
}

/** Adds a price book for a country, or changes its currency, tax rate and switch. The server checks everything again. */
@Component({
  selector: 'app-price-book-dialog',
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatSlideToggleModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './price-book-dialog.component.html',
  styleUrl: './price-book-dialog.component.css',
})
export class PriceBookDialogComponent {
  private readonly data = inject<PriceBookDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject<MatDialogRef<PriceBookDialogComponent, PriceBookChanges>>(MatDialogRef);

  protected readonly book = this.data.book;

  protected readonly form = inject(FormBuilder).nonNullable.group({
    countryCode: [{ value: this.book?.countryCode ?? '', disabled: this.book !== null }, [Validators.required, Validators.pattern(/^[A-Za-z]{2}$/)]],
    currencyCode: [this.book?.currencyCode ?? '', [Validators.required, Validators.pattern(/^[A-Z]{3}$/)]],
    taxRate: [this.book?.taxRate ?? 0, [Validators.required, Validators.min(0), Validators.max(100)]],
    isActive: [this.book?.isActive ?? true],
  });

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.dialogRef.close({
      countryCode: value.countryCode.trim().toUpperCase(),
      currencyCode: value.currencyCode.trim().toUpperCase(),
      taxRate: Number(value.taxRate),
      isActive: value.isActive,
    });
  }
}
