import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { finalize } from 'rxjs';

import { QuarantineRecord } from '../../core/models/commerce.models';
import { CommerceApi } from '../../core/services/commerce-api';
import { toIsoDate } from '../../core/utils/date-utils';
import { getErrorMessage } from '../../core/utils/http-error';

/** Registro de cuarentena preparado para la vista (motivos separados). */
interface QuarantineRow extends QuarantineRecord {
  reasons: string[];
}

/**
 * Pantalla de Cuarentena: lista los registros rechazados con sus motivos.
 * Permite filtrar por fecha (en el servidor) y buscar texto (en el navegador).
 */
@Component({
  selector: 'app-quarantine-page',
  imports: [DatePipe],
  templateUrl: './quarantine-page.html',
  styleUrl: './quarantine-page.scss'
})
export class QuarantinePage {
  private readonly api = inject(CommerceApi);
  private readonly route = inject(ActivatedRoute);

  /** Filtro por fecha; viene de la URL (?date=yyyy-MM-dd) si se llega desde Procesamiento. */
  protected readonly dateFilter = signal(toIsoDate(this.route.snapshot.queryParamMap.get('date')));
  protected readonly search = signal('');

  protected readonly records = signal<QuarantineRecord[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  /** Registros filtrados por el texto de búsqueda, con los motivos separados. */
  protected readonly rows = computed<QuarantineRow[]>(() => {
    const term = this.search().trim().toLowerCase();

    return this.records()
      .filter(record =>
        !term ||
        [record.pcCodComRed, record.pcNomComRed, record.pcNumDoc, record.pcCiudad, record.motivo]
          .some(value => (value ?? '').toLowerCase().includes(term))
      )
      .map(record => ({ ...record, reasons: record.motivo.split(' | ') }));
  });

  constructor() {
    this.load();
  }

  protected onDateChange(event: Event): void {
    this.dateFilter.set((event.target as HTMLInputElement).value);
    this.load();
  }

  protected onSearch(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
  }

  protected clearFilters(): void {
    this.dateFilter.set('');
    this.search.set('');
    this.load();
  }

  /** Llama a GET /api/commerce/quarantine. */
  protected load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.api
      .getQuarantine(this.dateFilter() || undefined)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: records => this.records.set(records),
        error: error => {
          this.records.set([]);
          this.error.set(getErrorMessage(error));
        }
      });
  }
}