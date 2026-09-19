import { HttpErrorResponse } from '@angular/common/http';
import { ApiErrorHandler } from './api-error-handler';

describe('ApiErrorHandler', () => {
  const handler = new ApiErrorHandler();

  function problem(code: string, status = 400): HttpErrorResponse {
    return new HttpErrorResponse({
      status,
      error: { extensions: { errors: [code] } },
    });
  }

  it('should map known backend codes', () => {
    expect(handler.toMessage(problem('Order.InsufficientStock'))).toContain('stock');
    expect(handler.toMessage(problem('Authentication.EmailNotConfirmed', 403))).toContain('confirm');
    expect(handler.toCode(problem('Order.ConcurrencyConflict', 409))).toBe(
      'Order.ConcurrencyConflict',
    );
  });

  it('should explain rate limiting and outages', () => {
    expect(handler.toMessage(new HttpErrorResponse({ status: 429 }))).toContain('slow down');
    expect(handler.toMessage(new HttpErrorResponse({ status: 0 }))).toContain('reach the server');
  });

  it('should fall back for unknown errors', () => {
    expect(handler.toMessage(new HttpErrorResponse({ status: 500 }))).toContain('Something went wrong');
    expect(handler.toMessage(new Error('boom'))).toContain('Something went wrong');
    expect(handler.toCode(new Error('boom'))).toBeNull();
  });
});
