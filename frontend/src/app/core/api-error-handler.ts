import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ProblemDetails } from './models';

const CODE_MESSAGES: Record<string, string> = {
  'Authentication.InvalidCredentials': 'Invalid email or password.',
  'Authentication.EmailAlreadyExists': 'This email is already registered.',
  'Authentication.EmailNotConfirmed': 'Please confirm your email before logging in.',
  'Authentication.InvalidRefreshToken': 'Your session expired. Please log in again.',
  'Order.InsufficientStock': 'Not enough stock available.',
  'Order.InvalidCoupon': 'This coupon is invalid, expired, or exhausted.',
  'Order.ConcurrencyConflict': 'Someone just took the last items. Please try again.',
  'Order.IdempotentPayloadMismatch': 'This request conflicts with a previous one. Please start a new order.',
};

@Injectable({ providedIn: 'root' })
export class ApiErrorHandler {
  toMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 0) return 'Cannot reach the server. Please try again.';
      if (error.status === 429) return 'Too many requests. Please slow down.';
      const code = (error.error as ProblemDetails | null)?.extensions?.errors?.[0];
      if (code && CODE_MESSAGES[code]) return CODE_MESSAGES[code];
      if (typeof error.error === 'string' && error.error) return error.error;
    }
    return 'Something went wrong. Please try again.';
  }

  toCode(error: unknown): string | null {
    if (error instanceof HttpErrorResponse) {
      return (error.error as ProblemDetails | null)?.extensions?.errors?.[0] ?? null;
    }
    return null;
  }
}
