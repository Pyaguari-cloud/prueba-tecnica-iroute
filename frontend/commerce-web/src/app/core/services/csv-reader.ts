import { Injectable } from '@angular/core';
import Papa from 'papaparse';

import { CSV_COLUMNS, CommerceCsvRow } from '../models/commerce.models';

/** Resultado de leer el CSV en el navegador. */
export interface CsvReadResult {
  /** Filas del archivo (sin el encabezado). */
  rows: CommerceCsvRow[];
  /** Columnas obligatorias que no vienen en el encabezado. */
  missingColumns: string[];
}

/**
 * Lee el archivo CSV en el navegador (sin enviarlo al servidor)
 * para poder previsualizarlo antes de la carga.
 * Usa PapaParse, que detecta automáticamente el separador (',' o ';')
 * y maneja correctamente comillas, tildes y saltos de línea.
 */
@Injectable({ providedIn: 'root' })
export class CsvReader {
  read(file: File): Promise<CsvReadResult> {
    return new Promise((resolve, reject) => {
      Papa.parse<Record<string, string | undefined>>(file, {
        header: true,
        skipEmptyLines: 'greedy',
        // Ignora mayúsculas y espacios en los nombres de columna (igual que la API).
        transformHeader: header => header.trim().toLowerCase(),
        complete: results => {
          const headers = results.meta.fields ?? [];
          const missingColumns = CSV_COLUMNS.filter(column => !headers.includes(column));

          // Se normaliza cada fila: solo las columnas esperadas y nunca undefined.
          const rows = results.data.map(raw => {
            const row = {} as CommerceCsvRow;
            for (const column of CSV_COLUMNS) {
              row[column] = raw[column] ?? '';
            }
            return row;
          });

          resolve({ rows, missingColumns });
        },
        error: error => reject(error)
      });
    });
  }
}