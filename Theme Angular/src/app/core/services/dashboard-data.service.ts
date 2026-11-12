import { computed, Injectable, signal } from '@angular/core';
import { delay, Observable, of } from 'rxjs';
import {
  DeliveryAnalyticsDataset,
  DeliveryTab,
  EarningReportItem,
  MarketingReportData,
  OverallBalanceData,
  PaymentCard,
  ProjectCategory,
  ProjectItem,
  RoiData,
  RoiPeriod,
  TeamAvatar,
  TransactionItem,
} from '../../shared/models/dashboard.models';

const TEAM_POOL: readonly TeamAvatar[] = [
  { initials: 'EM', name: 'Erin', color: '#ffffff', bg: '#00a1ff' },
  { initials: 'TH', name: 'Timothy', color: '#ffffff', bg: '#ff6692' },
  { initials: 'KC', name: 'Kristen', color: '#ffffff', bg: '#8965e5' },
];

const INITIAL_PROJECTS: readonly ProjectItem[] = [
  {
    id: 'prj-1',
    name: 'Photoshop',
    brandMark: 'Ps',
    brandColor: '#ffffff',
    brandBg: '#0b2545',
    budget: 29374.6,
    team: TEAM_POOL,
    extraMembers: 3,
    leader: 'Erin',
    category: 'SaaS',
    trendData: [24, 38, 30, 48, 42, 56, 51],
    trendColor: '#00ceb6',
    status: 'Active',
    updatedAt: 'Today, 10:15 AM',
  },
  {
    id: 'prj-2',
    name: 'Website SEO',
    brandMark: 'SEO',
    brandColor: '#ffffff',
    brandBg: '#ffae1f',
    budget: 1843.73,
    team: TEAM_POOL,
    extraMembers: 3,
    leader: 'Timothy',
    category: 'SaaS',
    trendData: [40, 29, 36, 25, 34, 22, 31],
    trendColor: '#ff6692',
    status: 'In Review',
    updatedAt: 'Today, 08:40 AM',
  },
  {
    id: 'prj-3',
    name: 'iOS Mobile App Design',
    brandMark: 'V',
    brandColor: '#00ceb6',
    brandBg: 'rgba(0, 206, 182, 0.14)',
    budget: 0.9989,
    team: TEAM_POOL,
    extraMembers: 3,
    leader: 'Tyler',
    category: 'Mobile',
    trendData: [18, 28, 22, 35, 29, 44, 38],
    trendColor: '#00ceb6',
    status: 'Active',
    updatedAt: 'Yesterday',
  },
  {
    id: 'prj-4',
    name: 'Figma Components',
    brandMark: 'F',
    brandColor: '#ff6692',
    brandBg: 'rgba(255, 102, 146, 0.14)',
    budget: 238.61,
    team: TEAM_POOL,
    extraMembers: 3,
    leader: 'Kristen',
    category: 'SaaS',
    trendData: [32, 24, 35, 29, 38, 31, 40],
    trendColor: '#ffae1f',
    status: 'Active',
    updatedAt: 'Aug 16, 2026',
  },
  {
    id: 'prj-5',
    name: 'Web App Design',
    brandMark: 'Re',
    brandColor: '#00a1ff',
    brandBg: 'rgba(0, 161, 255, 0.14)',
    budget: 0.629,
    team: TEAM_POOL,
    extraMembers: 3,
    leader: 'Isabelle',
    category: 'Others',
    trendData: [22, 34, 26, 40, 32, 46, 39],
    trendColor: '#8965e5',
    status: 'Completed',
    updatedAt: 'Aug 14, 2026',
  },
  {
    id: 'prj-6',
    name: 'Android Checkout SDK',
    brandMark: 'An',
    brandColor: '#ffffff',
    brandBg: '#00ceb6',
    budget: 12450.0,
    team: TEAM_POOL,
    extraMembers: 4,
    leader: 'Marcus',
    category: 'Mobile',
    trendData: [20, 32, 40, 35, 52, 49, 64],
    trendColor: '#00a1ff',
    status: 'Active',
    updatedAt: 'Aug 12, 2026',
  },
  {
    id: 'prj-7',
    name: 'Design Tokens Pipeline',
    brandMark: 'DT',
    brandColor: '#ffffff',
    brandBg: '#8965e5',
    budget: 4920.5,
    team: TEAM_POOL,
    extraMembers: 2,
    leader: 'Sophia',
    category: 'Others',
    trendData: [28, 30, 38, 41, 44, 52, 58],
    trendColor: '#8965e5',
    status: 'Completed',
    updatedAt: 'Aug 10, 2026',
  },
];

