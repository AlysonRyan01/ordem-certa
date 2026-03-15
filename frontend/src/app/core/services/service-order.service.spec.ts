import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { ServiceOrderService } from './service-order.service';
import { ServiceOrderOutput, ServiceOrderStatus } from '../models/service-order.model';
import { environment } from '../../../environments/environment';

const BASE = `${environment.apiUrl}/api/serviceorders`;

const mockOrder: ServiceOrderOutput = {
  id: 'order-1',
  companyId: 'company-1',
  customerId: 'customer-1',
  orderNumber: 42,
  deviceType: 'Smartphone',
  brand: 'Samsung',
  model: 'A54',
  reportedDefect: 'Tela quebrada',
  status: 'Received',
  entryDate: '2026-03-01T00:00:00Z',
};

describe('ServiceOrderService', () => {
  let service: ServiceOrderService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ServiceOrderService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('getPaged() should call GET with page and pageSize params', () => {
    service.getPaged(2, 10).subscribe();

    const req = http.expectOne((r) => r.url === BASE && r.params.get('page') === '2');
    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('pageSize')).toBe('10');
    req.flush([mockOrder]);
  });

  it('getById() should call GET with correct id', () => {
    service.getById('order-1').subscribe((o) => expect(o.orderNumber).toBe(42));

    const req = http.expectOne(`${BASE}/order-1`);
    expect(req.request.method).toBe('GET');
    req.flush(mockOrder);
  });

  it('create() should call POST with body', () => {
    const input = {
      customerId: 'customer-1',
      deviceType: 'Notebook',
      brand: 'Dell',
      model: 'XPS',
      reportedDefect: 'Não liga',
    };
    service.create(input).subscribe();

    const req = http.expectOne(BASE);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(input);
    req.flush(mockOrder);
  });

  it('update() should call PUT with id and body', () => {
    const input = {
      deviceType: 'Tablet',
      brand: 'Apple',
      model: 'iPad',
      reportedDefect: 'Bateria',
    };
    service.update('order-1', input).subscribe();

    const req = http.expectOne(`${BASE}/order-1`);
    expect(req.request.method).toBe('PUT');
    req.flush(mockOrder);
  });

  it('delete() should call DELETE with id', () => {
    service.delete('order-1').subscribe();

    const req = http.expectOne(`${BASE}/order-1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('changeStatus() should call PATCH with status body', () => {
    const status: ServiceOrderStatus = 'UnderAnalysis';
    service.changeStatus('order-1', { status }).subscribe();

    const req = http.expectOne(`${BASE}/order-1/status`);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({ status });
    req.flush(mockOrder);
  });

  it('createBudget() should call POST to budget endpoint', () => {
    service.createBudget('order-1', { value: 350, description: 'Troca de tela' }).subscribe();

    const req = http.expectOne(`${BASE}/order-1/budget`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body.value).toBe(350);
    req.flush(mockOrder);
  });

  it('getByStatus() should call GET with status in URL', () => {
    service.getByStatus('ReadyForPickup', 1, 10).subscribe();

    const req = http.expectOne((r) => r.url === `${BASE}/by-status/ReadyForPickup`);
    expect(req.request.method).toBe('GET');
    req.flush([mockOrder]);
  });
});
