import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Message } from 'primeng/message';
import { ApiErrorHandler } from '../../core/api-error-handler';
import { OrdersService } from '../../core/orders.service';
import { Order } from '../../core/models/order';
import { orderStatusLabel } from '../../core/models/order-status';

@Component({
  selector: 'app-order-detail',
  imports: [RouterLink, Button, Card, Message],
  templateUrl: './order-detail.html',
  styleUrl: './order-detail.scss',
})
export class OrderDetail {
  private readonly route = inject(ActivatedRoute);
  private readonly orders = inject(OrdersService);
  private readonly errors = inject(ApiErrorHandler);

  protected readonly order = signal<Order | null>(null);
  protected readonly failure = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly label = orderStatusLabel;

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
