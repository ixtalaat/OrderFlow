import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputNumber } from 'primeng/inputnumber';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { ApiErrorHandler } from '../../../core/api-error-handler';
import { InventoryService } from '../../../core/inventory.service';
import { InventoryRecord } from '../../../core/models/inventory-record';

@Component({
  selector: 'app-admin-inventory',
  imports: [FormsModule, Button, Card, InputNumber, InputText, Message],
  templateUrl: './inventory.html',
  styleUrl: './inventory.scss',
})
export class AdminInventory {
  private readonly inventory = inject(InventoryService);
  private readonly errors = inject(ApiErrorHandler);

  protected readonly productId = signal<number | null>(null);
  protected readonly record = signal<InventoryRecord | null>(null);
  protected readonly quantity = signal<number>(1);
  protected readonly reason = signal('');
  protected readonly failure = signal<string | null>(null);

  protected async lookup(): Promise<void> {
    const id = this.productId();
    if (!id) return;
    this.failure.set(null);
    try {
      this.record.set(await firstValueFrom(this.inventory.get(id)));
    } catch (error: unknown) {
      this.record.set(null);
      this.failure.set(this.errors.toMessage(error));
    }
  }

  protected async operate(action: 'add-stock' | 'reserve' | 'release' | 'confirm'): Promise<void> {
    const id = this.productId();
    if (!id) return;
    this.failure.set(null);
    try {
      this.record.set(await firstValueFrom(this.inventory.operate(id, action, this.quantity())));
    } catch (error: unknown) {
      this.failure.set(this.errors.toMessage(error));
    }
  }

  protected async adjust(): Promise<void> {
    const id = this.productId();
    if (!id) return;
    this.failure.set(null);
    try {
      this.record.set(
        await firstValueFrom(this.inventory.adjust(id, this.quantity(), this.reason())),
      );
    } catch (error: unknown) {
      this.failure.set(this.errors.toMessage(error));
    }
  }
}
