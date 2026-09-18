import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, from, switchMap, throwError } from 'rxjs';
import { AuthStore } from './auth-store';

const ANONYMOUS_PATHS = [
  '/api/auth/login',
  '/api/auth/register',
  '/api/auth/refresh',
  '/api/auth/confirm-email',
  '/api/auth/resend-confirmation',
];

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthStore);
  const isAnonymous = ANONYMOUS_PATHS.some((path) => request.url.includes(path));

  let outgoing = request.clone({
    setHeaders: { 'X-Correlation-ID': crypto.randomUUID() },
  });
  const token = auth.accessToken();
  if (token && !isAnonymous) {
    outgoing = outgoing.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  }

  return next(outgoing).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse) || error.status !== 401 || isAnonymous) {
        return throwError(() => error);
      }
      return from(auth.refreshOnce()).pipe(
        switchMap((freshToken) =>
          next(
            outgoing.clone({
              setHeaders: {
                Authorization: `Bearer ${freshToken}`,
                'X-Correlation-ID': crypto.randomUUID(),
              },
            }),
          ),
        ),
        catchError((refreshError: unknown) => throwError(() => refreshError)),
      );
    }),
  );
};
