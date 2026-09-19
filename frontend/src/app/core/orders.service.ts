import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConfigService } from './app-config.service';
import { Order } from './models/order';
import { PagedList } from './models/paged-list';
import { CreateOrderItem } from './models/create-order-item';

@Injectable({ providedIn: 'root' })
export class OrdersService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(AppConfigService);

  create(
    items: CreateOrderItem[],
    options: { couponCode?: string | null; idempotencyKey?: string } = {},
  ): Observable<Order> {
    return this.http.post<Order>(`${this.config.apiUrl}/api/orders`, {
      items,
      couponCode: options.couponCode ?? null,
    },
    {
      headers: options.idempotencyKey ? { 'Idempotency-Key': options.idempotencyKey } : {},
    });
  }

  history(pageNumber: number, pageSize: number): Observable<PagedList<Order>> {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http.get<PagedList<Order>>(`${this.config.apiUrl}/api/orders`, { params });
  }

  detail(id: number): Observable<Order> {
    return this.http.get<Order>(`${this.config.apiUrl}/api/orders/${id}`);
  }

  cancel(id: number): Observable<Order> {
    return this.http.patch<Order>(`${this.config.apiUrl}/api/orders/${id}/cancel`, {});
  }
}
