import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Dialog } from 'primeng/dialog';
import { InputNumber } from 'primeng/inputnumber';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Paginator, PaginatorState } from 'primeng/paginator';
import { TableModule } from 'primeng/table';
import { ApiErrorHandler } from '../../../core/api-error-handler';
import { ToastNotify } from '../../../core/toast-notify';
import { ManagedProduct } from '../../../core/models/managed-product';
import { PagedList } from '../../../core/models/paged-list';
import { ProductInput, ProductsAdminService } from '../../../core/products-admin.service';

@Component({
  selector: 'app-admin-products',
  imports: [FormsModule, Button, Card, Dialog, InputNumber, InputText, Message, Paginator, TableModule],
  templateUrl: './products.html',
  styleUrl: './products.scss',
})
export class AdminProducts {
  private readonly products = inject(ProductsAdminService);
  private readonly errors = inject(ApiErrorHandler);
  private readonly toast = inject(ToastNotify);

  protected readonly search = signal('');
  protected readonly page = signal<PagedList<ManagedProduct> | null>(null);
  protected readonly failure = signal<string | null>(null);
  protected readonly dialogOpen = signal(false);
  protected readonly thresholdOpen = signal(false);
  protected readonly editingId = signal<number | null>(null);
  protected readonly form: ProductInput = {
    name: '',
    description: '',
    sku: '',
    price: 0,
    categoryName: '',
  };
  protected readonly threshold: { value: number | null } = { value: null };
  protected readonly pageSize = 10;

  constructor() {
    void this.load(1);
  }

  protected async searchNow(): Promise<void> {
    await this.load(1);
  }

  protected async onPage(event: PaginatorState): Promise<void> {
    await this.load((event.page ?? 0) + 1);
  }

  protected openCreate(): void {
    this.editingId.set(null);
    this.form.name = '';
    this.form.description = '';
    this.form.sku = '';
    this.form.price = 0;
    this.form.categoryName = '';
    this.dialogOpen.set(true);
  }

  protected openEdit(product: ManagedProduct): void {
    this.editingId.set(product.id);
    this.form.name = product.name;
    this.form.description = product.description;
    this.form.sku = product.sku;
    this.form.price = product.price;
    this.form.categoryName = product.categoryName;
    this.dialogOpen.set(true);
  }

  protected openThreshold(product: ManagedProduct): void {
    this.editingId.set(product.id);
    this.threshold.value = null;
    this.thresholdOpen.set(true);
  }

  protected async save(): Promise<void> {
    this.failure.set(null);
    try {
      const id = this.editingId();
      if (id !== null) {
        await firstValueFrom(this.products.update(id, this.form));
      } else {
        await firstValueFrom(this.products.create(this.form));
      }
      this.dialogOpen.set(false);
      this.toast.success(id !== null ? 'Product updated.' : 'Product created.');
      await this.load(this.page()?.pageNumber ?? 1);
    } catch (error: unknown) {
      this.failure.set(this.errors.toMessage(error));
    }
  }

  protected async saveThreshold(): Promise<void> {
    const id = this.editingId();
    if (id === null) return;
    this.failure.set(null);
    try {
      await firstValueFrom(this.products.setThreshold(id, this.threshold.value));
      this.thresholdOpen.set(false);
      this.toast.success('Threshold updated.');
    } catch (error: unknown) {
      this.failure.set(this.errors.toMessage(error));
    }
  }

  protected async toggleActive(product: ManagedProduct): Promise<void> {
    this.failure.set(null);
    try {
      if (product.isActive) {
        await firstValueFrom(this.products.deactivate(product.id));
      } else {
        await firstValueFrom(this.products.activate(product.id));
      }
      this.toast.success(product.isActive ? 'Product deactivated.' : 'Product activated.');
      await this.load(this.page()?.pageNumber ?? 1);
    } catch (error: unknown) {
      this.failure.set(this.errors.toMessage(error));
    }
  }

  private async load(pageNumber: number): Promise<void> {
    this.failure.set(null);
    try {
      const term = this.search().trim() || null;
      this.page.set(await firstValueFrom(this.products.list(term, pageNumber, this.pageSize)));
    } catch (error: unknown) {
      this.failure.set(this.errors.toMessage(error));
    }
  }
}
