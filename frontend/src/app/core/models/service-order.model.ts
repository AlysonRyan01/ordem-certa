export type ServiceOrderStatus =
  | 'UnderAnalysis'
  | 'AwaitingApproval'
  | 'UnderRepair'
  | 'ReadyForPickup'
  | 'Delivered'
  | 'Cancelled';

export type ServiceOrderRepairStatus = 'Entered' | 'Approved' | 'Disapproved' | 'Waiting';

export type RepairResult = 'CanBeRepaired' | 'NoFix' | 'NoDefectFound';
export type WarrantyUnit = 'Days' | 'Months' | 'Years';

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
  budgetStatus?: ServiceOrderRepairStatus;
  repairResult?: RepairResult;
  warrantyDuration?: number;
  warrantyUnit?: WarrantyUnit;
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
  repairResult: RepairResult;
  warrantyDuration?: number;
  warrantyUnit?: WarrantyUnit;
}

export interface UpdateBudgetInput {
  value: number;
  description: string;
  repairResult?: RepairResult;
  warrantyDuration?: number;
  warrantyUnit?: WarrantyUnit;
}

export interface SetRepairResultInput {
  repairResult: RepairResult;
}

export interface SetWarrantyInput {
  duration: number;
  unit: WarrantyUnit;
}
