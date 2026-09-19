import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { DashboardService } from './dashboard.service';

describe('DashboardService', () => {
  let service: DashboardService;
  let http: HttpTestingController;
  const api = 'http://localhost:8080';

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(DashboardService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
  });

  it('should fetch stats', async () => {
    const promise = service.stats().toPromise();
    const request = http.expectOne(`${api}/api/dashboard/stats`);
    expect(request.request.method).toBe('GET');
    request.flush({ totalOrders: 3 });
    const stats = await promise;
    expect(stats?.totalOrders).toBe(3);
  });

  it('should page staff orders', async () => {
    const promise = service.orders(2, 10).toPromise();
    const request = http.expectOne(
      (req) =>
        req.url === `${api}/api/dashboard/orders` &&
        req.params.get('pageNumber') === '2' &&
        req.params.get('pageSize') === '10',
    );
    request.flush({ items: [], totalCount: 0 });
    const page = await promise;
    expect(page?.totalCount).toBe(0);
  });

  it('should transition orders', async () => {
    const promise = service.transition(7, 'confirm').toPromise();
    const request = http.expectOne(`${api}/api/orders/7/confirm`);
    expect(request.request.method).toBe('PATCH');
    request.flush({ id: 7 });
    const order = await promise;
    expect(order?.id).toBe(7);
  });
});
