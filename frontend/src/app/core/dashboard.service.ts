import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConfigService } from './app-config.service';
import { DashboardStats } from './models/dashboard-stats';
import { Order } from './models/order';
import { PagedList } from './models/paged-list';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(AppConfigService);

  stats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.config.apiUrl}/api/dashboard/stats`);
  }

  orders(pageNumber: number, pageSize: number): Observable<PagedList<Order>> {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http.get<PagedList<Order>>(`${this.config.apiUrl}/api/dashboard/orders`, { params });
  }

  transition(orderId: number, action: 'confirm' | 'reject' | 'processing' | 'complete'): Observable<Order> {
    return this.http.patch<Order>(
      `${this.config.apiUrl}/api/orders/${orderId}/${action}`,
      {},
    );
  }
}
