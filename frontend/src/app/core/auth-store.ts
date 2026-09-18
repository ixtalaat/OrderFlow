import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AppConfigService } from './app-config.service';
import { AuthResponse, RefreshResponse } from './models';

const REFRESH_KEY = 'ordermint_refresh_token';

function decodeRoles(token: string): string[] {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const roles = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    if (!roles) return [];
    return Array.isArray(roles) ? roles : [roles];
  } catch {
    return [];
  }
}

@Injectable({ providedIn: 'root' })
export class AuthStore {
  private readonly http = inject(HttpClient);
  private readonly config = inject(AppConfigService);
  private readonly router = inject(Router);

  readonly accessToken = signal<string | null>(null);
  readonly roles = signal<string[]>([]);
  readonly isAuthenticated = computed(() => this.accessToken() !== null);

  private refreshInFlight: Promise<string> | null = null;

  async login(email: string, password: string): Promise<void> {
    const response = await firstValueFrom(
      this.http.post<AuthResponse>(`${this.config.apiUrl}/api/auth/login`, { email, password }),
    );
    this.setSession(response.accessToken, response.refreshToken ?? null);
  }

  async register(email: string, password: string, fullName: string): Promise<void> {
    await firstValueFrom(
      this.http.post(`${this.config.apiUrl}/api/auth/register`, { email, password, fullName }),
    );
  }

  logout(navigate = true): void {
    this.accessToken.set(null);
    this.roles.set([]);
    localStorage.removeItem(REFRESH_KEY);
    this.refreshInFlight = null;
    if (navigate) void this.router.navigate(['/login']);
  }

  refreshOnce(): Promise<string> {
    this.refreshInFlight ??= this.doRefresh().finally(() => {
      this.refreshInFlight = null;
    });
    return this.refreshInFlight;
  }

  hasRole(role: string): boolean {
    return this.roles().includes(role);
  }

  private async doRefresh(): Promise<string> {
    const refreshToken = localStorage.getItem(REFRESH_KEY);
    if (!refreshToken) {
      this.logout();
      throw new Error('No refresh token available.');
    }
    try {
      const response = await firstValueFrom(
        this.http.post<RefreshResponse>(`${this.config.apiUrl}/api/auth/refresh`, { refreshToken }),
      );
      this.setSession(response.accessToken, response.refreshToken);
      return response.accessToken;
    } catch (error) {
      this.logout();
      throw error;
    }
  }

  private setSession(accessToken: string, refreshToken: string | null): void {
    this.accessToken.set(accessToken);
    this.roles.set(decodeRoles(accessToken));
    if (refreshToken) {
      localStorage.setItem(REFRESH_KEY, refreshToken);
    }
  }
}
