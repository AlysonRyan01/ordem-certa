import { Routes } from '@angular/router';

export const salesRoutes: Routes = [
  {
    path: '',
    title: 'Vendas — OrdemCerta',
    loadComponent: () => import('./sale-list/sale-list.component').then((m) => m.SaleListComponent),
  },
  {
    path: 'new',
    title: 'Nova Venda — OrdemCerta',
    data: { breadcrumb: 'Nova Venda' },
    loadComponent: () => import('./sale-form/sale-form.component').then((m) => m.SaleFormComponent),
  },
  {
    path: ':id',
    title: 'Detalhe da Venda — OrdemCerta',
    data: { breadcrumb: 'Detalhe' },
    loadComponent: () => import('./sale-detail/sale-detail.component').then((m) => m.SaleDetailComponent),
  },
  {
    path: ':id/edit',
    title: 'Editar Venda — OrdemCerta',
    data: { breadcrumb: 'Editar' },
    loadComponent: () => import('./sale-form/sale-form.component').then((m) => m.SaleFormComponent),
  },
];
