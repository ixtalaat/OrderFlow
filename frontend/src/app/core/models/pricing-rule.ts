export interface PricingRule {
  id: number;
  productId: number;
  tier: number;
  discountPercentage: number;
  validFromUtc: string;
  validToUtc?: string | null;
}
