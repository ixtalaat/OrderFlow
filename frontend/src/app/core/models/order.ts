import { OrderItem } from './order-item';

export interface Order {
  id: number;
  customerId: number;
  status: number;
  totalAmount: number;
  createdAtUtc: string;
  items: OrderItem[];
  couponCode?: string | null;
  discountAmount?: number;
}
