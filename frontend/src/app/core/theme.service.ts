import { Injectable, signal } from '@angular/core';

const STORAGE_KEY = 'orderflow_theme';
const DARK_CLASS = 'app-dark';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  readonly dark = signal(false);

  constructor() {
    const stored = localStorage.getItem(STORAGE_KEY);
    const prefersDark =
      typeof window.matchMedia === 'function' &&
      window.matchMedia('(prefers-color-scheme: dark)').matches;
    this.apply(stored ? stored === 'dark' : prefersDark);
  }

  toggle(): void {
    this.apply(!this.dark());
  }

  private apply(dark: boolean): void {
    this.dark.set(dark);
    document.documentElement.classList.toggle(DARK_CLASS, dark);
    try {
      localStorage.setItem(STORAGE_KEY, dark ? 'dark' : 'light');
    } catch {
      // Storage unavailable (private mode): theme still applies for the session.
    }
  }
}
