import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
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
        path: 'profile',
        title: 'Perfil — OrdemCerta',
        data: { breadcrumb: 'Perfil' },
        loadComponent: () =>
          import('./features/profile/profile.component').then((m) => m.ProfileComponent),
      },
    ],
  },
  {
    path: 'public/orders/:id/approve',
    title: 'Aprovação de Orçamento — OrdemCerta',
    loadComponent: () =>
      import('./features/public/budget-response/budget-response.component').then(
        (m) => m.BudgetResponseComponent
      ),
  },
  {
    path: 'public/orders/:id/refuse',
    title: 'Recusa de Orçamento — OrdemCerta',
    loadComponent: () =>
      import('./features/public/budget-response/budget-response.component').then(
        (m) => m.BudgetResponseComponent
      ),
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
