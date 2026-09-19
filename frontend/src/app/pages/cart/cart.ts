import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputNumber } from 'primeng/inputnumber';
import { CartStore } from '../../core/cart-store';

@Component({
  selector: 'app-cart',
  imports: [FormsModule, RouterLink, Button, Card, InputNumber],
  templateUrl: './cart.html',
  styleUrl: './cart.scss',
})
export class Cart {
  protected readonly cart = inject(CartStore);

  protected setQuantity(productId: number, quantity: number | null): void {
    this.cart.setQuantity(productId, quantity ?? 0);
  }
}
