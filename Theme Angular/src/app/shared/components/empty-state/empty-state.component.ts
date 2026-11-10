import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { AppIconName } from '../../models/navigation.models';
import { AppIconComponent } from '../app-icon/app-icon.component';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [MatButtonModule, AppIconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="mm-empty" role="status">
      <div class="mm-icon-pill mm-icon-pill--round mm-icon-pill--soft-primary">
        <app-icon [name]="icon()" [size]="24" />
      </div>
      <h3 class="mm-empty__title">{{ title() }}</h3>
      <p class="mm-empty__desc">{{ description() }}</p>
      @if (actionLabel()) {
        <button mat-flat-button color="primary" type="button" (click)="action.emit()">
          {{ actionLabel() }}
        </button>
      }
    </div>
  `,
  styles: `
    .mm-empty {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      text-align: center;
      padding: 40px 24px;
      gap: 10px;
    }

    .mm-empty__title {
      margin: 8px 0 0;
      font-size: var(--mm-text-base);
      font-weight: 600;
      color: var(--mm-text-primary);
    }

    .mm-empty__desc {
      margin: 0 0 8px;
      max-width: 360px;
      font-size: var(--mm-text-sm);
      color: var(--mm-text-muted);
    }
  `,
})
export class EmptyStateComponent {
  readonly icon = input<AppIconName>('inbox');
  readonly title = input<string>('No records found');
  readonly description = input<string>('Try adjusting your filters or search criteria.');
  readonly actionLabel = input<string>('');
  readonly action = output<void>();
}
