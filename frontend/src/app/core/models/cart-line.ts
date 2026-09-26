export interface CartLine {
  productId: number;
  name: string;
  unitPrice: number;
  quantity: number;
  imageUrl?: string | null;
}
