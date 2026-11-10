import { AppIconName } from './navigation.models';

export type SemanticTone = 'primary' | 'secondary' | 'success' | 'warning' | 'error' | 'info';

export type OverallBalanceTab = 'orders' | 'income';
export type RoiPeriod = 'h1' | 'h2';
export type ProjectCategory = 'SaaS' | 'Mobile' | 'Others';
export type DeliveryTab = 'email' | 'last_week';
export type TransactionStatus = 'Approved' | 'Pending' | 'Declined';

export interface MiniStatTile {
  readonly id: string;
  readonly label: string;
  readonly value: string;
  readonly icon: AppIconName;
  readonly tone: SemanticTone;
}

export interface OverallBalanceData {
  readonly totalBalance: string;
  readonly deltaPercent: string;
  readonly deltaLabel: string;
  readonly ordersCategories: readonly string[];
  readonly ordersPrimarySeries: readonly number[];
  readonly ordersComparisonSeries: readonly number[];
  readonly incomeCategories: readonly string[];
  readonly incomeSeries: readonly number[];
  readonly miniStats: readonly MiniStatTile[];
}

export interface RoiData {
  readonly period: RoiPeriod;
  readonly percentage: string;
  readonly delta: string;
  readonly monthLabel: string;
  readonly categories: readonly string[];
  readonly values: readonly number[];
  readonly highlightIndex: number;
}

export interface MarketingMetric {
  readonly id: string;
  readonly label: string;
  readonly value: string;
  readonly icon: AppIconName;
}

export interface MarketingReportData {
  readonly metrics: readonly MarketingMetric[];
  readonly score: number;
  readonly caption: string;
  readonly gaugeSeries: readonly number[];
  readonly ctaText: string;
}

export interface PaymentCard {
  readonly id: string;
  readonly brand: 'visa' | 'mastercard';
  readonly last4: string;
  readonly balance: number;
  readonly holder: string;
  readonly expiry: string;
  readonly theme: 'dark' | 'light';
}

export interface TransactionItem {
  readonly id: string;
  readonly merchant: string;
  readonly shortCode: string;
  readonly dateLabel: string;
  readonly status: TransactionStatus;
  readonly amount: number;
  readonly isCredit: boolean;
  readonly bonusLabel: string;
  readonly cardId: string;
  readonly category: string;
}

export interface TeamAvatar {
  readonly initials: string;
  readonly name: string;
  readonly color: string;
  readonly bg: string;
}

export interface ProjectItem {
  readonly id: string;
  readonly name: string;
  readonly brandMark: string;
  readonly brandColor: string;
  readonly brandBg: string;
  readonly budget: number;
  readonly team: readonly TeamAvatar[];
  readonly extraMembers: number;
  readonly leader: string;
  readonly category: ProjectCategory;
  readonly trendData: readonly number[];
  readonly trendColor: string;
  readonly status: 'Active' | 'In Review' | 'Completed';
  readonly updatedAt: string;
}

export interface DeliveryMetric {
  readonly id: string;
  readonly label: string;
  readonly value: string;
  readonly color: string;
}

export interface DeliveryAnalyticsDataset {
  readonly headline: string;
  readonly subheadline: string;
  readonly categories: readonly string[];
  readonly primaryName: string;
  readonly primarySeries: readonly number[];
  readonly secondaryName: string;
  readonly secondarySeries: readonly number[];
  readonly metrics: readonly DeliveryMetric[];
}

export interface EarningReportItem {
  readonly id: string;
  readonly title: string;
  readonly subtitle: string;
  readonly details: string;
  readonly growthPercent: string;
  readonly icon: AppIconName;
  readonly tone: SemanticTone;
  readonly revenue: string;
}

export interface AppNotification {
  readonly id: string;
  readonly title: string;
  readonly description: string;
  readonly time: string;
  readonly unread: boolean;
  readonly tone: SemanticTone;
  readonly icon: AppIconName;
}

export interface AppMessage {
  readonly id: string;
  readonly sender: string;
  readonly initials: string;
  readonly avatarBg: string;
  readonly avatarColor: string;
  readonly preview: string;
  readonly time: string;
  readonly unread: boolean;
}

export interface UserProfile {
  readonly id: string;
  readonly name: string;
  readonly email: string;
  readonly role: string;
  readonly company: string;
  readonly location: string;
  readonly phone: string;
  readonly bio: string;
  readonly avatarInitials: string;
}
