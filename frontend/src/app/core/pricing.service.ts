import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConfigService } from './app-config.service';
import { PricingRule } from './models/pricing-rule';

export interface PricingRuleInput {
  productId: number;
  tier: number;
  discountPercentage: number;
  validFromUtc: string;
  validToUtc?: string | null;
}

@Injectable({ providedIn: 'root' })
export class PricingService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(AppConfigService);

  create(input: PricingRuleInput): Observable<PricingRule> {
    return this.http.post<PricingRule>(`${this.config.apiUrl}/api/pricing-rules`, input);
  }

  update(id: number, input: PricingRuleInput): Observable<PricingRule> {
    return this.http.put<PricingRule>(`${this.config.apiUrl}/api/pricing-rules/${id}`, input);
  }

  remove(id: number): Observable<void> {
    return this.http.delete<void>(`${this.config.apiUrl}/api/pricing-rules/${id}`);
  }
}
