import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { ProcessResult } from '../../core/models/commerce.models';
import { CommerceApi } from '../../core/services/commerce-api';
import { toIsoDate, todayIso } from '../../core/utils/date-utils';
import { getErrorMessage } from '../../core/utils/http-error';

/**
 * Pantalla de Procesamiento: envía la fecha (pc_processdate) a la API para
 * validar los registros cargados y mover los inválidos a cuarentena.
 */
@Component({
  selector: 'app-process-page',
  imports: [RouterLink],
  templateUrl: './process-page.html',
  styleUrl: './process-page.scss'
})
export class ProcessPage {
  private readonly api = inject(CommerceApi);
  private readonly route = inject(ActivatedRoute);

  /**
   * Fecha a procesar. Si se llega desde la pantalla de Carga, viene en la URL
   * (?date=yyyy-MM-dd); si no, se propone la fecha de hoy.
   */
  protected readonly processDate = signal(
    toIsoDate(this.route.snapshot.queryParamMap.get('date')) || todayIso()
  );

  protected readonly processing = signal(false);
  protected readonly result = signal<ProcessResult | null>(null);
  protected readonly error = signal<string | null>(null);

  protected onDateChange(event: Event): void {
    this.processDate.set((event.target as HTMLInputElement).value);
    this.result.set(null);
    this.error.set(null);
  }

  /** Llama a POST /api/commerce/process. */
  protected process(): void {
    const date = this.processDate();
    if (!date || this.processing()) {
      return;
    }

    this.processing.set(true);
    this.result.set(null);
    this.error.set(null);

    this.api
      .process(date)
      .pipe(finalize(() => this.processing.set(false)))
      .subscribe({
        next: result => this.result.set(result),
        error: error => this.error.set(getErrorMessage(error))
      });
  }
}