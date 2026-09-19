import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Paginator, PaginatorState } from 'primeng/paginator';
import { TableModule } from 'primeng/table';
import { ApiErrorHandler } from '../../../core/api-error-handler';
import { CustomersService } from '../../../core/customers.service';
import { ManagedCustomer } from '../../../core/models/managed-customer';
import { PagedList } from '../../../core/models/paged-list';

@Component({
  selector: 'app-admin-customers',
  imports: [FormsModule, Button, InputText, Message, Paginator, TableModule],
  templateUrl: './customers.html',
  styleUrl: './customers.scss',
})
export class AdminCustomers {
  private readonly customers = inject(CustomersService);
  private readonly errors = inject(ApiErrorHandler);

  protected readonly search = signal('');
  protected readonly page = signal<PagedList<ManagedCustomer> | null>(null);
  protected readonly failure = signal<string | null>(null);
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

  protected async setTier(customer: ManagedCustomer, tier: string): Promise<void> {
    await this.act(() => this.customers.setTier(customer.id, tier));
  }

  protected async toggleActive(customer: ManagedCustomer): Promise<void> {
    await this.act(() =>
      customer.isActive
        ? this.customers.deactivate(customer.id)
        : this.customers.activate(customer.id),
    );
  }

  protected async erase(customer: ManagedCustomer): Promise<void> {
    if (!confirm(`Erase all personal data of ${customer.email}?`)) return;
    await this.act(() => this.customers.erase(customer.id));
  }

  private async act(action: () => ReturnType<CustomersService['setTier']>): Promise<void> {
    this.failure.set(null);
    try {
      await firstValueFrom(action());
      await this.load(this.page()?.pageNumber ?? 1);
    } catch (error: unknown) {
      this.failure.set(this.errors.toMessage(error));
    }
  }

  private async load(pageNumber: number): Promise<void> {
    this.failure.set(null);
    try {
      const term = this.search().trim() || null;
      this.page.set(await firstValueFrom(this.customers.list(term, pageNumber, this.pageSize)));
    } catch (error: unknown) {
      this.failure.set(this.errors.toMessage(error));
    }
  }
}
