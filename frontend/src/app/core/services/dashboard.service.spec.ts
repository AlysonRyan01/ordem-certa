import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { DashboardService } from './dashboard.service';
import { DashboardOutput } from '../models/dashboard.model';
import { environment } from '../../../environments/environment';

const mockDashboard: DashboardOutput = {
  openOrders: 5,
  readyForPickup: 2,
  waitingApproval: 1,
  totalOrders: 8,
  plan: 'Demo',
  recentOrders: [],
  ordersByStatus: [{ status: 'Received', count: 3 }],
};

describe('DashboardService', () => {
  let service: DashboardService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(DashboardService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('get() should call GET /api/dashboard and return data', () => {
    service.get().subscribe((d) => {
      expect(d.openOrders).toBe(5);
      expect(d.plan).toBe('Demo');
      expect(d.ordersByStatus.length).toBe(1);
    });

    const req = http.expectOne(`${environment.apiUrl}/api/dashboard`);
    expect(req.request.method).toBe('GET');
    req.flush(mockDashboard);
  });
});
