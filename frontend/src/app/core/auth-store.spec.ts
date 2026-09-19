import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { AuthStore } from './auth-store';

function fakeJwt(roles: string[]): string {
  const payload = btoa(
    JSON.stringify({ 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': roles }),
  );
  return `header.${payload}.signature`;
}

describe('AuthStore', () => {
  let store: AuthStore;
  let http: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    store = TestBed.inject(AuthStore);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    localStorage.clear();
  });

  it('should store session and persist refresh token on login', async () => {
    const promise = store.login('user@example.com', 'Password@123');
    const request = http.expectOne('http://localhost:8080/api/auth/login');
    request.flush({
      accessToken: fakeJwt(['Customer']),
      expiresAt: new Date().toISOString(),
      userId: 'user-1',
      email: 'user@example.com',
      roles: ['Customer'],
      refreshToken: 'refresh-1',
    });
    await promise;

    expect(store.isAuthenticated()).toBeTrue();
    expect(store.roles()).toEqual(['Customer']);
    expect(store.hasRole('Admin')).toBeFalse();
    expect(localStorage.getItem('orderflow_refresh_token')).toBe('refresh-1');
  });

  it('should clear session on logout', () => {
    localStorage.setItem('orderflow_refresh_token', 'refresh-1');
    store.accessToken.set(fakeJwt([]));
    store.logout(false);

    expect(store.isAuthenticated()).toBeFalse();
    expect(localStorage.getItem('orderflow_refresh_token')).toBeNull();
  });

  it('should share one refresh across concurrent callers', async () => {
    localStorage.setItem('orderflow_refresh_token', 'refresh-old');
    const first = store.refreshOnce();
    const second = store.refreshOnce();
    const request = http.expectOne('http://localhost:8080/api/auth/refresh');
    request.flush({
      accessToken: fakeJwt(['Customer']),
      expiresAtUtc: new Date().toISOString(),
      refreshToken: 'refresh-new',
      refreshExpiresAtUtc: new Date().toISOString(),
    });

    await expectAsync(first).toBeResolved();
    await expectAsync(second).toBeResolved();
    http.expectNone('http://localhost:8080/api/auth/refresh');
    expect(localStorage.getItem('orderflow_refresh_token')).toBe('refresh-new');
  });

  it('should logout when no refresh token exists', async () => {
    store.accessToken.set(fakeJwt([]));

    await expectAsync(store.refreshOnce()).toBeRejected();
    expect(store.isAuthenticated()).toBeFalse();
  });

  it('should restore session from a stored refresh token', async () => {
    localStorage.setItem('orderflow_refresh_token', 'refresh-old');
    const promise = store.restoreSession();
    const request = http.expectOne('http://localhost:8080/api/auth/refresh');
    request.flush({
      accessToken: fakeJwt(['Customer']),
      expiresAtUtc: new Date().toISOString(),
      refreshToken: 'refresh-new',
      refreshExpiresAtUtc: new Date().toISOString(),
    });
    await promise;

    expect(store.isAuthenticated()).toBeTrue();
  });

  it('should stay logged out when restore has no token', async () => {
    await store.restoreSession();

    expect(store.isAuthenticated()).toBeFalse();
    http.expectNone('http://localhost:8080/api/auth/refresh');
  });
});
