import { Routes } from '@angular/router';
import { adminGuard } from '../../core/guards/admin.guard';

export const adminRoutes: Routes = [
  {
    path: 'login',
    title: 'Admin — OrdemCerta',
    loadComponent: () =>
      import('./admin-login/admin-login.component').then((m) => m.AdminLoginComponent),
  },
  {
    path: '',
    title: 'Admin Dashboard — OrdemCerta',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('./admin-dashboard/admin-dashboard.component').then((m) => m.AdminDashboardComponent),
  },
  {
    path: 'companies',
    title: 'Empresas — Admin OrdemCerta',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('./admin-companies/admin-companies.component').then((m) => m.AdminCompaniesComponent),
  },
];
