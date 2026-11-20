import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { provideNativeDateAdapter } from '@angular/material/core';
import { MatTabsModule } from '@angular/material/tabs';
import { CampusOption } from '../../shared/components/campus-filter/campus-filter.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { CampusesClient } from '../../web-api-client';
import { AcademicYearsPanelComponent } from './academic-years-panel.component';
import { SchoolCalendarPanelComponent } from './school-calendar-panel.component';
import { ShiftsPanelComponent } from './shifts-panel.component';

/**
 * The academic calendar of the school (Academic setup module): its academic years and terms, the shifts of each campus,
 * and the calendar of holidays and events. The date pickers and the calendar are Angular Material's, in the theme's tokens,
 * with dates in the page's language.
 */
@Component({
  selector: 'app-academic-calendar-page',
  imports: [MatTabsModule, PageHeaderComponent, AcademicYearsPanelComponent, ShiftsPanelComponent, SchoolCalendarPanelComponent],
  providers: [provideNativeDateAdapter()],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './academic-calendar.page.html',
  styleUrl: './academic-calendar.page.css',
})
export class AcademicCalendarPage {
  private readonly campusesClient = inject(CampusesClient);

  /** The school's campuses, shared by the shifts and the calendar. A failure leaves the list empty (whole school only). */
  protected readonly campuses = signal<readonly CampusOption[]>([]);

  constructor() {
    this.campusesClient.getCampuses().subscribe({
      next: (campuses) => this.campuses.set(campuses.map((campus) => ({ id: campus.id, name: campus.name }))),
      error: () => this.campuses.set([]),
    });
  }
}
