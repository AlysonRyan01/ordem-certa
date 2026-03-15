import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CompanyOutput, ConfirmPasswordChangeInput, UpdateCompanyInput } from '../models/company.model';

@Injectable({ providedIn: 'root' })
export class CompanyService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/company`;

  getMe(): Observable<CompanyOutput> {
    return this.http.get<CompanyOutput>(`${this.base}/me`);
  }

  updateMe(input: UpdateCompanyInput): Observable<CompanyOutput> {
    return this.http.put<CompanyOutput>(`${this.base}/me`, input);
  }

  requestPasswordChange(): Observable<void> {
    return this.http.post<void>(`${this.base}/me/request-password-change`, {});
  }

  confirmPasswordChange(input: ConfirmPasswordChangeInput): Observable<void> {
    return this.http.post<void>(`${this.base}/me/confirm-password-change`, input);
  }
}
