import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthStore } from './auth-store';
import { authInterceptor } from './auth-interceptor';

describe('authInterceptor', () => {
  let http: HttpClient;
  let testing: HttpTestingController;
  let store: AuthStore;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    });
    http = TestBed.inject(HttpClient);
    testing = TestBed.inject(HttpTestingController);
    store = TestBed.inject(AuthStore);
  });

  afterEach(() => {
    testing.verify();
    localStorage.clear();
  });

  it('should attach bearer and correlation headers', async () => {
    store.accessToken.set('access-1');

    const promise = firstValueFrom(http.get('/api/catalog/products'));
    const request = testing.expectOne('/api/catalog/products');
    expect(request.request.headers.get('Authorization')).toBe('Bearer access-1');
    expect(request.request.headers.get('X-Correlation-ID')).toBeTruthy();
    request.flush([]);
    await promise;
  });

  it('should not attach bearer on anonymous paths', async () => {
    store.accessToken.set('access-1');

    const promise = firstValueFrom(http.post('/api/auth/login', {}));
    const request = testing.expectOne('/api/auth/login');
    expect(request.request.headers.has('Authorization')).toBeFalse();
    request.flush({});
    await promise;
  });

  it('should refresh once and retry on 401', async () => {
    store.accessToken.set('expired');
    localStorage.setItem('orderflow_refresh_token', 'refresh-old');

    const promise = firstValueFrom(http.get('/api/catalog/products'));
    testing.expectOne('/api/catalog/products').flush(null, { status: 401, statusText: 'Unauthorized' });

    const refresh = testing.expectOne('http://localhost:8080/api/auth/refresh');
    refresh.flush({
      accessToken: 'fresh',
      expiresAtUtc: new Date().toISOString(),
      refreshToken: 'refresh-new',
      refreshExpiresAtUtc: new Date().toISOString(),
    });
    await new Promise((resolve) => setTimeout(resolve, 0));

    const retry = testing.expectOne('/api/catalog/products');
    expect(retry.request.headers.get('Authorization')).toBe('Bearer fresh');
    retry.flush([]);
    await promise;

    expect(store.accessToken()).toBe('fresh');
  });
});
