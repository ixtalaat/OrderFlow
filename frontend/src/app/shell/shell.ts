import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Button } from 'primeng/button';
import { Drawer } from 'primeng/drawer';
import { Toast } from 'primeng/toast';
import { AuthStore } from '../core/auth-store';
import { CartStore } from '../core/cart-store';
import { ThemeService } from '../core/theme.service';

interface NavLink {
  label: string;
  route: string[];
  badge?: number;
}

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, Button, Drawer, Toast],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
})
export class Shell {
  private readonly auth = inject(AuthStore);
  private readonly cart = inject(CartStore);
  protected readonly theme = inject(ThemeService);
  protected readonly menuOpen = signal(false);

  protected readonly primaryLinks = computed<NavLink[]>(() => {
    if (!this.auth.isAuthenticated()) return [];
    const links: NavLink[] = [
      { label: 'Catalog', route: ['/catalog'] },
      { label: `Cart (${this.cart.count()})`, route: ['/cart'] },
      { label: 'Orders', route: ['/orders'] },
    ];
    if (this.isStaff()) {
      links.push(
        { label: 'Dashboard', route: ['/admin'] },
        { label: 'Products', route: ['/admin/products'] },
        { label: 'Inventory', route: ['/admin/inventory'] },
        { label: 'Queue', route: ['/admin/orders'] },
        { label: 'Pricing', route: ['/admin/pricing'] },
        { label: 'Coupons', route: ['/admin/coupons'] },
        { label: 'Customers', route: ['/admin/customers'] },
      );
    }
    return links;
  });

  protected readonly isLoggedIn = computed(() => this.auth.isAuthenticated());

  protected closeMenu(): void {
    this.menuOpen.set(false);
  }

  protected logout(): void {
    this.closeMenu();
    this.auth.logout();
  }

  private isStaff(): boolean {
    return this.auth.hasRole('Admin') || this.auth.hasRole('SalesEmployee');
  }
}
