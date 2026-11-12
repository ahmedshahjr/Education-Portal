import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { AppIconComponent } from '../app-icon/app-icon.component';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, AppIconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="mm-modal">
      <div class="mm-modal__header">
        <div>
          <h2 class="mm-modal__title">{{ title() }}</h2>
          @if (subtitle()) {
            <p class="mm-modal__subtitle">{{ subtitle() }}</p>
          }
        </div>
        <button
          mat-icon-button
          type="button"
          aria-label="Close modal"
          (click)="closed.emit()"
        >
          <app-icon name="x" [size]="18" />
        </button>
      </div>

      <mat-dialog-content class="mm-modal__body">
        <ng-content />
      </mat-dialog-content>

      <div class="mm-modal__actions">
        <ng-content select="[modal-actions]" />
      </div>
    </div>
  `,
  styles: `
    .mm-modal {
      display: flex;
      flex-direction: column;
      min-width: 320px;
    }

    .mm-modal__header {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 16px;
      padding: 20px 24px 12px;
      border-bottom: 1px solid var(--mm-border-color);
    }

    .mm-modal__title {
      margin: 0;
      font-size: var(--mm-text-lg);
      font-weight: 600;
      color: var(--mm-text-primary);
    }

    .mm-modal__subtitle {
      margin: 4px 0 0;
      font-size: var(--mm-text-sm);
      color: var(--mm-text-muted);
    }

    .mm-modal__body {
      padding: 20px 24px !important;
    }

    .mm-modal__actions {
      display: flex;
      align-items: center;
      justify-content: flex-end;
      gap: 10px;
      padding: 12px 24px 20px;

      &:empty {
        display: none;
      }
    }
  `,
})
export class ModalComponent {
  readonly title = input.required<string>();
  readonly subtitle = input<string>('');
  readonly closed = output<void>();
}
