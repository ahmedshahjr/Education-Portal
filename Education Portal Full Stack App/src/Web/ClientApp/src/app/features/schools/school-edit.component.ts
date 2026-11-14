import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MatTabsModule } from '@angular/material/tabs';
import { SchoolSummaryDto } from '../../web-api-client';
import { SchoolInvoicesComponent } from './school-invoices.component';
import { SchoolModulesComponent } from './school-modules.component';
import { SchoolProfileComponent } from './school-profile.component';
import { SchoolSubscriptionComponent } from './school-subscription.component';

/**
 * Master portal: one school's edit view, in tabs (Material 3 tabs): the profile (name and domain), the plan and its
 * modules, and the invoices. Each tab keeps its own data and reloads it when the school changes. A saved profile is passed
 * up so the schools list shows the new name and domain.
 */
@Component({
  selector: 'app-school-edit',
  imports: [MatTabsModule, SchoolProfileComponent, SchoolSubscriptionComponent, SchoolModulesComponent, SchoolInvoicesComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './school-edit.component.html',
  styleUrl: './school-edit.component.css',
})
export class SchoolEditComponent {
  readonly school = input.required<SchoolSummaryDto>();

  /** The school as the server now holds it, after its profile was saved. */
  readonly saved = output<SchoolSummaryDto>();
}
