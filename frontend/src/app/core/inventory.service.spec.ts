import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { InventoryService } from './inventory.service';

describe('InventoryService', () => {
  let service: InventoryService;
  let http: HttpTestingController;
  const api = 'http://localhost:8080';

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(InventoryService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
  });

  it('should fetch inventory', async () => {
    const promise = service.get(7).toPromise();
    http.expectOne(`${api}/api/inventory/7`).flush({ productId: 7 });
    expect((await promise)?.productId).toBe(7);
  });

  it('should post stock operations with quantity', async () => {
    const promise = service.operate(7, 'reserve', 2).toPromise();
    const request = http.expectOne(`${api}/api/inventory/7/reserve`);
    expect(request.request.body).toEqual({ quantity: 2 });
    request.flush({ productId: 7 });
    await promise;
  });

  it('should adjust with a reason', async () => {
    const promise = service.adjust(7, -1, 'Cycle count').toPromise();
    const request = http.expectOne(`${api}/api/inventory/7/adjust`);
    expect(request.request.method).toBe('PATCH');
    expect(request.request.body).toEqual({ quantity: -1, reason: 'Cycle count' });
    request.flush({ productId: 7 });
    await promise;
  });
});
