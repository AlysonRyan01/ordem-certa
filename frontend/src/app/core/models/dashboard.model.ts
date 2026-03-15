import { ServiceOrderOutput } from './service-order.model';

export interface StatusCountOutput {
  status: string;
  count: number;
}

export interface DashboardOutput {
  openOrders: number;
  readyForPickup: number;
  waitingApproval: number;
  totalOrders: number;
  plan: 'Demo' | 'Paid';
  recentOrders: ServiceOrderOutput[];
  ordersByStatus: StatusCountOutput[];
}
