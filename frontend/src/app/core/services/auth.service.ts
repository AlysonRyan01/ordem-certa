import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { switchMap, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginInput, RegisterInput, TokenOutput } from '../models/auth.model';

interface JwtPayload {
  sub: string;
  email: string;
  name: string;
  companyId: string;
  exp: number;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly _user = signal<JwtPayload | null>(this.loadFromToken());

  readonly user = this._user.asReadonly();
  readonly isAuthenticated = computed(() => this._user() !== null);
  readonly userName = computed(() => this._user()?.name ?? '');
  readonly companyId = computed(() => this._user()?.companyId ?? '');

  login(input: LoginInput) {
    return this.http
      .post<TokenOutput>(`${environment.apiUrl}/api/auth/login`, input)
      .pipe(
        tap((response) => {
          localStorage.setItem('token', response.token);
          this._user.set(this.decodeToken(response.token));
        })
      );
  }

  register(input: RegisterInput) {
    return this.http
      .post(`${environment.apiUrl}/api/company`, {
        name: input.companyName,
        phone: input.companyPhone,
        cnpj: input.companyCnpj || null,
        email: input.email,
        password: input.password,
      })
      .pipe(
        switchMap(() =>
          this.login({
            email: input.email,
            password: input.password,
          })
        )
      );
  }

  requestPasswordReset(email: string) {
    return this.http.post<void>(`${environment.apiUrl}/api/auth/request-password-reset`, { email });
  }

  confirmPasswordReset(input: { email: string; code: string; newPassword: string }) {
    return this.http.post<void>(`${environment.apiUrl}/api/auth/confirm-password-reset`, input);
  }

  logout(): void {
    localStorage.removeItem('token');
    this._user.set(null);
    this.router.navigate(['/login']);
  }

  private loadFromToken(): JwtPayload | null {
    const token = localStorage.getItem('token');
    if (!token) return null;
    return this.decodeToken(token);
  }

  private decodeToken(token: string): JwtPayload | null {
    try {
      const payload = token.split('.')[1];
      return JSON.parse(atob(payload)) as JwtPayload;
    } catch {
      return null;
    }
  }
}
