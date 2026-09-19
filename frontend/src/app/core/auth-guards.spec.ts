import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot, provideRouter } from '@angular/router';
import { AuthStore } from './auth-store';
import { authGuard, roleGuard } from './auth-guards';

describe('auth guards', () => {
  let store: AuthStore;
  let router: Router;
  const state = {} as RouterStateSnapshot;
  const route = {} as ActivatedRouteSnapshot;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    store = TestBed.inject(AuthStore);
    router = TestBed.inject(Router);
    spyOn(router, 'navigate').and.resolveTo(true);
  });

  it('authGuard should allow authenticated users', () => {
    store.accessToken.set('token');

    expect(TestBed.runInInjectionContext(() => authGuard(route, state))).toBeTrue();
  });

  it('authGuard should redirect anonymous users to login', () => {
    store.logout(false);

    expect(TestBed.runInInjectionContext(() => authGuard(route, state))).toBeFalse();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('roleGuard should allow matching roles', () => {
    store.accessToken.set('token');
    store.roles.set(['Admin']);

    const guard = roleGuard('Admin', 'SalesEmployee');
    expect(TestBed.runInInjectionContext(() => guard(route, state))).toBeTrue();
  });

  it('roleGuard should redirect home on insufficient role', () => {
    store.accessToken.set('token');
    store.roles.set(['Customer']);

    const guard = roleGuard('Admin');
    expect(TestBed.runInInjectionContext(() => guard(route, state))).toBeFalse();
    expect(router.navigate).toHaveBeenCalledWith(['/']);
  });
});
