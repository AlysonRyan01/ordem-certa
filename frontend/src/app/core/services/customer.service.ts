import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateCustomerInput,
  CustomerOutput,
  UpdateCustomerInput,
} from '../models/customer.model';

@Injectable({ providedIn: 'root' })
export class CustomerService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/customers`;

  getPaged(page: number, pageSize: number): Observable<CustomerOutput[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<CustomerOutput[]>(this.base, { params });
  }

  search(term: string, page: number, pageSize: number): Observable<CustomerOutput[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<CustomerOutput[]>(`${this.base}/search`, {
      params: params.set('searchTerm', term),
    });
  }

  getById(id: string): Observable<CustomerOutput> {
    return this.http.get<CustomerOutput>(`${this.base}/${id}`);
  }

  create(input: CreateCustomerInput): Observable<CustomerOutput> {
    return this.http.post<CustomerOutput>(this.base, input);
  }

  update(id: string, input: UpdateCustomerInput): Observable<CustomerOutput> {
    return this.http.put<CustomerOutput>(`${this.base}/${id}`, input);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
