import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatTabsModule } from '@angular/material/tabs';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { AssessmentsPanelComponent } from './assessments-panel.component';
import { GradingScalesPanelComponent } from './grading-scales-panel.component';
import { ReportCardPanelComponent } from './report-card-panel.component';

/** Exams (Exams module): the grading scales, the assessments of each course with their scores, and report cards. */
@Component({
  selector: 'app-exams-page',
  imports: [MatTabsModule, PageHeaderComponent, GradingScalesPanelComponent, AssessmentsPanelComponent, ReportCardPanelComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './exams.page.html',
  styleUrl: '../academics/academics.css',
})
export class ExamsPage {}
