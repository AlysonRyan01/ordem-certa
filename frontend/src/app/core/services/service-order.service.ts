import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ChangeStatusInput,
  CreateBudgetInput,
  CreateServiceOrderInput,
  ServiceOrderOutput,
  ServiceOrderStatus,
  UpdateServiceOrderInput,
} from '../models/service-order.model';

@Injectable({ providedIn: 'root' })
export class ServiceOrderService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/serviceorders`;

  getPaged(page: number, pageSize: number): Observable<ServiceOrderOutput[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<ServiceOrderOutput[]>(this.base, { params });
  }

  getByStatus(
    status: ServiceOrderStatus,
    page: number,
    pageSize: number
  ): Observable<ServiceOrderOutput[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<ServiceOrderOutput[]>(`${this.base}/by-status/${status}`, { params });
  }

  getByCustomer(
    customerId: string,
    page: number,
    pageSize: number
  ): Observable<ServiceOrderOutput[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<ServiceOrderOutput[]>(`${this.base}/by-customer/${customerId}`, {
      params,
    });
  }

  getById(id: string): Observable<ServiceOrderOutput> {
    return this.http.get<ServiceOrderOutput>(`${this.base}/${id}`);
  }

  create(input: CreateServiceOrderInput): Observable<ServiceOrderOutput> {
    return this.http.post<ServiceOrderOutput>(this.base, input);
  }

  update(id: string, input: UpdateServiceOrderInput): Observable<ServiceOrderOutput> {
    return this.http.put<ServiceOrderOutput>(`${this.base}/${id}`, input);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  changeStatus(id: string, input: ChangeStatusInput): Observable<ServiceOrderOutput> {
    return this.http.patch<ServiceOrderOutput>(`${this.base}/${id}/status`, input);
  }

  createBudget(id: string, input: CreateBudgetInput): Observable<ServiceOrderOutput> {
    return this.http.post<ServiceOrderOutput>(`${this.base}/${id}/budget`, input);
  }

  approveBudget(id: string): Observable<ServiceOrderOutput> {
    return this.http.get<ServiceOrderOutput>(`${environment.apiUrl}/public/orders/${id}/approve`);
  }

  refuseBudget(id: string): Observable<ServiceOrderOutput> {
    return this.http.get<ServiceOrderOutput>(`${environment.apiUrl}/public/orders/${id}/refuse`);
  }
}
