export interface CustomerDocumentOutput {
  value: string;
  type: 'Cpf' | 'Cnpj';
}

export interface CustomerAddressOutput {
  street?: string;
  number?: string;
  city?: string;
  state?: string;
  fullAddress?: string;
}

export interface CustomerOutput {
  id: string;
  companyId: string;
  fullName: string;
  phone: string;
  phoneFormatted: string;
  email?: string;
  document?: CustomerDocumentOutput;
  address?: CustomerAddressOutput;
}

export interface CreateCustomerInput {
  fullName: string;
  phone: string;
  email?: string;
  document?: string;
  street?: string;
  number?: string;
  city?: string;
  state?: string;
}

export interface UpdateCustomerInput {
  fullName: string;
  phone: string;
  email?: string;
  street?: string;
  number?: string;
  city?: string;
  state?: string;
}
