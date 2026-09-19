import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { Button } from 'primeng/button';
import { Dialog } from 'primeng/dialog';
import { InputNumber } from 'primeng/inputnumber';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { ApiErrorHandler } from '../../../core/api-error-handler';
import { CouponsService } from '../../../core/coupons.service';

@Component({
  selector: 'app-admin-coupons',
  imports: [FormsModule, Button, Dialog, InputNumber, InputText, Message],
  templateUrl: './coupons.html',
  styleUrl: './coupons.scss',
})
export class AdminCoupons {
  private readonly coupons = inject(CouponsService);
  private readonly errors = inject(ApiErrorHandler);

  protected readonly failure = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);
  protected readonly dialogOpen = signal(false);
  protected readonly form = {
    code: '',
    discountPercentage: 10,
    minOrderTotal: 0,
    validFromUtc: new Date().toISOString(),
  };

  protected openCreate(): void {
    this.form.code = '';
    this.dialogOpen.set(true);
  }

  protected async save(): Promise<void> {
    this.failure.set(null);
    this.success.set(null);
    try {
      const created = await firstValueFrom(
        this.coupons.create({
          code: this.form.code,
          discountPercentage: this.form.discountPercentage,
          minOrderTotal: this.form.minOrderTotal,
          validFromUtc: this.form.validFromUtc,
          validToUtc: null,
          maxRedemptions: null,
        }),
      );
      this.dialogOpen.set(false);
      this.success.set(`Coupon ${created.code} created.`);
    } catch (error: unknown) {
      this.failure.set(this.errors.toMessage(error));
    }
  }
}
