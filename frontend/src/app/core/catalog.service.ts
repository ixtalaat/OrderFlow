import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConfigService } from './app-config.service';
import { CatalogProduct } from './models/catalog-product';
import { PagedList } from './models/paged-list';

@Injectable({ providedIn: 'root' })
export class CatalogService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(AppConfigService);

  list(searchTerm: string | null, pageNumber: number, pageSize: number): Observable<PagedList<CatalogProduct>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    if (searchTerm) params = params.set('searchTerm', searchTerm);
    return this.http.get<PagedList<CatalogProduct>>(`${this.config.apiUrl}/api/catalog/products`, {
      params,
    });
  }

  detail(id: number): Observable<CatalogProduct> {
    return this.http.get<CatalogProduct>(`${this.config.apiUrl}/api/catalog/products/${id}`);
  }
}
