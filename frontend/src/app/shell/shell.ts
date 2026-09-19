import { Component, computed, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { Button } from 'primeng/button';
import { Menubar } from 'primeng/menubar';
import { Toast } from 'primeng/toast';
import { AuthStore } from '../core/auth-store';
import { CartStore } from '../core/cart-store';
import { ThemeService } from '../core/theme.service';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, Menubar, Toast, Button],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
})
export class Shell {
  private readonly auth = inject(AuthStore);
  private readonly cart = inject(CartStore);
  protected readonly theme = inject(ThemeService);

  protected readonly menuItems = computed<MenuItem[]>(() => {
    const items: MenuItem[] = [{ label: 'Catalog', routerLink: ['/catalog'] }];
    if (this.auth.isAuthenticated()) {
      items.push({ label: `Cart (${this.cart.count()})`, routerLink: ['/cart'] });
      items.push({ label: 'Orders', routerLink: ['/orders'] });
      if (this.isStaff()) {
        items.push({
          label: 'Admin',
          items: [
            { label: 'Dashboard', routerLink: ['/admin'] },
            { label: 'Products', routerLink: ['/admin/products'] },
            { label: 'Inventory', routerLink: ['/admin/inventory'] },
            { label: 'Order queue', routerLink: ['/admin/orders'] },
            { label: 'Pricing', routerLink: ['/admin/pricing'] },
            { label: 'Coupons', routerLink: ['/admin/coupons'] },
            { label: 'Customers', routerLink: ['/admin/customers'] },
          ],
        });
      }
      items.push({
        label: 'Log out',
        command: () => this.auth.logout(),
      });
    } else {
      items.push({ label: 'Log in', routerLink: ['/login'] });
      items.push({ label: 'Register', routerLink: ['/register'] });
    }
    return items;
  });

  private isStaff(): boolean {
    return this.auth.hasRole('Admin') || this.auth.hasRole('SalesEmployee');
  }
}
