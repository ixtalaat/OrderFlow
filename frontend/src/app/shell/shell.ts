import { Component, computed, inject, signal } from '@angular/core';
import { MessageService, PrimeTemplate } from 'primeng/api';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Button } from 'primeng/button';
import { Drawer } from 'primeng/drawer';
import { Toast } from 'primeng/toast';
import { AuthStore } from '../core/auth-store';
import { CartStore } from '../core/cart-store';
import { LangService } from '../core/lang.service';
import { ThemeService } from '../core/theme.service';
import { Logo } from '../shared/logo/logo';

interface NavLink {
  label: string;
  route: string[];
  badge?: number;
}

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, Button, Drawer, Logo, PrimeTemplate, Toast],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
})
export class Shell {
  private readonly auth = inject(AuthStore);
  private readonly messages = inject(MessageService);
  private readonly cart = inject(CartStore);
  protected readonly theme = inject(ThemeService);
  protected readonly lang = inject(LangService);
  protected readonly menuOpen = signal(false);

  protected readonly cartCount = computed(() => this.cart.count());

  protected readonly primaryLinks = computed<NavLink[]>(() => {
    if (!this.auth.isAuthenticated()) return [];
    // Shop pages are Customer-only on the API (403 for other roles),
    // so only link them for customers instead of a dead end.
    if (this.isStaff() && !this.auth.hasRole('Customer')) {
      return [
        { label: this.lang.t('nav.dashboard'), route: ['/admin'] },
        { label: this.lang.t('nav.products'), route: ['/admin/products'] },
        { label: this.lang.t('nav.inventory'), route: ['/admin/inventory'] },
        { label: this.lang.t('nav.queue'), route: ['/admin/orders'] },
        { label: this.lang.t('nav.pricing'), route: ['/admin/pricing'] },
        { label: this.lang.t('nav.coupons'), route: ['/admin/coupons'] },
        { label: this.lang.t('nav.customers'), route: ['/admin/customers'] },
      ];
    }
    return [
      { label: this.lang.t('nav.catalog'), route: ['/catalog'] },
      { label: this.lang.t('nav.cart'), route: ['/cart'], badge: this.cart.count() },
      { label: this.lang.t('nav.orders'), route: ['/orders'] },
    ];
  });

  protected readonly isLoggedIn = computed(() => this.auth.isAuthenticated());

  protected closeMenu(): void {
    this.menuOpen.set(false);
  }

  protected clearToasts(): void {
    this.messages.clear();
  }

  protected logout(): void {
    this.closeMenu();
    this.auth.logout();
  }

  private isStaff(): boolean {
    return this.auth.hasRole('Admin') || this.auth.hasRole('SalesEmployee');
  }
}
