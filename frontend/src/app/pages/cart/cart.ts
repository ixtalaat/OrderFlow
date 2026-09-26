import { Component, inject } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { LangService } from '../../core/lang.service';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputNumber } from 'primeng/inputnumber';
import { CartStore } from '../../core/cart-store';
import { ProductArt } from '../../shared/product-art/product-art';

@Component({
  selector: 'app-cart',
  imports: [CurrencyPipe, FormsModule, RouterLink, Button, Card, InputNumber, ProductArt],
  templateUrl: './cart.html',
  styleUrl: './cart.scss',
})
export class Cart {
  protected readonly cart = inject(CartStore);
  protected readonly lang = inject(LangService);

  protected setQuantity(productId: number, quantity: number | null): void {
    this.cart.setQuantity(productId, quantity ?? 0);
  }
}
