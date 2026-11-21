import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatTabsModule } from '@angular/material/tabs';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { CurriculaPanelComponent } from './curricula-panel.component';
import { FacilitiesPanelComponent } from './facilities-panel.component';
import { PhotosPanelComponent } from './photos-panel.component';
import { ProfilePanelComponent } from './profile-panel.component';

/**
 * The school's public page (School module): its text, curricula, facilities and photos, in tabs. The page is a draft until the
 * school publishes it; the server decides what each user may change.
 */
@Component({
  selector: 'app-school-page-page',
  imports: [MatTabsModule, PageHeaderComponent, ProfilePanelComponent, CurriculaPanelComponent, FacilitiesPanelComponent, PhotosPanelComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './school-page.page.html',
  styleUrl: './school-page.css',
})
export class SchoolPagePage {}
