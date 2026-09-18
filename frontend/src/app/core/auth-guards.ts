import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from './auth-store';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthStore);
  const router = inject(Router);
  if (auth.isAuthenticated()) return true;
  void router.navigate(['/login']);
  return false;
};

export function roleGuard(...roles: string[]): CanActivateFn {
  return () => {
    const auth = inject(AuthStore);
    const router = inject(Router);
    if (!auth.isAuthenticated()) {
      void router.navigate(['/login']);
      return false;
    }
    if (roles.some((role) => auth.hasRole(role))) return true;
    void router.navigate(['/']);
    return false;
  };
}
