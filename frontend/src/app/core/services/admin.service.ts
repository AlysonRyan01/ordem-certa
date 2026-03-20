import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AdminTokenOutput, AdminStatsOutput, AdminCompanyOutput, AdminLoginRequest } from '../models/admin.model';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/admin`;

  login(request: AdminLoginRequest): Observable<AdminTokenOutput> {
    return this.http.post<AdminTokenOutput>(`${this.base}/login`, request).pipe(
      tap((response) => localStorage.setItem('admin_token', response.token))
    );
  }

  logout(): void {
    localStorage.removeItem('admin_token');
  }

  isAdminAuthenticated(): boolean {
    return !!localStorage.getItem('admin_token');
  }

  getStats(): Observable<AdminStatsOutput> {
    return this.http.get<AdminStatsOutput>(`${this.base}/stats`);
  }

  getCompanies(page: number, pageSize: number): Observable<AdminCompanyOutput[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<AdminCompanyOutput[]>(`${this.base}/companies`, { params });
  }
}
