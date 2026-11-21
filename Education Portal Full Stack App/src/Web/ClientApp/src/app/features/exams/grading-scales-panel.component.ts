import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { problemMessages } from '../../core/http/problem-details';
import { AppIconComponent } from '../../shared/components/app-icon/app-icon.component';
import { DataTableColumn, DataTableComponent } from '../../shared/components/data-table/data-table.component';
import { ToastService } from '../../shared/services/toast.service';
import { CreateGradingScaleCommand, GradeBandDto, GradingScaleDto, GradingScalesClient, IGradeBandDto } from '../../web-api-client';
import { bandsOverlap, describeBands } from './exams.logic';

export const SCALE_NAME_MAX_LENGTH = 100;

/** A grading scale as the table shows it: its name and its bands in one line. */
interface ScaleRow {
  readonly id: string;
  readonly name: string;
  readonly bands: string;
}

/**
 * The grading scales of the school: each is a list of bands (a range of percent, a letter and grade points). The server checks
 * that the bands cover 0 to 100 without gaps or overlaps; the form only catches the plain mistakes first.
 */
@Component({
  selector: 'app-grading-scales-panel',
  imports: [ReactiveFormsModule, MatButtonModule, MatCardModule, MatFormFieldModule, MatInputModule, AppIconComponent, DataTableComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './grading-scales-panel.component.html',
  styleUrl: '../academics/academics.css',
})
export class GradingScalesPanelComponent implements OnInit {
  private readonly client = inject(GradingScalesClient);
  private readonly toast = inject(ToastService);
  private readonly formBuilder = inject(FormBuilder);

  protected readonly nameMax = SCALE_NAME_MAX_LENGTH;

  protected readonly scales = signal<readonly GradingScaleDto[] | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly adding = signal(false);
  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  protected readonly rows = computed<readonly ScaleRow[]>(() =>
    (this.scales() ?? []).map((scale) => ({ id: scale.id, name: scale.name, bands: describeBands(scale.bands) }))
  );

  protected readonly columns: DataTableColumn<ScaleRow>[] = [
    { key: 'name', header: $localize`:@@exams.colScale:Grading scale`, valueFn: (s) => s.name },
    { key: 'bands', header: $localize`:@@exams.colBands:Bands`, valueFn: (s) => s.bands },
  ];

  protected readonly form = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(SCALE_NAME_MAX_LENGTH)]],
    bands: this.formBuilder.array([this.newBand()]),
  });

  ngOnInit(): void {
    this.load();
  }

  protected get bands(): FormArray {
    return this.form.controls.bands;
  }

  private load(): void {
    this.loadError.set(null);
    this.client.getGradingScales().subscribe({
      next: (scales) => this.scales.set(scales),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  private newBand() {
    return this.formBuilder.nonNullable.group({
      minPercent: [0, [Validators.required, Validators.min(0), Validators.max(100)]],
      maxPercent: [100, [Validators.required, Validators.min(0), Validators.max(100)]],
      letter: ['', [Validators.required, Validators.maxLength(5)]],
      points: [0, [Validators.required, Validators.min(0)]],
    });
  }

  protected startAdd(): void {
    this.errors.set([]);
    this.form.reset({ name: '' });
    this.bands.clear();
    this.bands.push(this.newBand());
    this.adding.set(true);
  }

  protected addBand(): void {
    this.bands.push(this.newBand());
  }

  protected removeBand(index: number): void {
    if (this.bands.length > 1) {
      this.bands.removeAt(index);
    }
  }

  /** True when the bands typed so far have a band that ends before it starts, or two bands that share percent. */
  protected get bandsProblem(): boolean {
    const bands = this.bands.getRawValue() as IGradeBandDto[];
    return bands.some((band) => band.minPercent > band.maxPercent) || bandsOverlap(bands);
  }

  protected save(): void {
    if (this.form.invalid || this.bandsProblem) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const command = new CreateGradingScaleCommand({
      name: value.name.trim(),
      bands: (value.bands as IGradeBandDto[]).map((band) => new GradeBandDto({ ...band, letter: band.letter.trim() })),
    });
    this.busy.set(true);
    this.errors.set([]);
    this.client.createGradingScale(command).subscribe({
      next: () => {
        this.busy.set(false);
        this.adding.set(false);
        this.toast.success($localize`:@@exams.scaleSaved:Grading scale saved.`);
        this.load();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }
}
