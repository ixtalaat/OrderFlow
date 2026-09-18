import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface AppConfig {
  apiUrl: string;
}

@Injectable({ providedIn: 'root' })
export class AppConfigService {
  private readonly http = inject(HttpClient);
  private readonly config = signal<AppConfig | null>(null);

  load(): Promise<void> {
    return new Promise((resolve) => {
      this.http.get<AppConfig>('/config.json').subscribe({
        next: (config) => {
          this.config.set(config);
          resolve();
        },
        error: () => {
          this.config.set({ apiUrl: 'http://localhost:8080' });
          resolve();
        },
      });
    });
  }

  get apiUrl(): string {
    return this.config()?.apiUrl ?? 'http://localhost:8080';
  }
}
