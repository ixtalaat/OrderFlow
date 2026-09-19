import { Injectable, inject } from '@angular/core';
import { MessageService } from 'primeng/api';

@Injectable({ providedIn: 'root' })
export class ToastNotify {
  private readonly messages = inject(MessageService);

  success(detail: string, summary = 'Done'): void {
    this.messages.add({ severity: 'success', summary, detail });
  }

  error(detail: string, summary = 'Error'): void {
    this.messages.add({ severity: 'error', summary, detail, sticky: false });
  }
}
