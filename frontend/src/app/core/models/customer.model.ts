export interface CustomerPhoneOutput {
  value: string;
  areaCode: string;
  number: string;
}

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
  email?: string;
  document?: CustomerDocumentOutput;
  address?: CustomerAddressOutput;
  phones: CustomerPhoneOutput[];
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
  email?: string;
  street?: string;
  number?: string;
  city?: string;
  state?: string;
}

export interface AddPhoneInput {
  phone: string;
}

export interface RemovePhoneInput {
  phone: string;
}
