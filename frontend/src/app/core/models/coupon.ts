export interface Coupon {
  id: number;
  code: string;
  discountPercentage: number;
  minOrderTotal: number;
  validFromUtc: string;
  validToUtc?: string | null;
  maxRedemptions?: number | null;
  timesRedeemed: number;
  isActive: boolean;
}
