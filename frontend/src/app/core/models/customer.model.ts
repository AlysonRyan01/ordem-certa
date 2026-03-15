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
  street: string;
  number: string;
  complement?: string;
  neighborhood: string;
  city: string;
  state: string;
  zipCode: string;
}

export interface CustomerOutput {
  id: string;
  companyId: string;
  name: string;
  email?: string;
  document?: CustomerDocumentOutput;
  address?: CustomerAddressOutput;
  phones: CustomerPhoneOutput[];
}

export interface CreateCustomerInput {
  name: string;
  email?: string;
  document?: string;
  street?: string;
  addressNumber?: string;
  complement?: string;
  neighborhood?: string;
  city?: string;
  state?: string;
  zipCode?: string;
}

export interface UpdateCustomerInput {
  name: string;
  email?: string;
  street?: string;
  addressNumber?: string;
  complement?: string;
  neighborhood?: string;
  city?: string;
  state?: string;
  zipCode?: string;
}

export interface AddPhoneInput {
  phone: string;
}

export interface RemovePhoneInput {
  phone: string;
}
