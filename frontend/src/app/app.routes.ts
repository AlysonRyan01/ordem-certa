import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    title: 'OrdemCerta — Gestão de Ordens de Serviço via WhatsApp',
    loadComponent: () =>
      import('./features/landing/landing.component').then((m) => m.LandingComponent),
  },
  {
    path: 'login',
    title: 'Login — OrdemCerta',
    loadComponent: () =>
      import('./features/auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'register',
    title: 'Criar conta — OrdemCerta',
    loadComponent: () =>
      import('./features/auth/register/register.component').then((m) => m.RegisterComponent),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./layouts/main-layout/main-layout.component').then((m) => m.MainLayoutComponent),
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
      {
        path: 'dashboard',
        title: 'Dashboard — OrdemCerta',
        data: { breadcrumb: 'Dashboard' },
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent),
      },
      {
        path: 'orders',
        data: { breadcrumb: 'Ordens de Serviço' },
        loadChildren: () =>
          import('./features/orders/orders.routes').then((m) => m.ordersRoutes),
      },
      {
        path: 'customers',
        data: { breadcrumb: 'Clientes' },
        loadChildren: () =>
          import('./features/customers/customers.routes').then((m) => m.customersRoutes),
      },
      {
        path: 'sales',
        data: { breadcrumb: 'Vendas' },
        loadChildren: () =>
          import('./features/sales/sales.routes').then((m) => m.salesRoutes),
      },
      {
        path: 'profile',
        title: 'Perfil — OrdemCerta',
        data: { breadcrumb: 'Perfil' },
        loadComponent: () =>
          import('./features/profile/profile.component').then((m) => m.ProfileComponent),
      },
      {
        path: 'billing',
        title: 'Planos — OrdemCerta',
        data: { breadcrumb: 'Planos' },
        loadComponent: () =>
          import('./features/billing/billing.component').then((m) => m.BillingComponent),
      },
    ],
  },
  {
    path: 'billing/success',
    title: 'Assinatura ativada — OrdemCerta',
    loadComponent: () =>
      import('./features/billing/billing-success/billing-success.component').then((m) => m.BillingSuccessComponent),
  },
  {
    path: 'billing/cancel',
    title: 'Checkout cancelado — OrdemCerta',
    loadComponent: () =>
      import('./features/billing/billing-cancel/billing-cancel.component').then((m) => m.BillingCancelComponent),
  },
  {
    path: 'orcamento/order/:id',
    title: 'Orçamento — OrdemCerta',
    loadComponent: () =>
      import('./features/public/budget-view/budget-view.component').then(
        (m) => m.BudgetViewComponent
      ),
  },
  {
    path: 'admin',
    loadChildren: () =>
      import('./features/admin/admin.routes').then((m) => m.adminRoutes),
  },
  {
    path: '**',
    title: 'Página não encontrada — OrdemCerta',
    loadComponent: () =>
      import('./shared/components/not-found/not-found.component').then(
        (m) => m.NotFoundComponent
      ),
  },
];
