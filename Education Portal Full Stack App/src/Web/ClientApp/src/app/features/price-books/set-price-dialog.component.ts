import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

export interface SetPriceDialogData {
  /** The heading, for example "Standard, monthly". */
  readonly title: string;
  /** The currency the price is in (the price book's). */
  readonly currencyCode: string;
  /** The price now, or null when none is set. */
  readonly current: number | null;
}

/** Sets one price in a price book: one plan or one paid module, for one billing cycle. The price is in the book's currency. */
@Component({
  selector: 'app-set-price-dialog',
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './set-price-dialog.component.html',
  styleUrl: './set-price-dialog.component.css',
})
export class SetPriceDialogComponent {
  protected readonly data = inject<SetPriceDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject<MatDialogRef<SetPriceDialogComponent, number>>(MatDialogRef);

  /** Up to two decimal places and no more than a very large amount; the server checks it again (SEC-INPUT-01). */
  protected readonly form = inject(FormBuilder).nonNullable.group({
    price: [this.data.current ?? 0, [Validators.required, Validators.min(0), Validators.max(1_000_000_000_000)]],
  });

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.dialogRef.close(Math.round(Number(this.form.controls.price.value) * 100) / 100);
  }
}
