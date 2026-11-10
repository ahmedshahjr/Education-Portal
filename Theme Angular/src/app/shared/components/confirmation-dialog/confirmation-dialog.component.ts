import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ModalComponent } from '../modal/modal.component';

export interface ConfirmationDialogData {
  readonly title: string;
  readonly message: string;
  readonly confirmText?: string;
  readonly cancelText?: string;
  readonly destructive?: boolean;
}

@Component({
  selector: 'app-confirmation-dialog',
  standalone: true,
  imports: [MatButtonModule, ModalComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-modal [title]="data.title" (closed)="close(false)">
      <p class="mm-confirm-msg">{{ data.message }}</p>
      <div modal-actions>
        <button mat-stroked-button type="button" (click)="close(false)">
          {{ data.cancelText || 'Cancel' }}
        </button>
        <button
          mat-flat-button
          [color]="data.destructive ? 'warn' : 'primary'"
          type="button"
          (click)="close(true)"
        >
          {{ data.confirmText || 'Confirm' }}
        </button>
      </div>
    </app-modal>
  `,
  styles: `
    .mm-confirm-msg {
      margin: 0;
      font-size: var(--mm-text-sm);
      color: var(--mm-text-secondary);
      line-height: var(--mm-lh-base);
    }
  `,
})
export class ConfirmationDialogComponent {
  protected readonly data = inject<ConfirmationDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<ConfirmationDialogComponent, boolean>);

  protected close(confirmed: boolean): void {
    this.dialogRef.close(confirmed);
  }
}
