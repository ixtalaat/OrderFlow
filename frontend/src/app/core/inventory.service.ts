import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConfigService } from './app-config.service';
import { InventoryRecord } from './models/inventory-record';

@Injectable({ providedIn: 'root' })
export class InventoryService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(AppConfigService);

  get(productId: number): Observable<InventoryRecord> {
    return this.http.get<InventoryRecord>(`${this.config.apiUrl}/api/inventory/${productId}`);
  }

  operate(productId: number, action: 'add-stock' | 'reserve' | 'release' | 'confirm', quantity: number): Observable<InventoryRecord> {
    return this.http.post<InventoryRecord>(
      `${this.config.apiUrl}/api/inventory/${productId}/${action}`,
      { quantity },
    );
  }

  adjust(productId: number, quantity: number, reason: string): Observable<InventoryRecord> {
    return this.http.patch<InventoryRecord>(`${this.config.apiUrl}/api/inventory/${productId}/adjust`, {
      quantity,
      reason,
    });
  }
}
