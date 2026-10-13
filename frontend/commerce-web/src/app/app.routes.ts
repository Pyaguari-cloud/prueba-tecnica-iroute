import { Routes } from '@angular/router';

/**
 * Rutas de la aplicación.
 * Cada pantalla se carga de forma diferida (lazy loading): su código
 * solo se descarga cuando el usuario navega a ella.
 */
export const routes: Routes = [
  { path: '', redirectTo: 'upload', pathMatch: 'full' },
  {
    path: 'upload',
    title: 'Carga de archivo | Commerce ETL',
    loadComponent: () => import('./pages/upload/upload-page').then(m => m.UploadPage)
  },
  {
    path: 'process',
    title: 'Procesamiento | Commerce ETL',
    loadComponent: () => import('./pages/process/process-page').then(m => m.ProcessPage)
  },
  {
    path: 'quarantine',
    title: 'Cuarentena | Commerce ETL',
    loadComponent: () => import('./pages/quarantine/quarantine-page').then(m => m.QuarantinePage)
  },
  { path: '**', redirectTo: 'upload' }
];