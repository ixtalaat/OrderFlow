import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Message } from 'primeng/message';
import { Paginator, PaginatorState } from 'primeng/paginator';
import { ApiErrorHandler } from '../../core/api-error-handler';
import { OrdersService } from '../../core/orders.service';
import { Order } from '../../core/models/order';
import { PagedList } from '../../core/models/paged-list';
import { orderStatusLabel } from '../../core/models/order-status';

@Component({
  selector: 'app-orders',
  imports: [RouterLink, Button, Card, Message, Paginator],
  templateUrl: './orders.html',
  styleUrl: './orders.scss',
})
export class Orders {
  private readonly orders = inject(OrdersService);
  private readonly errors = inject(ApiErrorHandler);

  protected readonly page = signal<PagedList<Order> | null>(null);
  protected readonly failure = signal<string | null>(null);
  protected readonly pageSize = 10;
  protected readonly label = orderStatusLabel;

  constructor() {
    void this.load(1);
  }

  protected async onPage(event: PaginatorState): Promise<void> {
    await this.load((event.page ?? 0) + 1);
  }

  private async load(pageNumber: number): Promise<void> {
    this.failure.set(null);
    try {
      this.page.set(await firstValueFrom(this.orders.history(pageNumber, this.pageSize)));
    } catch (error: unknown) {
      this.failure.set(this.errors.toMessage(error));
    }
  }
}
