export interface LoginInput {
  email: string;
  password: string;
}

export interface TokenOutput {
  token: string;
  expiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
}

export interface RegisterInput {
  companyName: string;
  companyPhone: string;
  companyCnpj?: string;
  email: string;
  password: string;
}
