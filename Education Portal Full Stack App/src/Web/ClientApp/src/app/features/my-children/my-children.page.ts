import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { problemMessages } from '../../core/http/problem-details';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { ParentsClient, UserDetailsDto } from '../../web-api-client';

/**
 * A parent's own view: the students linked to the signed-in parent, and no one else's (the server decides). For anyone
 * else the list is empty.
 */
@Component({
  selector: 'app-my-children-page',
  imports: [MatCardModule, PageHeaderComponent, LoadingStateComponent, EmptyStateComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './my-children.page.html',
  styleUrl: './my-children.page.css',
})
export class MyChildrenPage {
  private readonly parents = inject(ParentsClient);

  protected readonly children = signal<readonly UserDetailsDto[] | null>(null);
  protected readonly loadError = signal<string | null>(null);

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loadError.set(null);
    this.parents.getMyChildren().subscribe({
      next: (children) => this.children.set(children),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  protected fullName(child: UserDetailsDto): string {
    return `${child.firstName}${child.middleName ? ' ' + child.middleName : ''} ${child.lastName}`;
  }
}
