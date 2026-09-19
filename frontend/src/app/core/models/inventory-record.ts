export interface InventoryRecord {
  productId: number;
  quantity: number;
  reservedQuantity: number;
  availableQuantity: number;
  version: number;
}
