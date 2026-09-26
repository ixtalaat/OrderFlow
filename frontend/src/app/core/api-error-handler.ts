import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { LangService } from './lang.service';
import { ProblemDetails } from './models/problem-details';

const CODE_KEYS: Record<string, string> = {
  'Authentication.InvalidCredentials': 'error.badCredentials',
  'Authentication.EmailAlreadyExists': 'error.emailExists',
  'Authentication.EmailNotConfirmed': 'error.emailNotConfirmed',
  'Authentication.InvalidRefreshToken': 'error.refreshInvalid',
  'Order.InsufficientStock': 'error.insufficientStock',
  'Order.InvalidCoupon': 'error.invalidCoupon',
  'Order.ConcurrencyConflict': 'error.concurrency',
  'Order.IdempotentPayloadMismatch': 'error.payloadMismatch',
};

@Injectable({ providedIn: 'root' })
export class ApiErrorHandler {
  private readonly lang = inject(LangService);

  toMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 0) return this.lang.t('error.unreachable');
      if (error.status === 401) return this.lang.t('error.session');
      if (error.status === 403) return this.lang.t('error.forbidden');
      if (error.status === 429) return this.lang.t('error.tooMany');
      const code = (error.error as ProblemDetails | null)?.extensions?.errors?.[0];
      if (code && CODE_KEYS[code]) return this.lang.t(CODE_KEYS[code]);
      if (typeof error.error === 'string' && error.error) return error.error;
    }
    return this.lang.t('error.generic');
  }

  toCode(error: unknown): string | null {
    if (error instanceof HttpErrorResponse) {
      return (error.error as ProblemDetails | null)?.extensions?.errors?.[0] ?? null;
    }
    return null;
  }
}
