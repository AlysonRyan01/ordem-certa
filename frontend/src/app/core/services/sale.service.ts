import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AddSaleItemInput,
  CreateSaleInput,
  SaleOutput,
  SalePaymentMethod,
  SaleStatus,
  SetSaleWarrantyInput,
  UpdateSaleInput,
  UpdateSaleItemInput,
} from '../models/sale.model';

@Injectable({ providedIn: 'root' })
export class SaleService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/sales`;

  getAll(page: number, pageSize: number): Observable<SaleOutput[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<SaleOutput[]>(this.base, { params });
  }

  getById(id: string): Observable<SaleOutput> {
    return this.http.get<SaleOutput>(`${this.base}/${id}`);
  }

  getByStatus(status: SaleStatus, page: number, pageSize: number): Observable<SaleOutput[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<SaleOutput[]>(`${this.base}/by-status`, { params: params.set('status', status) });
  }

  getByCustomer(customerId: string, page: number, pageSize: number): Observable<SaleOutput[]> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<SaleOutput[]>(`${this.base}/by-customer/${customerId}`, { params });
  }

  create(input: CreateSaleInput): Observable<SaleOutput> {
    return this.http.post<SaleOutput>(this.base, input);
  }

  update(id: string, input: UpdateSaleInput): Observable<SaleOutput> {
    return this.http.put<SaleOutput>(`${this.base}/${id}`, input);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  addItem(id: string, input: AddSaleItemInput): Observable<SaleOutput> {
    return this.http.post<SaleOutput>(`${this.base}/${id}/items`, input);
  }

  updateItem(id: string, itemId: string, input: UpdateSaleItemInput): Observable<SaleOutput> {
    return this.http.put<SaleOutput>(`${this.base}/${id}/items/${itemId}`, input);
  }

  removeItem(id: string, itemId: string): Observable<SaleOutput> {
    return this.http.delete<SaleOutput>(`${this.base}/${id}/items/${itemId}`);
  }

  complete(id: string): Observable<SaleOutput> {
    return this.http.post<SaleOutput>(`${this.base}/${id}/complete`, {});
  }

  cancel(id: string): Observable<SaleOutput> {
    return this.http.post<SaleOutput>(`${this.base}/${id}/cancel`, {});
  }

  setWarranty(id: string, input: SetSaleWarrantyInput): Observable<SaleOutput> {
    return this.http.post<SaleOutput>(`${this.base}/${id}/warranty`, input);
  }

  printReceipt(id: string): Observable<Blob> {
    return this.http.get(`${this.base}/${id}/print/receipt`, { responseType: 'blob' });
  }

  printWarranty(id: string): Observable<Blob> {
    return this.http.get(`${this.base}/${id}/print/warranty`, { responseType: 'blob' });
  }
}
