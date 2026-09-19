import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { firstValueFrom } from 'rxjs';
import { ApiErrorHandler } from '../../core/api-error-handler';
import { CartStore } from '../../core/cart-store';
import { OrdersService } from '../../core/orders.service';

@Component({
  selector: 'app-checkout',
  imports: [FormsModule, RouterLink, Button, Card, InputText, Message],
  templateUrl: './checkout.html',
  styleUrl: './checkout.scss',
})
export class Checkout {
  private readonly orders = inject(OrdersService);
  private readonly errors = inject(ApiErrorHandler);
  private readonly router = inject(Router);
  protected readonly cart = inject(CartStore);

  protected readonly couponCode = signal('');
  protected readonly failure = signal<string | null>(null);
  protected readonly busy = signal(false);
  private lastSignature: string | null = null;
  private idempotencyKey: string | null = null;

  protected async submit(): Promise<void> {
    if (this.busy() || this.cart.lines().length === 0) return;
    this.failure.set(null);
    this.busy.set(true);
    try {
      const signature = this.cart.signature();
      if (signature !== this.lastSignature || !this.idempotencyKey) {
        this.idempotencyKey = crypto.randomUUID();
        this.lastSignature = signature;
      }
      const coupon = this.couponCode().trim() || null;
      const order = await firstValueFrom(
        this.orders.create(
          this.cart.lines().map((line) => ({ productId: line.productId, quantity: line.quantity })),
          { couponCode: coupon, idempotencyKey: this.idempotencyKey },
        ),
      );
      this.cart.clear();
      this.lastSignature = null;
      this.idempotencyKey = null;
      await this.router.navigate(['/orders', order.id]);
    } catch (error: unknown) {
      this.failure.set(this.errors.toMessage(error));
    } finally {
      this.busy.set(false);
    }
  }
}
