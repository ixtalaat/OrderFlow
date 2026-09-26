import { Routes } from '@angular/router';
import { authGuard, roleGuard } from './core/auth-guards';

const staffOnly = roleGuard('Admin', 'SalesEmployee');

export const routes: Routes = [
  { path: '', loadComponent: () => import('./pages/home/home').then((m) => m.Home) },
  { path: 'login', loadComponent: () => import('./pages/login/login').then((m) => m.Login) },
  { path: 'register', loadComponent: () => import('./pages/register/register').then((m) => m.Register) },
  { path: 'confirm-email', loadComponent: () => import('./pages/confirm-email/confirm-email').then((m) => m.ConfirmEmail) },
  { path: 'catalog', loadComponent: () => import('./pages/catalog/catalog').then((m) => m.Catalog), canActivate: [authGuard] },
  { path: 'catalog/:id', loadComponent: () => import('./pages/product-detail/product-detail').then((m) => m.ProductDetail), canActivate: [authGuard] },
  { path: 'cart', loadComponent: () => import('./pages/cart/cart').then((m) => m.Cart), canActivate: [authGuard] },
  { path: 'checkout', loadComponent: () => import('./pages/checkout/checkout').then((m) => m.Checkout), canActivate: [authGuard] },
  { path: 'orders', loadComponent: () => import('./pages/orders/orders').then((m) => m.Orders), canActivate: [authGuard] },
  { path: 'orders/:id', loadComponent: () => import('./pages/order-detail/order-detail').then((m) => m.OrderDetail), canActivate: [authGuard] },
  { path: 'admin', loadComponent: () => import('./pages/admin/dashboard/dashboard').then((m) => m.AdminDashboard), canActivate: [staffOnly] },
  { path: 'admin/products', loadComponent: () => import('./pages/admin/products/products').then((m) => m.AdminProducts), canActivate: [staffOnly] },
  { path: 'admin/inventory', loadComponent: () => import('./pages/admin/inventory/inventory').then((m) => m.AdminInventory), canActivate: [staffOnly] },
  { path: 'admin/orders', loadComponent: () => import('./pages/admin/orders/orders').then((m) => m.AdminOrders), canActivate: [staffOnly] },
  { path: 'admin/pricing', loadComponent: () => import('./pages/admin/pricing/pricing').then((m) => m.AdminPricing), canActivate: [staffOnly] },
  { path: 'admin/coupons', loadComponent: () => import('./pages/admin/coupons/coupons').then((m) => m.AdminCoupons), canActivate: [staffOnly] },
  { path: 'admin/customers', loadComponent: () => import('./pages/admin/customers/customers').then((m) => m.AdminCustomers), canActivate: [staffOnly] },
  { path: '**', loadComponent: () => import('./pages/not-found/not-found').then((m) => m.NotFound) },
];
