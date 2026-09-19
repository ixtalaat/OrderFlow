import { Injectable, computed, effect, signal } from '@angular/core';
import { CartLine } from './models/cart-line';

const STORAGE_KEY = 'orderflow_cart';

function load(): CartLine[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return [];
    const parsed = JSON.parse(raw) as CartLine[];
    return Array.isArray(parsed) ? parsed.filter((line) => line.quantity > 0) : [];
  } catch {
    return [];
  }
}

@Injectable({ providedIn: 'root' })
export class CartStore {
  readonly lines = signal<CartLine[]>(load());
  readonly count = computed(() => this.lines().reduce((sum, line) => sum + line.quantity, 0));
  readonly subtotal = computed(() =>
    this.lines().reduce((sum, line) => sum + line.quantity * line.unitPrice, 0),
  );

  constructor() {
    effect(() => {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(this.lines()));
    });
  }

  add(productId: number, name: string, unitPrice: number, quantity = 1): void {
    this.lines.update((lines) => {
      const existing = lines.find((line) => line.productId === productId);
      if (existing) {
        return lines.map((line) =>
          line.productId === productId ? { ...line, quantity: line.quantity + quantity } : line,
        );
      }
      return [...lines, { productId, name, unitPrice, quantity }];
    });
  }

  setQuantity(productId: number, quantity: number): void {
    this.lines.update((lines) =>
      quantity <= 0
        ? lines.filter((line) => line.productId !== productId)
        : lines.map((line) => (line.productId === productId ? { ...line, quantity } : line)),
    );
  }

  remove(productId: number): void {
    this.setQuantity(productId, 0);
  }

  clear(): void {
    this.lines.set([]);
  }

  signature(): string {
    return JSON.stringify(
      this.lines()
        .map((line) => [line.productId, line.quantity])
        .sort((a, b) => a[0] - b[0]),
    );
  }
}
