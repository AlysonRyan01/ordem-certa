import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { AuthService } from './auth.service';

// Minimal valid JWT with { sub, email, name, companyId, exp }
const FAKE_JWT =
  'eyJhbGciOiJIUzI1NiJ9.' +
  btoa(
    JSON.stringify({
      sub: 'user-123',
      email: 'test@test.com',
      name: 'Test User',
      companyId: 'company-456',
      exp: Math.floor(Date.now() / 1000) + 3600,
    })
  ).replace(/=/g, '') +
  '.signature';

describe('AuthService', () => {
  let service: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    service = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should start unauthenticated when no token in localStorage', () => {
    expect(service.isAuthenticated()).toBeFalse();
    expect(service.userName()).toBe('');
    expect(service.companyId()).toBe('');
  });

  it('login() should store token and update signals', () => {
    service.login({ email: 'test@test.com', password: '123456' }).subscribe();

    const req = http.expectOne((r) => r.url.includes('/api/auth/login'));
    req.flush({ token: FAKE_JWT, expiresAt: new Date().toISOString() });

    expect(localStorage.getItem('token')).toBe(FAKE_JWT);
    expect(service.isAuthenticated()).toBeTrue();
    expect(service.userName()).toBe('Test User');
    expect(service.companyId()).toBe('company-456');
  });

  it('logout() should clear token and reset signals', () => {
    localStorage.setItem('token', FAKE_JWT);
    service['_user'].set(service['decodeToken'](FAKE_JWT));

    service.logout();

    expect(localStorage.getItem('token')).toBeNull();
    expect(service.isAuthenticated()).toBeFalse();
    expect(service.userName()).toBe('');
  });

  it('should restore session from localStorage on init', () => {
    localStorage.setItem('token', FAKE_JWT);
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    const freshService = TestBed.inject(AuthService);

    expect(freshService.isAuthenticated()).toBeTrue();
    expect(freshService.userName()).toBe('Test User');
  });
});
