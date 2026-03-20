export type SaleStatus = 'Pending' | 'Completed' | 'Cancelled';
export type SalePaymentMethod = 'Cash' | 'CreditCard' | 'DebitCard' | 'Pix' | 'BankTransfer' | 'Check' | 'Other';

export interface SaleItemOutput {
  id: string;
  description: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface SaleOutput {
  id: string;
  companyId: string;
  customerId?: string;
  saleNumber: number;
  customerName?: string;
  description?: string;
  status: SaleStatus;
  paymentMethod: SalePaymentMethod;
  totalValue: number;
  warrantyDuration?: number;
  warrantyUnit?: string;
  warrantyFormatted?: string;
  notes?: string;
  saleDate: string;
  createdAt: string;
  updatedAt?: string;
  items: SaleItemOutput[];
}

export interface SaleItemInput {
  description: string;
  quantity: number;
  unitPrice: number;
}

export interface CreateSaleInput {
  customerId?: string;
  customerName?: string;
  description?: string;
  paymentMethod: SalePaymentMethod;
  notes?: string;
  items: SaleItemInput[];
}

export interface UpdateSaleInput {
  customerId?: string;
  customerName?: string;
  description?: string;
  paymentMethod: SalePaymentMethod;
  notes?: string;
}

export interface AddSaleItemInput {
  description: string;
  quantity: number;
  unitPrice: number;
}

export interface UpdateSaleItemInput {
  description: string;
  quantity: number;
  unitPrice: number;
}

export interface SetSaleWarrantyInput {
  duration: number;
  unit: 'Days' | 'Months' | 'Years';
}
