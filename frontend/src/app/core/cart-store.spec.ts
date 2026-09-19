import { TestBed } from '@angular/core/testing';
import { CartStore } from './cart-store';

describe('CartStore', () => {
  let store: CartStore;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({});
    store = TestBed.inject(CartStore);
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('should start empty', () => {
    expect(store.lines()).toEqual([]);
    expect(store.count()).toBe(0);
    expect(store.subtotal()).toBe(0);
  });

  it('should add and merge lines', () => {
    store.add(1, 'Widget', 10);
    store.add(1, 'Widget', 10, 2);
    store.add(2, 'Gadget', 5);

    expect(store.lines()).toEqual([
      { productId: 1, name: 'Widget', unitPrice: 10, quantity: 3 },
      { productId: 2, name: 'Gadget', unitPrice: 5, quantity: 1 },
    ]);
    expect(store.count()).toBe(4);
    expect(store.subtotal()).toBe(35);
  });

  it('should remove line when quantity set to zero', () => {
    store.add(1, 'Widget', 10);
    store.setQuantity(1, 0);

    expect(store.lines()).toEqual([]);
  });

  it('should clear the cart', () => {
    store.add(1, 'Widget', 10);
    store.clear();

    expect(store.lines()).toEqual([]);
  });

  it('should persist lines across instances', async () => {
    store.add(1, 'Widget', 10, 2);
    await new Promise((resolve) => setTimeout(resolve, 0));

    const reloaded = TestBed.runInInjectionContext(() => new CartStore());
    expect(reloaded.lines()).toEqual([{ productId: 1, name: 'Widget', unitPrice: 10, quantity: 2 }]);
  });

  it('should produce a stable signature regardless of order', () => {
    store.add(2, 'Gadget', 5);
    store.add(1, 'Widget', 10);
    const first = store.signature();

    store.clear();
    store.add(1, 'Widget', 10);
    store.add(2, 'Gadget', 5);

    expect(store.signature()).toBe(first);
  });
});
