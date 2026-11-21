import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { problemMessages } from '../../core/http/problem-details';
import { GradingScaleDto, GradingScalesClient, ReportCardDto, ReportCardsClient } from '../../web-api-client';
import { AcademicsLookups } from '../academics/academics-lookups.service';
import { offeringLabel, personName } from '../academics/academics.logic';

/** A student's report card for one class offering: each course's percent, letter and points, and the grade point average. */
@Component({
  selector: 'app-report-card-panel',
  imports: [ReactiveFormsModule, MatButtonModule, MatCardModule, MatFormFieldModule, MatSelectModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './report-card-panel.component.html',
  styleUrl: '../academics/academics.css',
})
export class ReportCardPanelComponent implements OnInit {
  private readonly reportCards = inject(ReportCardsClient);
  private readonly scalesClient = inject(GradingScalesClient);
  protected readonly lookups = inject(AcademicsLookups);

  protected readonly offeringLabel = offeringLabel;
  protected readonly personName = personName;

  protected readonly form = inject(FormBuilder).nonNullable.group({
    studentId: ['', Validators.required],
    classOfferingId: ['', Validators.required],
    gradingScaleId: ['', Validators.required],
  });

  protected readonly scales = signal<readonly GradingScaleDto[]>([]);
  protected readonly card = signal<ReportCardDto | null>(null);
  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  ngOnInit(): void {
    this.lookups.loadStudents();
    this.lookups.loadOfferings();
    this.scalesClient.getGradingScales().subscribe({
      next: (scales) => this.scales.set(scales),
      error: (error: unknown) => this.errors.set(problemMessages(error)),
    });
  }

  protected show(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { studentId, classOfferingId, gradingScaleId } = this.form.getRawValue();
    this.busy.set(true);
    this.errors.set([]);
    this.card.set(null);
    this.reportCards.getReportCard(studentId, classOfferingId, gradingScaleId).subscribe({
      next: (card) => {
        this.busy.set(false);
        this.card.set(card);
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }
}
