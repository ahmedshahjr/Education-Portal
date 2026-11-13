import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable } from 'rxjs';
import { problemMessages } from '../../core/http/problem-details';
import { LocalDatePipe } from '../../core/i18n/local-date.pipe';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { AddOnRequestDto, AddOnRequestsClient, AddOnRequestStatus } from '../../web-api-client';

/**
 * Master portal: add-on modules that schools asked for. Approving one adds the module to the school's subscription at
 * once, as if the master had added it by hand; rejecting changes nothing. Master tenant administrators only; the server
 * enforces it.
 */
@Component({
  selector: 'app-add-on-requests-page',
  imports: [LocalDatePipe, MatCardModule, MatButtonModule, PageHeaderComponent, LoadingStateComponent, EmptyStateComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './add-on-requests.page.html',
  styleUrl: './add-on-requests.page.css',
})
export class AddOnRequestsPage {
  private readonly client = inject(AddOnRequestsClient);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly requests = signal<readonly AddOnRequestDto[] | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly busy = signal(false);

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loadError.set(null);
    this.requests.set(null);
    this.client.getAddOnRequests(AddOnRequestStatus.Pending).subscribe({
      next: (requests) => this.requests.set(requests),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  protected approve(request: AddOnRequestDto): void {
    this.decide(this.client.approveAddOnRequest(request.id), $localize`:@@addOnRequests.approved:Request approved: the module was added.`);
  }

  protected reject(request: AddOnRequestDto): void {
    this.decide(this.client.rejectAddOnRequest(request.id), $localize`:@@addOnRequests.rejected:Request rejected.`);
  }

  private decide(request: Observable<void>, done: string): void {
    this.busy.set(true);
    request.subscribe({
      next: () => {
        this.busy.set(false);
        this.snackBar.open(done, undefined, { duration: 3000 });
        this.load();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.snackBar.open(problemMessages(error)[0], undefined, { duration: 5000 });
      },
    });
  }
}
