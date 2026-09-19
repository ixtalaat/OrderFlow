import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConfigService } from './app-config.service';
import { Coupon } from './models/coupon';

export interface CouponInput {
  code: string;
  discountPercentage: number;
  minOrderTotal: number;
  validFromUtc: string;
  validToUtc?: string | null;
  maxRedemptions?: number | null;
}

@Injectable({ providedIn: 'root' })
export class CouponsService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(AppConfigService);

  create(input: CouponInput): Observable<Coupon> {
    return this.http.post<Coupon>(`${this.config.apiUrl}/api/coupons`, input);
  }

  update(id: number, input: CouponInput): Observable<Coupon> {
    return this.http.put<Coupon>(`${this.config.apiUrl}/api/coupons/${id}`, input);
  }

  remove(id: number): Observable<void> {
    return this.http.delete<void>(`${this.config.apiUrl}/api/coupons/${id}`);
  }
}
