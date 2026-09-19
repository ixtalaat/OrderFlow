import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { Button } from 'primeng/button';
import { Dialog } from 'primeng/dialog';
import { InputNumber } from 'primeng/inputnumber';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { ApiErrorHandler } from '../../../core/api-error-handler';
import { PricingService } from '../../../core/pricing.service';

@Component({
  selector: 'app-admin-pricing',
  imports: [FormsModule, Button, Dialog, InputNumber, InputText, Message],
  templateUrl: './pricing.html',
  styleUrl: './pricing.scss',
})
export class AdminPricing {
  private readonly pricing = inject(PricingService);
  private readonly errors = inject(ApiErrorHandler);

  protected readonly failure = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);
  protected readonly dialogOpen = signal(false);
  protected readonly form = {
    productId: null as number | null,
    tier: 1,
    discountPercentage: 10,
    validFromUtc: new Date().toISOString(),
    validToUtc: null as string | null,
  };

  protected openCreate(): void {
    this.form.productId = null;
    this.dialogOpen.set(true);
  }

  protected async save(): Promise<void> {
    if (this.form.productId === null) return;
    this.failure.set(null);
    this.success.set(null);
    try {
      const created = await firstValueFrom(
        this.pricing.create({
          productId: this.form.productId,
          tier: this.form.tier,
          discountPercentage: this.form.discountPercentage,
          validFromUtc: this.form.validFromUtc,
          validToUtc: this.form.validToUtc,
        }),
      );
      this.dialogOpen.set(false);
      this.success.set(`Rule #${created.id} created.`);
    } catch (error: unknown) {
      this.failure.set(this.errors.toMessage(error));
    }
  }
}
