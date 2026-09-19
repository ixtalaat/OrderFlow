import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Message } from 'primeng/message';
import { CatalogService } from '../../core/catalog.service';
import { ApiErrorHandler } from '../../core/api-error-handler';
import { CartStore } from '../../core/cart-store';
import { CatalogProduct } from '../../core/models/catalog-product';

@Component({
  selector: 'app-product-detail',
  imports: [RouterLink, Button, Card, Message],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.scss',
})
export class ProductDetail {
  private readonly route = inject(ActivatedRoute);
  private readonly catalog = inject(CatalogService);
  private readonly errors = inject(ApiErrorHandler);
  private readonly cart = inject(CartStore);

  protected readonly product = signal<CatalogProduct | null>(null);
  protected readonly failure = signal<string | null>(null);

  constructor() {
    void this.load();
  }

  protected add(): void {
    const product = this.product();
    if (product) this.cart.add(product.id, product.name, product.currentCustomerPrice);
  }

  private async load(): Promise<void> {
    try {
      const id = Number(this.route.snapshot.paramMap.get('id'));
      this.product.set(await firstValueFrom(this.catalog.detail(id)));
    } catch (error: unknown) {
      this.failure.set(this.errors.toMessage(error));
    }
  }
}
