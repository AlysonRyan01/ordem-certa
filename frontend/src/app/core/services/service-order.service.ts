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
  SetRepairResultInput,
  SetWarrantyInput,
  UpdateBudgetInput,
  UpdateServiceOrderInput,
} from '../models/service-order.model';
import { ServiceOrderNotificationOutput } from '../models/service-order-notification.model';

@Injectable({ providedIn: 'root' })
export class ServiceOrderService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/service-orders`;

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

  rollback(id: string): Observable<ServiceOrderOutput> {
    return this.http.post<ServiceOrderOutput>(`${this.base}/${id}/rollback`, {});
  }

  createBudget(id: string, input: CreateBudgetInput): Observable<ServiceOrderOutput> {
    return this.http.post<ServiceOrderOutput>(`${this.base}/${id}/budget`, input);
  }

  updateBudget(id: string, input: UpdateBudgetInput): Observable<ServiceOrderOutput> {
    return this.http.put<ServiceOrderOutput>(`${this.base}/${id}/budget`, input);
  }

  getPublicById(id: string): Observable<ServiceOrderOutput> {
    return this.http.get<ServiceOrderOutput>(`${environment.apiUrl}/api/public/orders/${id}`);
  }

  approveBudget(id: string): Observable<ServiceOrderOutput> {
    return this.http.post<ServiceOrderOutput>(`${this.base}/${id}/budget/approve`, {});
  }

  refuseBudget(id: string): Observable<ServiceOrderOutput> {
    return this.http.post<ServiceOrderOutput>(`${this.base}/${id}/budget/refuse`, {});
  }

  approveBudgetFromLink(id: string): Observable<ServiceOrderOutput> {
    return this.http.get<ServiceOrderOutput>(`${environment.apiUrl}/public/orders/${id}/approve`);
  }

  refuseBudgetFromLink(id: string): Observable<ServiceOrderOutput> {
    return this.http.get<ServiceOrderOutput>(`${environment.apiUrl}/public/orders/${id}/refuse`);
  }

  setRepairResult(id: string, input: SetRepairResultInput): Observable<ServiceOrderOutput> {
    return this.http.patch<ServiceOrderOutput>(`${this.base}/${id}/repair-result`, input);
  }

  setWarranty(id: string, input: SetWarrantyInput): Observable<ServiceOrderOutput> {
    return this.http.patch<ServiceOrderOutput>(`${this.base}/${id}/warranty`, input);
  }

  notifyBudgetCreated(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/notify/budget-created`, {});
  }

  notifyBudgetApproved(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/notify/budget-approved`, {});
  }

  notifyBudgetRefused(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/notify/budget-refused`, {});
  }

  notifyReadyForPickup(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/notify/ready-for-pickup`, {});
  }

  notifyUnderAnalysis(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/notify/under-analysis`, {});
  }

  notifyUnderRepair(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/notify/under-repair`, {});
  }

  notifyDelivered(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/notify/delivered`, {});
  }

  notifyCancelled(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/notify/cancelled`, {});
  }

  getNotifications(id: string): Observable<ServiceOrderNotificationOutput[]> {
    return this.http.get<ServiceOrderNotificationOutput[]>(`${this.base}/${id}/notifications`);
  }

  print(id: string): void {
    this.http.get(`${this.base}/${id}/print`, { responseType: 'blob' }).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        window.open(url, '_blank');
        setTimeout(() => URL.revokeObjectURL(url), 10000);
      },
    });
  }
}
