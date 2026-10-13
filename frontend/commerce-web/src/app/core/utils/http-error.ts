import { HttpErrorResponse } from '@angular/common/http';

/**
 * Convierte un error HTTP en un mensaje legible para el usuario.
 *
 * La API .NET responde los errores en formato ProblemDetails:
 *   - Validación de negocio:  { title, status, detail }
 *   - Validación de modelo:   { title, status, errors: { campo: [mensajes] } }
 */
export function getErrorMessage(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) {
    return 'Ocurrió un error inesperado.';
  }

  // status 0: la API no respondió (apagada, CORS o certificado no aceptado).
  if (error.status === 0) {
    return 'No se pudo conectar con la API. Verifique que el backend esté en ejecución.';
  }

  const body = error.error;

  if (body?.errors) {
    const messages = Object.values(body.errors as Record<string, string[]>).flat();
    if (messages.length > 0) {
      return messages.join(' ');
    }
  }

  return body?.detail ?? body?.title ?? `Error ${error.status}: ${error.statusText}`;
}