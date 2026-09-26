import { Component, computed, inject, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { LangService } from '../../core/lang.service';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Message } from 'primeng/message';
import { Tag } from 'primeng/tag';
import { ApiErrorHandler } from '../../core/api-error-handler';
import { OrdersService } from '../../core/orders.service';
import { ToastNotify } from '../../core/toast-notify';
import { Order } from '../../core/models/order';
import { orderStatusLabel, orderStatusSeverity } from '../../core/models/order-status';

@Component({
  selector: 'app-order-detail',
  imports: [CurrencyPipe, RouterLink, Button, Card, Message, Tag],
  templateUrl: './order-detail.html',
  styleUrl: './order-detail.scss',
})
export class OrderDetail {
  private readonly route = inject(ActivatedRoute);
  private readonly orders = inject(OrdersService);
  private readonly errors = inject(ApiErrorHandler);
  private readonly toast = inject(ToastNotify);

  protected readonly order = signal<Order | null>(null);
  protected readonly failure = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly label = orderStatusLabel;
  protected readonly tagSeverity = orderStatusSeverity;
  protected readonly lang = inject(LangService);

  protected readonly timeline = computed(() => {
    const status = this.order()?.status;
    if (status === undefined || status === 5 || status === 6) return [];
    return [1, 2, 3, 4].map((step) => ({
      status: step,
      label: orderStatusLabel(step),
      state: step < status ? 'done' : step === status ? 'current' : 'todo',
    }));
  });

  protected readonly terminated = computed(() => {
    const status = this.order()?.status;
    return status === 5 || status === 6 ? orderStatusLabel(status) : null;
  });

  constructor() {
    void this.load();
  }

  protected canCancel(): boolean {
    return this.order()?.status === 1;
  }

  protected async cancel(): Promise<void> {
    const current = this.order();
    if (!current || this.busy()) return;
    this.failure.set(null);
    this.busy.set(true);
    try {
      this.order.set(await firstValueFrom(this.orders.cancel(current.id)));
      this.toast.success(`Order #${current.id} cancelled.`);
    } catch (error: unknown) {
      this.failure.set(this.errors.toMessage(error));
    } finally {
      this.busy.set(false);
    }
  }

  private async load(): Promise<void> {
    try {
      const id = Number(this.route.snapshot.paramMap.get('id'));
      this.order.set(await firstValueFrom(this.orders.detail(id)));
    } catch (error: unknown) {
      this.failure.set(this.errors.toMessage(error));
    }
  }
}
