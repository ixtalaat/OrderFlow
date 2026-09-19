import { Routes } from '@angular/router';
import { Home } from './pages/home/home';
import { NotFound } from './pages/not-found/not-found';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { ConfirmEmail } from './pages/confirm-email/confirm-email';
import { Catalog } from './pages/catalog/catalog';
import { ProductDetail } from './pages/product-detail/product-detail';
import { Cart } from './pages/cart/cart';
import { Checkout } from './pages/checkout/checkout';
import { Orders } from './pages/orders/orders';
import { OrderDetail } from './pages/order-detail/order-detail';
import { AdminDashboard } from './pages/admin/dashboard/dashboard';
import { AdminProducts } from './pages/admin/products/products';
import { AdminInventory } from './pages/admin/inventory/inventory';
import { AdminOrders } from './pages/admin/orders/orders';
import { AdminPricing } from './pages/admin/pricing/pricing';
import { AdminCoupons } from './pages/admin/coupons/coupons';
import { AdminCustomers } from './pages/admin/customers/customers';
import { authGuard, roleGuard } from './core/auth-guards';

const staffOnly = roleGuard('Admin', 'SalesEmployee');

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'confirm-email', component: ConfirmEmail },
  { path: 'catalog', component: Catalog, canActivate: [authGuard] },
  { path: 'catalog/:id', component: ProductDetail, canActivate: [authGuard] },
  { path: 'cart', component: Cart, canActivate: [authGuard] },
  { path: 'checkout', component: Checkout, canActivate: [authGuard] },
  { path: 'orders', component: Orders, canActivate: [authGuard] },
  { path: 'orders/:id', component: OrderDetail, canActivate: [authGuard] },
  { path: 'admin', component: AdminDashboard, canActivate: [staffOnly] },
  { path: 'admin/products', component: AdminProducts, canActivate: [staffOnly] },
  { path: 'admin/inventory', component: AdminInventory, canActivate: [staffOnly] },
  { path: 'admin/orders', component: AdminOrders, canActivate: [staffOnly] },
  { path: 'admin/pricing', component: AdminPricing, canActivate: [staffOnly] },
  { path: 'admin/coupons', component: AdminCoupons, canActivate: [staffOnly] },
  { path: 'admin/customers', component: AdminCustomers, canActivate: [staffOnly] },
  { path: '**', component: NotFound },
];
