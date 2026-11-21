import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Observable } from 'rxjs';
import { problemMessages } from '../../core/http/problem-details';
import { ToastService } from '../../shared/services/toast.service';
import { FacilityKind, SchoolFacilitiesClient, SetFacilityCommand } from '../../web-api-client';
import { FACILITY_KINDS, facilityLabel } from './school-page.labels';

export const FACILITY_DESCRIPTION_MAX_LENGTH = 300;

/** One facility the school can offer: whether it has it, and an optional note. */
interface FacilityRow {
  readonly kind: FacilityKind;
  readonly has: boolean;
  readonly description: FormControl<string>;
}

/** The facilities of the school (library, labs, transport...), each switched on or off with a short note. */
@Component({
  selector: 'app-facilities-panel',
  imports: [ReactiveFormsModule, MatButtonModule, MatCardModule, MatCheckboxModule, MatFormFieldModule, MatInputModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './facilities-panel.component.html',
  styleUrl: './school-page.css',
})
export class FacilitiesPanelComponent implements OnInit {
  private readonly client = inject(SchoolFacilitiesClient);
  private readonly toast = inject(ToastService);

  protected readonly label = facilityLabel;
  protected readonly descriptionMax = FACILITY_DESCRIPTION_MAX_LENGTH;

  protected readonly rows = signal<readonly FacilityRow[]>([]);
  protected readonly loading = signal(true);
  protected readonly busyKind = signal<FacilityKind | null>(null);
  protected readonly errors = signal<readonly string[]>([]);

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.client.getFacilities(undefined).subscribe({
      next: (facilities) => {
        const byKind = new Map(facilities.map((facility) => [facility.kind, facility]));
        this.rows.set(
          FACILITY_KINDS.map((kind) => ({
            kind,
            has: byKind.has(kind),
            description: new FormControl(byKind.get(kind)?.description ?? '', {
              nonNullable: true,
              validators: Validators.maxLength(FACILITY_DESCRIPTION_MAX_LENGTH),
            }),
          }))
        );
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }

  protected toggle(row: FacilityRow, has: boolean): void {
    const request = has
      ? this.client.setFacility(undefined, row.kind, new SetFacilityCommand({ description: row.description.value.trim() || undefined }))
      : this.client.removeFacility(undefined, row.kind);
    this.run(row, request, has ? $localize`:@@schoolPage.facilityAdded:Facility added.` : $localize`:@@schoolPage.facilityRemoved:Facility removed.`);
  }

  protected saveNote(row: FacilityRow): void {
    if (row.description.invalid) {
      row.description.markAsTouched();
      return;
    }
    this.run(
      row,
      this.client.setFacility(undefined, row.kind, new SetFacilityCommand({ description: row.description.value.trim() || undefined })),
      $localize`:@@schoolPage.facilitySaved:Facility saved.`
    );
  }

  private run(row: FacilityRow, request: Observable<unknown>, done: string): void {
    this.busyKind.set(row.kind);
    this.errors.set([]);
    request.subscribe({
      next: () => {
        this.busyKind.set(null);
        this.toast.success(done);
        this.load();
      },
      error: (error: unknown) => {
        this.busyKind.set(null);
        this.errors.set(problemMessages(error));
      },
    });
  }
}
