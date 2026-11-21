import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { problemMessages } from '../../core/http/problem-details';
import { ToastService } from '../../shared/services/toast.service';
import { CurriculumKind, SchoolCurriculaClient, SetCurriculaCommand } from '../../web-api-client';
import { CURRICULUM_KINDS, curriculumLabel } from './school-page.labels';

/** The curricula the school teaches, chosen from a closed list. They are saved together. */
@Component({
  selector: 'app-curricula-panel',
  imports: [MatButtonModule, MatCardModule, MatCheckboxModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './curricula-panel.component.html',
  styleUrl: './school-page.css',
})
export class CurriculaPanelComponent implements OnInit {
  private readonly client = inject(SchoolCurriculaClient);
  private readonly toast = inject(ToastService);

  protected readonly kinds = CURRICULUM_KINDS;
  protected readonly label = curriculumLabel;

  protected readonly selected = signal<ReadonlySet<CurriculumKind>>(new Set());
  protected readonly loading = signal(true);
  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  ngOnInit(): void {
    this.client.getCurricula(undefined).subscribe({
      next: (kinds) => {
        this.selected.set(new Set(kinds));
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.loading.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }

  protected toggle(kind: CurriculumKind, checked: boolean): void {
    const next = new Set(this.selected());
    if (checked) {
      next.add(kind);
    } else {
      next.delete(kind);
    }
    this.selected.set(next);
  }

  protected save(): void {
    this.busy.set(true);
    this.errors.set([]);
    // The list keeps the order of the closed list, so the same choice is always sent the same way.
    const kinds = this.kinds.filter((kind) => this.selected().has(kind));
    this.client.setCurricula(undefined, new SetCurriculaCommand({ kinds })).subscribe({
      next: () => {
        this.busy.set(false);
        this.toast.success($localize`:@@schoolPage.curriculaSaved:Curricula saved.`);
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }
}
