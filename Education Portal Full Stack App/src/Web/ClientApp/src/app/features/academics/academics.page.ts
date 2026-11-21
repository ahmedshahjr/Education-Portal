import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatTabsModule } from '@angular/material/tabs';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { CoursesPanelComponent } from './courses-panel.component';
import { EnrolmentsPanelComponent } from './enrolments-panel.component';

/** The courses of the school and the enrolment of its students in class offerings (Academic setup module). */
@Component({
  selector: 'app-academics-page',
  imports: [MatTabsModule, PageHeaderComponent, CoursesPanelComponent, EnrolmentsPanelComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './academics.page.html',
  styleUrl: './academics.css',
})
export class AcademicsPage {}
