import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

/**
 * Componente raíz: barra de navegación + área donde el router
 * dibuja la pantalla activa (<router-outlet />).
 */
@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  /** Opciones del menú principal. */
  protected readonly navItems = [
    { path: '/upload', label: '1. Carga' },
    { path: '/process', label: '2. Procesamiento' },
    { path: '/quarantine', label: '3. Cuarentena' }
  ];
}