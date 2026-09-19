import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ProductsAdminService } from './products-admin.service';

describe('ProductsAdminService', () => {
  let service: ProductsAdminService;
  let http: HttpTestingController;
  const api = 'http://localhost:8080';

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ProductsAdminService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
  });

  it('should create products', async () => {
    const promise = service
      .create({ name: 'Widget', description: 'Desc', sku: 'WDG-1', price: 10, categoryName: 'Cat' })
      .toPromise();
    const request = http.expectOne(`${api}/api/products`);
    expect(request.request.method).toBe('POST');
    request.flush({ id: 1 });
    expect((await promise)?.id).toBe(1);
  });

  it('should set thresholds', async () => {
    const promise = service.setThreshold(5, 3).toPromise();
    const request = http.expectOne(`${api}/api/products/5/low-stock-threshold`);
    expect(request.request.method).toBe('PATCH');
    expect(request.request.body).toEqual({ threshold: 3 });
    request.flush(null);
    await promise;
  });

  it('should toggle activation', async () => {
    const deactivated = service.deactivate(5).toPromise();
    http.expectOne(`${api}/api/products/5/deactivate`).flush(null);
    await deactivated;

    const activated = service.activate(5).toPromise();
    http.expectOne(`${api}/api/products/5/activate`).flush(null);
    await activated;
  });
});
