import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConfigService } from './app-config.service';
import { ManagedProduct } from './models/managed-product';
import { PagedList } from './models/paged-list';

export interface ProductInput {
  name: string;
  description: string;
  sku: string;
  price: number;
  categoryName: string;
}

@Injectable({ providedIn: 'root' })
export class ProductsAdminService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(AppConfigService);

  list(searchTerm: string | null, pageNumber: number, pageSize: number): Observable<PagedList<ManagedProduct>> {
    let params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    if (searchTerm) params = params.set('searchTerm', searchTerm);
    return this.http.get<PagedList<ManagedProduct>>(`${this.config.apiUrl}/api/products`, { params });
  }

  create(input: ProductInput): Observable<ManagedProduct> {
    return this.http.post<ManagedProduct>(`${this.config.apiUrl}/api/products`, input);
  }

  update(id: number, input: ProductInput): Observable<ManagedProduct> {
    return this.http.put<ManagedProduct>(`${this.config.apiUrl}/api/products/${id}`, input);
  }

  activate(id: number): Observable<void> {
    return this.http.patch<void>(`${this.config.apiUrl}/api/products/${id}/activate`, {});
  }

  deactivate(id: number): Observable<void> {
    return this.http.patch<void>(`${this.config.apiUrl}/api/products/${id}/deactivate`, {});
  }

  setThreshold(id: number, threshold: number | null): Observable<void> {
    return this.http.patch<void>(`${this.config.apiUrl}/api/products/${id}/low-stock-threshold`, {
      threshold,
    });
  }
}