const INITIAL_CARDS: readonly PaymentCard[] = [
  {
    id: 'card-visa',
    brand: 'visa',
    last4: '8942',
    balance: 26561.5,
    holder: 'Mathew Anderson',
    expiry: '08/28',
    theme: 'dark',
  },
  {
    id: 'card-mc',
    brand: 'mastercard',
    last4: '8942',
    balance: 26561.5,
    holder: 'Mathew Anderson',
    expiry: '11/27',
    theme: 'light',
  },
];

const INITIAL_TRANSACTIONS: readonly TransactionItem[] = [
  {
    id: 'tx-1',
    merchant: 'Netflix Subscription',
    shortCode: 'N',
    dateLabel: 'Today, 09:23am',
    status: 'Approved',
    amount: 4.58,
    isCredit: false,
    bonusLabel: '+5 Bonus',
    cardId: 'card-visa',
    category: 'Entertainment',
  },
  {
    id: 'tx-2',
    merchant: 'Upwork',
    shortCode: 'Up',
    dateLabel: 'August 15, 10:35pm',
    status: 'Pending',
    amount: 15.6,
    isCredit: true,
    bonusLabel: '+30 Bonus',
    cardId: 'card-visa',
    category: 'Freelance',
  },
  {
    id: 'tx-3',
    merchant: 'Figma Enterprise',
    shortCode: 'Fg',
    dateLabel: 'August 14, 04:12pm',
    status: 'Approved',
    amount: 45.0,
    isCredit: false,
    bonusLabel: '+15 Bonus',
    cardId: 'card-mc',
    category: 'Design Tools',
  },
  {
    id: 'tx-4',
    merchant: 'Stripe Payout',
    shortCode: 'St',
    dateLabel: 'August 12, 11:05am',
    status: 'Approved',
    amount: 1280.4,
    isCredit: true,
    bonusLabel: '+120 Bonus',
    cardId: 'card-mc',
    category: 'Revenue',
  },
  {
    id: 'tx-5',
    merchant: 'AWS Cloud Services',
    shortCode: 'AW',
    dateLabel: 'August 10, 07:45am',
    status: 'Approved',
    amount: 184.2,
    isCredit: false,
    bonusLabel: '+20 Bonus',
    cardId: 'card-visa',
    category: 'Infrastructure',
  },
];

@Injectable({ providedIn: 'root' })
export class DashboardDataService {
  private readonly projectsSignal = signal<readonly ProjectItem[]>(INITIAL_PROJECTS);
  private readonly transactionsSignal = signal<readonly TransactionItem[]>(INITIAL_TRANSACTIONS);
  private readonly cardsSignal = signal<readonly PaymentCard[]>(INITIAL_CARDS);

  readonly projects = this.projectsSignal.asReadonly();
  readonly transactions = this.transactionsSignal.asReadonly();
  readonly paymentCards = this.cardsSignal.asReadonly();

  readonly totalProjectBudget = computed(() =>
    this.projectsSignal().reduce((sum, p) => sum + p.budget, 0)
  );

