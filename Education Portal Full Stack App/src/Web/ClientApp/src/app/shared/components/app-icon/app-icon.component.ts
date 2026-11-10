import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import {
  LucideArrowDownRight,
  LucideArrowUpRight,
  LucideBanknote,
  LucideBell,
  LucideBlocks,
  LucideBookOpen,
  LucideCalendar,
  LucideChartColumn,
  LucideChartPie,
  LucideCheck,
  LucideChevronDown,
  LucideChevronRight,
  LucideCircleQuestionMark,
  LucideCircleUser,
  LucideClock,
  LucideCreditCard,
  LucideDollarSign,
  LucideDynamicIcon,
  LucideEllipsisVertical,
  LucideFileText,
  LucideFlag,
  LucideGitFork,
  LucideGlobe,
  LucideHouse,
  LucideIcon,
  LucideInbox,
  LucideInfo,
  LucideKanban,
  LucideKeyRound,
  LucideLayers,
  LucideLayoutGrid,
  LucideLock,
  LucideLogOut,
  LucideMail,
  LucideMenu,
  LucideMessageCircle,
  LucideMonitor,
  LucideMoon,
  LucideMousePointerClick,
  LucideNewspaper,
  LucideOctagonExclamationPoint,
  LucidePhone,
  LucidePlay,
  LucidePlus,
  LucideReceipt,
  LucideRocket,
  LucideSearch,
  LucideSend,
  LucideSettings,
  LucideShieldCheck,
  LucideShoppingCart,
  LucideSlidersHorizontal,
  LucideSquareCheck,
  LucideSquarePen,
  LucideSun,
  LucideTable,
  LucideTextCursorInput,
  LucideTicket,
  LucideTrash2,
  LucideTrendingUp,
  LucideUserPlus,
  LucideUsers,
  LucideWallet,
  LucideWrench,
  LucideX,
} from '@lucide/angular';
import { AppIconName } from '../../models/navigation.models';

const ICON_REGISTRY: Record<AppIconName, LucideIcon> = {
  'layout-grid': LucideLayoutGrid,
  'bar-chart-3': LucideChartColumn,
  monitor: LucideMonitor,
  home: LucideHouse,
  'message-circle': LucideMessageCircle,
  calendar: LucideCalendar,
  mail: LucideMail,
  kanban: LucideKanban,
  'user-circle': LucideCircleUser,
  'shopping-cart': LucideShoppingCart,
  phone: LucidePhone,
  'book-open': LucideBookOpen,
  users: LucideUsers,
  'file-text': LucideFileText,
  ticket: LucideTicket,
  receipt: LucideReceipt,
  'check-square': LucideSquareCheck,
  newspaper: LucideNewspaper,
  'shield-check': LucideShieldCheck,
  'git-fork': LucideGitFork,
  'dollar-sign': LucideDollarSign,
  settings: LucideSettings,
  'help-circle': LucideCircleQuestionMark,
  layers: LucideLayers,
  blocks: LucideBlocks,
  'form-input': LucideTextCursorInput,
  table: LucideTable,
  'pie-chart': LucideChartPie,
  lock: LucideLock,
  'user-plus': LucideUserPlus,
  'key-round': LucideKeyRound,
  'alert-triangle': LucideOctagonExclamationPoint,
  wrench: LucideWrench,
  'trending-up': LucideTrendingUp,
  wallet: LucideWallet,
  banknote: LucideBanknote,
  flag: LucideFlag,
  clock: LucideClock,
  'mouse-pointer-click': LucideMousePointerClick,
  send: LucideSend,
  play: LucidePlay,
  rocket: LucideRocket,
  'credit-card': LucideCreditCard,
  'arrow-up-right': LucideArrowUpRight,
  'arrow-down-right': LucideArrowDownRight,
  globe: LucideGlobe,
  bell: LucideBell,
  inbox: LucideInbox,
  search: LucideSearch,
  sun: LucideSun,
  moon: LucideMoon,
  menu: LucideMenu,
  'more-vertical': LucideEllipsisVertical,
  plus: LucidePlus,
  'trash-2': LucideTrash2,
  'edit-3': LucideSquarePen,
  check: LucideCheck,
  x: LucideX,
  'chevron-down': LucideChevronDown,
  'chevron-right': LucideChevronRight,
  'sliders-horizontal': LucideSlidersHorizontal,
  info: LucideInfo,
  'log-out': LucideLogOut,
};

@Component({
  selector: 'app-icon',
  imports: [LucideDynamicIcon],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <svg
      [lucideIcon]="iconComponent()"
      [size]="size()"
      [strokeWidth]="strokeWidth()"
      [class]="iconClass()"
      aria-hidden="true"
    ></svg>
  `,
  styles: `
    :host {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      line-height: 1;
      flex-shrink: 0;
    }
  `,
})
export class AppIconComponent {
  readonly name = input.required<AppIconName>();
  readonly size = input<number>(20);
  readonly strokeWidth = input<number>(1.75);
  readonly iconClass = input<string>('');

  protected readonly iconComponent = computed(() => ICON_REGISTRY[this.name()]);
}
