export type NavBadgeVariant = 'primary' | 'secondary' | 'success' | 'warning' | 'error' | 'outlined';

export type AppIconName =
  | 'layout-grid'
  | 'git-branch'
  | 'bar-chart-3'
  | 'monitor'
  | 'home'
  | 'message-circle'
  | 'calendar'
  | 'mail'
  | 'kanban'
  | 'user-circle'
  | 'shopping-cart'
  | 'phone'
  | 'book-open'
  | 'users'
  | 'file-text'
  | 'ticket'
  | 'receipt'
  | 'check-square'
  | 'newspaper'
  | 'shield-check'
  | 'git-fork'
  | 'dollar-sign'
  | 'settings'
  | 'help-circle'
  | 'layers'
  | 'blocks'
  | 'form-input'
  | 'table'
  | 'pie-chart'
  | 'lock'
  | 'user-plus'
  | 'key-round'
  | 'alert-triangle'
  | 'wrench'
  | 'trending-up'
  | 'wallet'
  | 'banknote'
  | 'flag'
  | 'clock'
  | 'mouse-pointer-click'
  | 'send'
  | 'play'
  | 'rocket'
  | 'credit-card'
  | 'arrow-up-right'
  | 'arrow-down-right'
  | 'globe'
  | 'bell'
  | 'inbox'
  | 'search'
  | 'sun'
  | 'moon'
  | 'menu'
  | 'more-vertical'
  | 'plus'
  | 'trash-2'
  | 'edit-3'
  | 'check'
  | 'x'
  | 'chevron-down'
  | 'chevron-right'
  | 'sliders-horizontal'
  | 'info'
  | 'log-out';

export interface NavItem {
  readonly id: string;
  readonly label: string;
  readonly icon: AppIconName;
  readonly route?: string;
  readonly queryParams?: Record<string, string>;
  readonly badge?: string;
  readonly badgeVariant?: NavBadgeVariant;
  readonly disabled?: boolean;
  readonly children?: readonly NavItem[];
}

export interface NavSection {
  readonly id: string;
  readonly title: string;
  readonly items: readonly NavItem[];
}

export interface BreadcrumbItem {
  readonly label: string;
  readonly route?: string;
}
