export interface CatalogProduct {
  id: number;
  name: string;
  description: string;
  sku: string;
  categoryName: string;
  currentCustomerPrice: number;
  availableQuantity: number;
}
