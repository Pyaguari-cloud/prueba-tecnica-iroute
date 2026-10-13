import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ProcessResult, QuarantineRecord, UploadResult } from '../models/commerce.models';

/**
 * Servicio que centraliza las llamadas HTTP a la API de comercios.
 * Los componentes nunca usan HttpClient directamente: siempre pasan por aquí
 * (equivalente a un custom hook o un módulo "api.ts" en React).
 */
@Injectable({ providedIn: 'root' })
export class CommerceApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/commerce`;

  /** POST /api/commerce/upload — envía el CSV como multipart/form-data. */
  upload(file: File): Observable<UploadResult> {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return this.http.post<UploadResult>(`${this.baseUrl}/upload`, formData);
  }

  /** POST /api/commerce/process — procesa la fecha indicada (yyyy-MM-dd). */
  process(processDate: string): Observable<ProcessResult> {
    return this.http.post<ProcessResult>(`${this.baseUrl}/process`, { processDate });
  }

  /** GET /api/commerce/quarantine — lista la cuarentena, opcionalmente filtrada por fecha. */
  getQuarantine(processDate?: string): Observable<QuarantineRecord[]> {
    let params = new HttpParams();
    if (processDate) {
      params = params.set('processDate', processDate);
    }

    return this.http.get<QuarantineRecord[]>(`${this.baseUrl}/quarantine`, { params });
  }
}