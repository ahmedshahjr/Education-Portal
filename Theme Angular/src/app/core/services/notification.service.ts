import { computed, inject, Injectable, signal } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AppMessage, AppNotification, SemanticTone } from '../../shared/models/dashboard.models';

const INITIAL_NOTIFICATIONS: readonly AppNotification[] = [
  {
    id: 'n-1',
    title: 'Launch Admin Dashboard',
    description: 'Dashboard 3 telemetry synced with Q1 KPI targets',
    time: '9:30 AM',
    unread: true,
    tone: 'primary',
    icon: 'rocket',
  },
  {
    id: 'n-2',
    title: 'New Payment Received',
    description: 'Upwork payout (+$15.60) queued for settlement',
    time: '9:10 AM',
    unread: true,
    tone: 'success',
    icon: 'wallet',
  },
  {
    id: 'n-3',
    title: 'Server Capacity Warning',
    description: 'US-East analytics cluster reached 84% memory',
    time: '8:45 AM',
    unread: true,
    tone: 'warning',
    icon: 'alert-triangle',
  },
  {
    id: 'n-4',
    title: 'Project Budget Updated',
    description: 'Photoshop redesign milestone approved by Erin',
    time: 'Yesterday',
    unread: true,
    tone: 'secondary',
    icon: 'layers',
  },
  {
    id: 'n-5',
    title: 'Security Audit Passed',
    description: 'All API endpoints verified with zero findings',
    time: 'Yesterday',
    unread: true,
    tone: 'info',
    icon: 'shield-check',
  },
];

const INITIAL_MESSAGES: readonly AppMessage[] = [
  {
    id: 'm-1',
    sender: 'Erin McKenzie',
    initials: 'EM',
    avatarBg: 'rgba(0, 161, 255, 0.15)',
    avatarColor: '#00a1ff',
    preview: 'Updated the SaaS Photoshop milestone budget.',
    time: '2m ago',
    unread: true,
  },
  {
    id: 'm-2',
    sender: 'Timothy Hayes',
    initials: 'TH',
    avatarBg: 'rgba(255, 102, 146, 0.15)',
    avatarColor: '#ff6692',
    preview: 'Website SEO audit report is ready for review.',
    time: '18m ago',
    unread: true,
  },
  {
    id: 'm-3',
    sender: 'Kristen Chen',
    initials: 'KC',
    avatarBg: 'rgba(137, 101, 229, 0.15)',
    avatarColor: '#8965e5',
    preview: 'Published Figma design tokens v2.4.',
    time: '1h ago',
    unread: true,
  },
];

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly snackBar = inject(MatSnackBar);

  private readonly notificationsSignal = signal<readonly AppNotification[]>(INITIAL_NOTIFICATIONS);
  private readonly messagesSignal = signal<readonly AppMessage[]>(INITIAL_MESSAGES);

  readonly notifications = this.notificationsSignal.asReadonly();
  readonly messages = this.messagesSignal.asReadonly();

  readonly unreadNotificationCount = computed(
    () => this.notificationsSignal().filter((n) => n.unread).length
  );

  readonly unreadMessageCount = computed(
    () => this.messagesSignal().filter((m) => m.unread).length
  );

  markNotificationRead(id: string): void {
    this.notificationsSignal.update((list) =>
      list.map((item) => (item.id === id ? { ...item, unread: false } : item))
    );
  }

  markAllNotificationsRead(): void {
    this.notificationsSignal.update((list) => list.map((item) => ({ ...item, unread: false })));
    this.notify('All notifications marked as read', 'success');
  }

  markMessageRead(id: string): void {
    this.messagesSignal.update((list) =>
      list.map((item) => (item.id === id ? { ...item, unread: false } : item))
    );
  }

  markAllMessagesRead(): void {
    this.messagesSignal.update((list) => list.map((item) => ({ ...item, unread: false })));
    this.notify('All messages marked as read', 'success');
  }

  notify(message: string, tone: SemanticTone = 'primary'): void {
    this.snackBar.open(message, 'Dismiss', {
      duration: 3200,
      horizontalPosition: 'end',
      verticalPosition: 'bottom',
      panelClass: [`mm-snackbar-${tone}`],
    });
  }
}
