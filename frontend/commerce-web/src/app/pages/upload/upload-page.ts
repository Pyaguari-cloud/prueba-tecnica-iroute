import { Component, ElementRef, computed, inject, signal, viewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { CSV_COLUMNS, CommerceCsvRow, UploadResult } from '../../core/models/commerce.models';
import { CommerceApi } from '../../core/services/commerce-api';
import { CsvReader } from '../../core/services/csv-reader';
import { isValidDate, toIsoDate } from '../../core/utils/date-utils';
import { getErrorMessage } from '../../core/utils/http-error';

/** commerce_DDMMYYYY.csv */
const FILE_NAME_PATTERN = /^commerce_(\d{2})(\d{2})(\d{4})\.csv$/i;
/** Mismo límite que la API (10 MB). */
const MAX_FILE_SIZE = 10 * 1024 * 1024;
/** Cantidad máxima de filas que se dibujan en la tabla de previsualización. */
const PREVIEW_LIMIT = 100;

/**
 * Pantalla de Carga: permite elegir el archivo commerce_DDMMYYYY.csv,
 * previsualizarlo en una tabla y enviarlo a la API.
 */
@Component({
  selector: 'app-upload-page',
  imports: [RouterLink],
  templateUrl: './upload-page.html',
  styleUrl: './upload-page.scss'
})
export class UploadPage {
  private readonly api = inject(CommerceApi);
  private readonly csvReader = inject(CsvReader);
  private readonly fileInput = viewChild.required<ElementRef<HTMLInputElement>>('fileInput');

  protected readonly columns = CSV_COLUMNS;
  protected readonly previewLimit = PREVIEW_LIMIT;

  // ----- Estado (signals) -----
  protected readonly file = signal<File | null>(null);
  protected readonly rows = signal<CommerceCsvRow[]>([]);
  protected readonly reading = signal(false);
  protected readonly validationError = signal<string | null>(null);
  protected readonly uploading = signal(false);
  protected readonly uploadResult = signal<UploadResult | null>(null);
  protected readonly uploadError = signal<string | null>(null);
  protected readonly isDragging = signal(false);

  // ----- Valores derivados (computed) -----
  /** Solo se dibujan las primeras filas: la tabla sigue siendo fluida con archivos grandes. */
  protected readonly previewRows = computed(() => this.rows().slice(0, PREVIEW_LIMIT));

  /** Fechas de proceso distintas que trae el archivo (yyyy-MM-dd). */
  protected readonly processDates = computed(() =>
    [...new Set(this.rows().map(row => toIsoDate(row.pc_processdate)).filter(Boolean))]
  );

  /** Cantidad de celdas vacías (se resaltan en la tabla como aviso). */
  protected readonly emptyCellCount = computed(() =>
    this.rows().reduce(
      (total, row) => total + CSV_COLUMNS.filter(column => !row[column].trim()).length,
      0
    )
  );

  // ----- Eventos de la vista -----
  protected onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const selected = input.files?.[0];
    if (selected) {
      void this.loadFile(selected);
    }
  }

  protected onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(true);
  }

  protected onDragLeave(): void {
    this.isDragging.set(false);
  }

  protected onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(false);
    const dropped = event.dataTransfer?.files?.[0];
    if (dropped) {
      void this.loadFile(dropped);
    }
  }

  /** Envía el archivo a POST /api/commerce/upload. */
  protected upload(): void {
    const selected = this.file();
    if (!selected || this.uploading()) {
      return;
    }

    this.uploading.set(true);
    this.uploadError.set(null);

    this.api
      .upload(selected)
      .pipe(finalize(() => this.uploading.set(false)))
      .subscribe({
        next: result => this.uploadResult.set(result),
        error: error => this.uploadError.set(getErrorMessage(error))
      });
  }

  /** Limpia la pantalla para elegir otro archivo. */
  protected reset(): void {
    this.file.set(null);
    this.rows.set([]);
    this.validationError.set(null);
    this.uploadResult.set(null);
    this.uploadError.set(null);
    // Permite volver a elegir el mismo archivo.
    this.fileInput().nativeElement.value = '';
  }

  protected formatSize(bytes: number): string {
    return bytes < 1024 ? `${bytes} B` : `${(bytes / 1024).toFixed(1)} KB`;
  }

  // ----- Lógica interna -----
  /** Valida el archivo y lo lee para la previsualización. */
  private async loadFile(selected: File): Promise<void> {
    this.reset();
    this.file.set(selected);

    const error = this.validateFile(selected);
    if (error) {
      this.validationError.set(error);
      return;
    }

    this.reading.set(true);
    try {
      const { rows, missingColumns } = await this.csvReader.read(selected);

      if (missingColumns.length > 0) {
        this.validationError.set(`Al archivo le faltan las columnas: ${missingColumns.join(', ')}.`);
      } else if (rows.length === 0) {
        this.validationError.set('El archivo no contiene registros, solo el encabezado.');
      } else {
        this.rows.set(rows);
      }
    } catch {
      this.validationError.set('No se pudo leer el archivo. Verifique que sea un CSV válido.');
    } finally {
      this.reading.set(false);
    }
  }

  /** Mismas reglas que la API, para avisar al usuario antes de enviar. */
  private validateFile(selected: File): string | null {
    if (selected.size === 0) {
      return 'El archivo está vacío.';
    }

    if (selected.size > MAX_FILE_SIZE) {
      return 'El archivo supera el tamaño máximo permitido (10 MB).';
    }

    const match = FILE_NAME_PATTERN.exec(selected.name);
    if (!match || !isValidDate(+match[1], +match[2], +match[3])) {
      return `El nombre "${selected.name}" no es válido. Formato esperado: commerce_DDMMYYYY.csv`;
    }

    return null;
  }
}