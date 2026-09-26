import { Injectable, inject } from '@angular/core';
import { MessageService } from 'primeng/api';
import { LangService } from './lang.service';

@Injectable({ providedIn: 'root' })
export class ToastNotify {
  private readonly messages = inject(MessageService);
  private readonly lang = inject(LangService);

  success(detail: string, summary = 'Done'): void {
    this.messages.add({ severity: 'success', summary, detail });
  }

  cartAdded(productName: string): void {
    this.messages.add({
      severity: 'success',
      summary: this.lang.t('catalog.addedToCart'),
      detail: productName,
      data: { action: 'cart' },
    });
  }

  error(detail: string, summary = 'Error'): void {
    this.messages.add({ severity: 'error', summary, detail, sticky: false });
  }
}
