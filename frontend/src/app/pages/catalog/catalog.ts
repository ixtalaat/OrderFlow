import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Paginator, PaginatorState } from 'primeng/paginator';
import { Skeleton } from 'primeng/skeleton';
import { CatalogService } from '../../core/catalog.service';
import { ApiErrorHandler } from '../../core/api-error-handler';
import { CartStore } from '../../core/cart-store';
import { CatalogProduct } from '../../core/models/catalog-product';
import { PagedList } from '../../core/models/paged-list';

@Component({
  selector: 'app-catalog',
  imports: [FormsModule, RouterLink, Button, Card, InputText, Message, Paginator, Skeleton],
  templateUrl: './catalog.html',
  styleUrl: './catalog.scss',
})
export class Catalog {
  private readonly catalog = inject(CatalogService);
  private readonly errors = inject(ApiErrorHandler);
  protected readonly cart = inject(CartStore);

  protected readonly search = signal('');
  protected readonly page = signal<PagedList<CatalogProduct> | null>(null);
  protected readonly failure = signal<string | null>(null);
  protected readonly loading = signal(false);
  protected readonly pageSize = 12;
  private loadSeq = 0;

  constructor() {
    void this.load(1);
  }

  protected async searchNow(): Promise<void> {
    await this.load(1);
  }

  protected async onPage(event: PaginatorState): Promise<void> {
    await this.load((event.page ?? 0) + 1);
  }

  protected add(product: CatalogProduct): void {
    this.cart.add(product.id, product.name, product.currentCustomerPrice);
  }

  private async load(pageNumber: number): Promise<void> {
    const sequence = ++this.loadSeq;
    this.loading.set(true);
    this.failure.set(null);
    try {
      const term = this.search().trim() || null;
      const result = await firstValueFrom(this.catalog.list(term, pageNumber, this.pageSize));
      if (sequence === this.loadSeq) this.page.set(result);
    } catch (error: unknown) {
      if (sequence === this.loadSeq) this.failure.set(this.errors.toMessage(error));
    } finally {
      if (sequence === this.loadSeq) this.loading.set(false);
    }
  }
}
