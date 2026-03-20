export interface AdminTokenOutput {
  token: string;
  expiresAt: string;
}

export interface AdminStatsOutput {
  totalCompanies: number;
  paidCompanies: number;
  demoCompanies: number;
  newLast30Days: number;
  totalServiceOrders: number;
}

export interface AdminCompanyOutput {
  id: string;
  name: string;
  email: string;
  plan: string;
  hasActiveSubscription: boolean;
  serviceOrderCount: number;
  createdAt: string;
}

export interface AdminLoginRequest {
  email: string;
  password: string;
}
