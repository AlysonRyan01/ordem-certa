import { SaleStatus } from '../models/sale.model';

export interface SaleStatusMeta {
  label: string;
  color: string;
  bgColor: string;
}

export const SALE_STATUS_META: Record<SaleStatus, SaleStatusMeta> = {
  Pending:   { label: 'Pendente',  color: '#b45309', bgColor: '#fef3c7' },
  Completed: { label: 'Concluída', color: '#065f46', bgColor: '#d1fae5' },
  Cancelled: { label: 'Cancelada', color: '#991b1b', bgColor: '#fee2e2' },
};

export const ALL_SALE_STATUSES: { value: SaleStatus; label: string }[] = [
  { value: 'Pending',   label: 'Pendente' },
  { value: 'Completed', label: 'Concluída' },
  { value: 'Cancelled', label: 'Cancelada' },
];

export function getSaleStatusMeta(status: SaleStatus): SaleStatusMeta {
  return SALE_STATUS_META[status] ?? { label: status, color: '#374151', bgColor: '#f3f4f6' };
}
