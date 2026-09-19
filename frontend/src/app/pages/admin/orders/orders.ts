import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Message } from 'primeng/message';
import { Paginator, PaginatorState } from 'primeng/paginator';
import { Tag } from 'primeng/tag';
import { ApiErrorHandler } from '../../../core/api-error-handler';
import { ToastNotify } from '../../../core/toast-notify';
import { DashboardService } from '../../../core/dashboard.service';
import { Order } from '../../../core/models/order';
import { PagedList } from '../../../core/models/paged-list';
import { orderStatusLabel, orderStatusSeverity } from '../../../core/models/order-status';

@Component({
  selector: 'app-admin-orders',
  imports: [RouterLink, Button, Card, Message, Paginator, Tag],
  templateUrl: './orders.html',
  styleUrl: './orders.scss',
})
export class AdminOrders {
  private readonly dashboard = inject(DashboardService);
  private readonly errors = inject(ApiErrorHandler);
  private readonly toast = inject(ToastNotify);

  protected readonly page = signal<PagedList<Order> | null>(null);
  protected readonly failure = signal<string | null>(null);
  protected readonly pageSize = 10;
  protected readonly label = orderStatusLabel;
  protected readonly tagSeverity = orderStatusSeverity;

  constructor() {
    void this.load(1);
  }

  protected async onPage(event: PaginatorState): Promise<void> {
    await this.load((event.page ?? 0) + 1);
  }

  protected async transition(order: Order, action: 'confirm' | 'reject' | 'processing' | 'complete'): Promise<void> {
    this.failure.set(null);
    try {
      await firstValueFrom(this.dashboard.transition(order.id, action));
      this.toast.success(`Order #${order.id} ${action}.`);
      await this.load(this.page()?.pageNumber ?? 1);
    } catch (error: unknown) {
      this.failure.set(this.errors.toMessage(error));
    }
  }

  private async load(pageNumber: number): Promise<void> {
    this.failure.set(null);
    try {
      this.page.set(await firstValueFrom(this.dashboard.orders(pageNumber, this.pageSize)));
    } catch (error: unknown) {
      this.failure.set(this.errors.toMessage(error));
    }
  }
}
