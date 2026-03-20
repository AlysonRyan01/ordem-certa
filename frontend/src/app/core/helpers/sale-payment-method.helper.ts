import { SalePaymentMethod } from '../models/sale.model';

export const PAYMENT_METHOD_LABELS: Record<SalePaymentMethod, string> = {
  Cash:         'Dinheiro',
  CreditCard:   'Cartão de Crédito',
  DebitCard:    'Cartão de Débito',
  Pix:          'Pix',
  BankTransfer: 'Transferência Bancária',
  Check:        'Cheque',
  Other:        'Outro',
};

export const ALL_PAYMENT_METHODS: { value: SalePaymentMethod; label: string }[] = [
  { value: 'Cash',         label: 'Dinheiro' },
  { value: 'CreditCard',   label: 'Cartão de Crédito' },
  { value: 'DebitCard',    label: 'Cartão de Débito' },
  { value: 'Pix',          label: 'Pix' },
  { value: 'BankTransfer', label: 'Transferência Bancária' },
  { value: 'Check',        label: 'Cheque' },
  { value: 'Other',        label: 'Outro' },
];

export function getPaymentMethodLabel(method: SalePaymentMethod): string {
  return PAYMENT_METHOD_LABELS[method] ?? method;
}
