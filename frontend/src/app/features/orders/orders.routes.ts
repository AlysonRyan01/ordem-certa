import { Routes } from '@angular/router';

export const ordersRoutes: Routes = [
  {
    path: '',
    title: 'Ordens de Serviço — OrdemCerta',
    loadComponent: () =>
      import('./order-list/order-list.component').then((m) => m.OrderListComponent),
  },
  {
    path: 'new',
    title: 'Nova Ordem — OrdemCerta',
    data: { breadcrumb: 'Nova' },
    loadComponent: () =>
      import('./order-form/order-form.component').then((m) => m.OrderFormComponent),
  },
  {
    path: ':id',
    title: 'Detalhe da Ordem — OrdemCerta',
    data: { breadcrumb: 'Detalhe' },
    loadComponent: () =>
      import('./order-detail/order-detail.component').then((m) => m.OrderDetailComponent),
  },
  {
    path: ':id/edit',
    title: 'Editar Ordem — OrdemCerta',
    data: { breadcrumb: 'Editar' },
    loadComponent: () =>
      import('./order-form/order-form.component').then((m) => m.OrderFormComponent),
  },
];
