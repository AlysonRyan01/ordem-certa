import { ServiceOrderStatus } from './service-order.model';

export interface StatusMeta {
  label: string;
  color: string;   // Tailwind text color class
  bg: string;      // Tailwind bg color class
  icon: string;    // Material icon name
}

export const STATUS_META: Record<ServiceOrderStatus, StatusMeta> = {
  Received:        { label: 'Recebido',              color: 'text-blue-700',  bg: 'bg-blue-100',   icon: 'inbox' },
  UnderAnalysis:   { label: 'Em análise',            color: 'text-indigo-700',bg: 'bg-indigo-100', icon: 'search' },
  BudgetPending:   { label: 'Orçamento pendente',    color: 'text-orange-700',bg: 'bg-orange-100', icon: 'pending' },
  WaitingApproval: { label: 'Aguardando aprovação',  color: 'text-yellow-700',bg: 'bg-yellow-100', icon: 'schedule' },
  BudgetApproved:  { label: 'Orçamento aprovado',    color: 'text-green-700', bg: 'bg-green-100',  icon: 'thumb_up' },
  BudgetRefused:   { label: 'Orçamento recusado',    color: 'text-red-700',   bg: 'bg-red-100',    icon: 'thumb_down' },
  UnderRepair:     { label: 'Em conserto',           color: 'text-purple-700',bg: 'bg-purple-100', icon: 'build' },
  ReadyForPickup:  { label: 'Pronto para retirada',  color: 'text-teal-700',  bg: 'bg-teal-100',   icon: 'check_circle' },
  Delivered:       { label: 'Entregue',              color: 'text-gray-700',  bg: 'bg-gray-100',   icon: 'done_all' },
  Cancelled:       { label: 'Cancelado',             color: 'text-red-900',   bg: 'bg-red-50',     icon: 'cancel' },
};

export const ALL_STATUSES = Object.entries(STATUS_META).map(([value, meta]) => ({
  value: value as ServiceOrderStatus,
  label: meta.label,
}));

export function statusMeta(status: ServiceOrderStatus): StatusMeta {
  return STATUS_META[status];
}
