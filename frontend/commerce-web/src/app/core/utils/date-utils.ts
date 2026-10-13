/**
 * Utilidades de fechas compartidas por las pantallas.
 */

/** Indica si día/mes/año forman una fecha real (ej. 31/02/2026 no lo es). */
export function isValidDate(day: number, month: number, year: number): boolean {
  const date = new Date(year, month - 1, day);
  return date.getFullYear() === year && date.getMonth() === month - 1 && date.getDate() === day;
}

/**
 * Normaliza una fecha del CSV al formato ISO yyyy-MM-dd.
 * Acepta yyyy-MM-dd y dd/MM/yyyy (los mismos formatos que acepta la API).
 * Devuelve cadena vacía si el valor no es una fecha válida.
 */
export function toIsoDate(value: string | null | undefined): string {
  const text = (value ?? '').trim();

  const iso = /^(\d{4})-(\d{2})-(\d{2})$/.exec(text);
  if (iso) {
    const [, year, month, day] = iso;
    return isValidDate(+day, +month, +year) ? text : '';
  }

  const latin = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(text);
  if (latin) {
    const [, day, month, year] = latin;
    return isValidDate(+day, +month, +year) ? `${year}-${month}-${day}` : '';
  }

  return '';
}

/** Fecha de hoy en formato yyyy-MM-dd (hora local). */
export function todayIso(): string {
  const now = new Date();
  const month = String(now.getMonth() + 1).padStart(2, '0');
  const day = String(now.getDate()).padStart(2, '0');
  return `${now.getFullYear()}-${month}-${day}`;
}