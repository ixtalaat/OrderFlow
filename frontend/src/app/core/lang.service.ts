import { Injectable, signal } from '@angular/core';
import { STRINGS } from './i18n-dictionary';

export type Lang = 'en' | 'ar';

const STORAGE_KEY = 'orderflow_lang';

@Injectable({ providedIn: 'root' })
export class LangService {
  readonly current = signal<Lang>('en');

  constructor() {
    let initial: Lang = 'en';
    try {
      const saved = localStorage.getItem(STORAGE_KEY);
      if (saved === 'ar' || saved === 'en') initial = saved;
    } catch {
      // Private mode: fall back to English.
    }
    this.apply(initial);
  }

  toggle(): void {
    this.apply(this.current() === 'en' ? 'ar' : 'en');
  }

  t(key: string): string {
    const lang = this.current();
    return STRINGS[lang][key] ?? STRINGS.en[key] ?? key;
  }

  apply(lang: Lang): void {
    this.current.set(lang);
    document.documentElement.lang = lang;
    document.documentElement.dir = lang === 'ar' ? 'rtl' : 'ltr';
    try {
      localStorage.setItem(STORAGE_KEY, lang);
    } catch {
      // Ignore persistence failures.
    }
  }
}
