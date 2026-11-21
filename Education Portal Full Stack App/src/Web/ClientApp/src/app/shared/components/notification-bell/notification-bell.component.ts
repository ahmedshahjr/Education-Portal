import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { LocalDatePipe } from '../../../core/i18n/local-date.pipe';
import { NotificationDto, NotificationsClient } from '../../../web-api-client';
import { AppIconComponent } from '../app-icon/app-icon.component';

/**
 * The bell in the top bar: how many notifications the signed-in user has not read, and a menu with the latest ones. Choosing one
 * marks it as read. A user who cannot read notifications (or a failed request) simply sees no bell, never an error.
 */
@Component({
  selector: 'app-notification-bell',
  imports: [MatButtonModule, MatMenuModule, MatTooltipModule, AppIconComponent, LocalDatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './notification-bell.component.html',
  styleUrl: './notification-bell.component.scss',
})
export class NotificationBellComponent implements OnInit {
  private readonly client = inject(NotificationsClient);

  protected readonly unreadLabel = $localize`:@@notifications.unreadLabel:Notifications, you have unread ones`;
  protected readonly noUnreadLabel = $localize`:@@notifications.noUnreadLabel:Notifications, none unread`;

  protected readonly available = signal(false);
  protected readonly items = signal<readonly NotificationDto[]>([]);
  protected readonly unread = signal(0);

  ngOnInit(): void {
    this.load();
  }

  /** Reads the notifications again. The menu does it each time it opens, so it never shows a stale list. */
  protected load(): void {
    this.client.getMyNotifications().subscribe({
      next: (notifications) => {
        this.items.set(notifications.items);
        this.unread.set(notifications.unreadCount);
        this.available.set(true);
      },
      error: () => this.available.set(false),
    });
  }

  protected open(notification: NotificationDto): void {
    if (notification.readAt) {
      return;
    }

    this.client.markNotificationRead(notification.id).subscribe({ next: () => this.load(), error: () => this.load() });
  }
}
