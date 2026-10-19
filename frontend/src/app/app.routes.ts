import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  {
    path: 'dashboard',
    title: 'Painel | LabTrack',
    loadComponent: () => import('./pages/dashboard/dashboard').then((m) => m.DashboardPage),
  },
  {
    path: 'samples',
    title: 'Amostras | LabTrack',
    loadComponent: () => import('./pages/sample-list/sample-list').then((m) => m.SampleListPage),
  },
  {
    path: 'samples/new',
    title: 'Nova amostra | LabTrack',
    loadComponent: () => import('./pages/sample-form/sample-form').then((m) => m.SampleFormPage),
  },
  {
    path: 'samples/:id',
    title: 'Amostra | LabTrack',
    loadComponent: () => import('./pages/sample-detail/sample-detail').then((m) => m.SampleDetailPage),
  },
  {
    path: 'samples/:id/edit',
    title: 'Editar amostra | LabTrack',
    loadComponent: () => import('./pages/sample-form/sample-form').then((m) => m.SampleFormPage),
  },
  { path: '**', redirectTo: 'dashboard' },
];
