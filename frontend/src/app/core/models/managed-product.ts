export interface ManagedProduct {
  id: number;
  name: string;
  description: string;
  sku: string;
  price: number;
  categoryId: number;
  categoryName: string;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc?: string | null;
}
