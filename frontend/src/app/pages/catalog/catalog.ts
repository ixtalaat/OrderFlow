import { Component, computed, inject, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Paginator, PaginatorState } from 'primeng/paginator';
import { Tag } from 'primeng/tag';
import { Skeleton } from 'primeng/skeleton';
import { ProductArt } from '../../shared/product-art/product-art';
import { ToastNotify } from '../../core/toast-notify';
import { CatalogService } from '../../core/catalog.service';
import { ApiErrorHandler } from '../../core/api-error-handler';
import { LangService } from '../../core/lang.service';
import { CartStore } from '../../core/cart-store';
import { CatalogProduct } from '../../core/models/catalog-product';
import { PagedList } from '../../core/models/paged-list';

@Component({
  selector: 'app-catalog',
  imports: [CurrencyPipe, FormsModule, RouterLink, Button, Card, InputText, Message, Paginator, ProductArt, Skeleton, Tag],
  templateUrl: './catalog.html',
  styleUrl: './catalog.scss',
})
export class Catalog {
  private readonly catalog = inject(CatalogService);
  private readonly errors = inject(ApiErrorHandler);
  private readonly toast = inject(ToastNotify);
  protected readonly cart = inject(CartStore);
  protected readonly lang = inject(LangService);

  protected readonly search = signal('');
  protected readonly category = signal<string | null>(null);
  protected readonly sort = signal<'name' | 'price-asc' | 'price-desc'>('name');
  protected readonly inStockOnly = signal(false);
  protected readonly page = signal<PagedList<CatalogProduct> | null>(null);
  protected readonly failure = signal<string | null>(null);
  protected readonly loading = signal(false);
  protected readonly pageSize = 12;
  protected readonly skeletonPlaceholders = [1, 2, 3, 4, 5, 6, 7, 8];
  private loadSeq = 0;
  private debounceTimer: ReturnType<typeof setTimeout> | null = null;

  protected readonly categories = computed(() => {
    const names = new Set<string>();
    for (const item of this.page()?.items ?? []) names.add(item.categoryName);
    return [...names].sort();
  });

  protected readonly visibleItems = computed(() => {
    let items = [...(this.page()?.items ?? [])];
    const active = this.category();
    if (active) items = items.filter((item) => item.categoryName === active);
    if (this.inStockOnly()) items = items.filter((item) => item.availableQuantity > 0);
    switch (this.sort()) {
      case 'price-asc':
        items.sort((a, b) => a.currentCustomerPrice - b.currentCustomerPrice);
        break;
      case 'price-desc':
        items.sort((a, b) => b.currentCustomerPrice - a.currentCustomerPrice);
        break;
      default:
        items.sort((a, b) => a.name.localeCompare(b.name));
        break;
    }
    return items;
  });

  constructor() {
    void this.load(1);
  }

  protected onSearchInput(value: string): void {
    this.search.set(value);
    if (this.debounceTimer) clearTimeout(this.debounceTimer);
    this.debounceTimer = setTimeout(() => void this.load(1), 300);
  }

  protected async searchNow(): Promise<void> {
    if (this.debounceTimer) {
      clearTimeout(this.debounceTimer);
      this.debounceTimer = null;
    }
    await this.load(1);
  }

  protected async onPage(event: PaginatorState): Promise<void> {
    await this.load((event.page ?? 0) + 1);
  }

  protected selectCategory(name: string | null): void {
    this.category.set(name);
  }

  protected async clearAll(): Promise<void> {
    this.search.set('');
    this.category.set(null);
    this.sort.set('name');
    this.inStockOnly.set(false);
    await this.load(1);
  }

  protected add(product: CatalogProduct): void {
    this.cart.add(product.id, product.name, product.currentCustomerPrice, 1, product.imageUrl ?? null);
    this.toast.cartAdded(product.name);
  }

  protected stockSeverity(available: number): 'success' | 'warn' | 'danger' {
    if (available <= 0) return 'danger';
    if (available <= 3) return 'warn';
    return 'success';
  }

  protected stockLabel(available: number): string {
    if (available <= 0) return this.lang.t('catalog.outOfStock');
    return this.lang.t('catalog.availableN').replace('{n}', String(available));
  }

  protected resultCount(total: number): string {
    return this.lang.t('catalog.count').replace('{n}', String(total));
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
