/**
 * Modelos (tipos) compartidos por la aplicación.
 * Reflejan el CSV y las respuestas JSON de la API .NET.
 */

/** Columnas obligatorias del archivo commerce_DDMMYYYY.csv. */
export const CSV_COLUMNS = [
  'pc_processdate',
  'pc_codcomred',
  'pc_nomcomred',
  'pc_tipdoc',
  'pc_numdoc',
  'pc_ciudad'
] as const;

/** Nombre de una columna del CSV. */
export type CsvColumn = (typeof CSV_COLUMNS)[number];

/** Una fila del CSV tal como se lee en el navegador (previsualización). */
export type CommerceCsvRow = Record<CsvColumn, string>;

/** Respuesta de POST /api/commerce/upload. */
export interface UploadResult {
  fileName: string;
  insertedCount: number;
}

/** Respuesta de POST /api/commerce/process. */
export interface ProcessResult {
  processDate: string;
  quarantinedCount: number;
}

/** Registro de GET /api/commerce/quarantine. */
export interface QuarantineRecord {
  id: number;
  commerceId: number;
  pcProcessDate: string;
  pcCodComRed: string | null;
  pcNomComRed: string | null;
  pcTipDoc: string | null;
  pcNumDoc: string | null;
  pcCiudad: string | null;
  motivo: string;
  quarantinedAt: string;
}