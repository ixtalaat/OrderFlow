import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConfigService } from './app-config.service';
import { ManagedCustomer } from './models/managed-customer';
import { PagedList } from './models/paged-list';

@Injectable({ providedIn: 'root' })
export class CustomersService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(AppConfigService);

  list(searchTerm: string | null, pageNumber: number, pageSize: number): Observable<PagedList<ManagedCustomer>> {
    let params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    if (searchTerm) params = params.set('searchTerm', searchTerm);
    return this.http.get<PagedList<ManagedCustomer>>(`${this.config.apiUrl}/api/customers`, { params });
  }

  setTier(id: number, tier: string): Observable<void> {
    return this.http.patch<void>(`${this.config.apiUrl}/api/customers/${id}/tier`, { tier });
  }

  activate(id: number): Observable<void> {
    return this.http.patch<void>(`${this.config.apiUrl}/api/customers/${id}/activate`, {});
  }

  deactivate(id: number): Observable<void> {
    return this.http.patch<void>(`${this.config.apiUrl}/api/customers/${id}/deactivate`, {});
  }

  erase(id: number): Observable<void> {
    return this.http.delete<void>(`${this.config.apiUrl}/api/customers/${id}/erase`);
  }
}
