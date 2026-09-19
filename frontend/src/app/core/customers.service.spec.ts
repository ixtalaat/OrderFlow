import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { CustomersService } from './customers.service';

describe('CustomersService', () => {
  let service: CustomersService;
  let http: HttpTestingController;
  const api = 'http://localhost:8080';

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(CustomersService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
  });

  it('should set tiers', async () => {
    const promise = service.setTier(3, 'Wholesale').toPromise();
    const request = http.expectOne(`${api}/api/customers/3/tier`);
    expect(request.request.method).toBe('PATCH');
    expect(request.request.body).toEqual({ tier: 'Wholesale' });
    request.flush(null);
    await promise;
  });

  it('should erase customers', async () => {
    const promise = service.erase(3).toPromise();
    const request = http.expectOne(`${api}/api/customers/3/erase`);
    expect(request.request.method).toBe('DELETE');
    request.flush(null);
    await promise;
  });
});
