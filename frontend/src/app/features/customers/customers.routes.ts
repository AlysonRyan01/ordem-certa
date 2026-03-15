import { Routes } from '@angular/router';

export const customersRoutes: Routes = [
  {
    path: '',
    title: 'Clientes — OrdemCerta',
    loadComponent: () =>
      import('./customer-list/customer-list.component').then((m) => m.CustomerListComponent),
  },
  {
    path: 'new',
    title: 'Novo Cliente — OrdemCerta',
    data: { breadcrumb: 'Novo' },
    loadComponent: () =>
      import('./customer-form/customer-form.component').then((m) => m.CustomerFormComponent),
  },
  {
    path: ':id',
    title: 'Detalhe do Cliente — OrdemCerta',
    data: { breadcrumb: 'Detalhe' },
    loadComponent: () =>
      import('./customer-detail/customer-detail.component').then(
        (m) => m.CustomerDetailComponent
      ),
  },
  {
    path: ':id/edit',
    title: 'Editar Cliente — OrdemCerta',
    data: { breadcrumb: 'Editar' },
    loadComponent: () =>
      import('./customer-form/customer-form.component').then((m) => m.CustomerFormComponent),
  },
];
