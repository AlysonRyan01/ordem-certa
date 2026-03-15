export type PlanType = 'Demo' | 'Paid';

export interface CompanyOutput {
  id: string;
  name: string;
  email: string;
  cnpj?: string;
  cnpjFormatted?: string;
  phone: string;
  phoneFormatted: string;
  street?: string;
  number?: string;
  city?: string;
  state?: string;
  plan: PlanType;
}

export interface UpdateCompanyInput {
  name: string;
  phone: string;
  street?: string;
  number?: string;
  city?: string;
  state?: string;
}

export interface ConfirmPasswordChangeInput {
  code: string;
  newPassword: string;
}