  readonly overallBalanceData: OverallBalanceData = {
    totalBalance: '$2,538,942',
    deltaPercent: '16.3%',
    deltaLabel: 'last 12 months',
    ordersCategories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'July'],
    ordersPrimarySeries: [32, 23, 39, 23, 52, 34, 24],
    ordersComparisonSeries: [27, 13, 27, 46, 30, 52, 37],
    incomeCategories: ['Aug', 'Sep', 'Oct', 'Nov', 'Dec', 'Jan', 'Feb', 'Mar'],
    incomeSeries: [25, 13, 20, 39, 45, 51, 70, 30],
    miniStats: [
      { id: 'sales', label: 'Total Sales', value: '$14,673', icon: 'trending-up', tone: 'warning' },
      { id: 'profit', label: 'Total Profit', value: '$9,281', icon: 'dollar-sign', tone: 'error' },
      { id: 'users', label: 'Total Users', value: '45.1k', icon: 'flag', tone: 'success' },
      { id: 'expense', label: 'Total Expense', value: '$4,673', icon: 'pie-chart', tone: 'primary' },
    ],
  };

  readonly roiDatasets: Record<RoiPeriod, RoiData> = {
    h1: {
      period: 'h1',
      percentage: '283%',
      delta: '+24%',
      monthLabel: 'January',
      categories: ['JAN', 'FEB', 'MAR', 'APR', 'MAY', 'JUN'],
      values: [42, 82, 54, 44, 66, 52],
      highlightIndex: 1,
    },
    h2: {
      period: 'h2',
      percentage: '318%',
      delta: '+31%',
      monthLabel: 'October',
      categories: ['JUL', 'AUG', 'SEP', 'OCT', 'NOV', 'DEC'],
      values: [50, 62, 48, 86, 68, 74],
      highlightIndex: 3,
    },
  };

  readonly marketingReportData: MarketingReportData = {
    metrics: [
      { id: 'search-vol', label: 'Search Volume', value: '+2.9k', icon: 'trending-up' },
      { id: 'return-ratio-1', label: 'Return Ratio', value: '1.22', icon: 'clock' },
      { id: 'clicks-search', label: 'Clicks / search', value: '0.83', icon: 'mouse-pointer-click' },
      { id: 'return-ratio-2', label: 'Return Ratio', value: '41.2', icon: 'clock' },
    ],
    score: 275,
    caption: 'Learn insights how to manage all aspects of your startup.',
    gaugeSeries: [20, 20, 20, 20, 20],
    ctaText: 'Learn insights how to manage all aspects of your startup.',
  };

  readonly deliveryDatasets: Record<DeliveryTab, DeliveryAnalyticsDataset> = {
    email: {
      headline: 'Welcome Emails!',
      subheadline: 'Have great potential!',
      categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
      primaryName: 'Open Rate',
      primarySeries: [72, 42, 68, 70, 64, 62],
      secondaryName: 'Benchmark',
      secondarySeries: [52, 82, 50, 42, 48, 52],
      metrics: [
        { id: 'open', label: 'Open Rate', value: '22.3%', color: '#00a1ff' },
        { id: 'ctr', label: 'Click-through Rate', value: '14.2%', color: '#8965e5' },
        { id: 'conv', label: 'Conversation Rate', value: '11.6%', color: '#ff6692' },
      ],
    },
    last_week: {
      headline: 'Onboarding Drip',
      subheadline: 'Up 4.8% from last week',
      categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
      primaryName: 'Open Rate',
      primarySeries: [64, 78, 58, 76, 72, 68],
      secondaryName: 'Benchmark',
      secondarySeries: [48, 60, 66, 52, 56, 44],
      metrics: [
        { id: 'open', label: 'Open Rate', value: '26.8%', color: '#00a1ff' },
        { id: 'ctr', label: 'Click-through Rate', value: '17.5%', color: '#8965e5' },
        { id: 'conv', label: 'Conversation Rate', value: '13.9%', color: '#ff6692' },
      ],
    },
  };

  readonly earningReports: readonly EarningReportItem[] = [
    {
      id: 'er-1',
      title: 'Bank Transfer',
      subtitle: 'and +1 more',
      details: 'Direct ACH & Wire Settlements across North America',
      growthPercent: '16.3%',
      icon: 'credit-card',
      tone: 'primary',
      revenue: '$48,920',
    },
    {
      id: 'er-2',
      title: 'Net Profit',
      subtitle: 'and +4 more',
      details: 'Operating margin after cloud & payment gateway fees',
      growthPercent: '12.55%',
      icon: 'wallet',
      tone: 'error',
      revenue: '$31,410',
    },
    {
      id: 'er-3',
      title: 'Bank Transfer',
      subtitle: 'and +4 more',
      details: 'EMEA SEPA & UK Faster Payments clearing volume',
      growthPercent: '16.3%',
      icon: 'trending-up',
      tone: 'secondary',
      revenue: '$29,840',
    },
    {
      id: 'er-4',
      title: 'Total Expenses',
      subtitle: 'and +2 more',
      details: 'Marketing acquisition & SaaS infrastructure spend',
      growthPercent: '8.28%',
      icon: 'banknote',
      tone: 'warning',
      revenue: '$14,220',
    },
  ];

  getProjectsStream(category?: ProjectCategory): Observable<readonly ProjectItem[]> {
    const list = this.projectsSignal();
    const filtered = category ? list.filter((p) => p.category === category) : list;
    return of(filtered).pipe(delay(120));
  }

  getProjectsForDashboardTab(category: ProjectCategory): readonly ProjectItem[] {
    if (category === 'SaaS') {
      // Preserve the exact 5 reference rows on default SaaS tab while keeping category filtering meaningful
      return this.projectsSignal().slice(0, 5);
    }
    return this.projectsSignal().filter((p) => p.category === category);
  }

  addProject(project: Omit<ProjectItem, 'id' | 'team' | 'trendData' | 'updatedAt'>): void {
    const newItem: ProjectItem = {
      ...project,
      id: `prj-${Date.now()}`,
      team: TEAM_POOL,
      trendData: [20, 30, 26, 42, 39, 51, 48],
      updatedAt: 'Just now',
    };
    this.projectsSignal.update((prev) => [newItem, ...prev]);
  }

  updateProject(id: string, patch: Partial<ProjectItem>): void {
    this.projectsSignal.update((prev) =>
      prev.map((item) => (item.id === id ? { ...item, ...patch, updatedAt: 'Just now' } : item))
    );
  }

  deleteProject(id: string): void {
    this.projectsSignal.update((prev) => prev.filter((item) => item.id !== id));
  }

  addTransaction(tx: Omit<TransactionItem, 'id'>): void {
    const created: TransactionItem = {
      ...tx,
      id: `tx-${Date.now()}`,
    };
    this.transactionsSignal.update((prev) => [created, ...prev]);
  }
}
