export type ServiceOrderStatus =
  | 'Received'
  | 'UnderAnalysis'
  | 'BudgetPending'
  | 'WaitingApproval'
  | 'BudgetApproved'
  | 'BudgetRefused'
  | 'UnderRepair'
  | 'ReadyForPickup'
  | 'Delivered'
  | 'Cancelled';

export interface ServiceOrderOutput {
  id: string;
  companyId: string;
  customerId: string;
  orderNumber: number;
  deviceType: string;
  brand: string;
  model: string;
  reportedDefect: string;
  accessories?: string;
  observations?: string;
  status: ServiceOrderStatus;
  entryDate: string;
  technicianName?: string;
  budgetValue?: number;
  budgetDescription?: string;
  companyName?: string;
}

export interface CreateServiceOrderInput {
  customerId: string;
  deviceType: string;
  brand: string;
  model: string;
  reportedDefect: string;
  accessories?: string;
  observations?: string;
  technicianName?: string;
}

export interface UpdateServiceOrderInput {
  deviceType: string;
  brand: string;
  model: string;
  reportedDefect: string;
  accessories?: string;
  observations?: string;
  technicianName?: string;
}

export interface ChangeStatusInput {
  status: ServiceOrderStatus;
}

export interface CreateBudgetInput {
  value: number;
  description: string;
}
