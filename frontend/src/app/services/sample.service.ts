import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  AddResultRequest,
  AnalysisResult,
  Dashboard,
  SampleDetail,
  SampleRequest,
  SampleStatus,
  SampleSummary,
} from '../models/sample.models';

/**
 * Comunicação com a API REST do LabTrack.
 * Em desenvolvimento, "/api" é redirecionado para o back-end via proxy.conf.json;
 * no Docker, o Nginx faz o proxy reverso.
 */
@Injectable({ providedIn: 'root' })
export class SampleService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api';

  search(search = '', status: SampleStatus | '' = ''): Observable<SampleSummary[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (status) params = params.set('status', status);
    return this.http.get<SampleSummary[]>(`${this.baseUrl}/samples`, { params });
  }

  getById(id: number): Observable<SampleDetail> {
    return this.http.get<SampleDetail>(`${this.baseUrl}/samples/${id}`);
  }

  create(request: SampleRequest): Observable<SampleDetail> {
    return this.http.post<SampleDetail>(`${this.baseUrl}/samples`, request);
  }

  update(id: number, request: SampleRequest): Observable<SampleDetail> {
    return this.http.put<SampleDetail>(`${this.baseUrl}/samples/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/samples/${id}`);
  }

  addResult(sampleId: number, request: AddResultRequest): Observable<AnalysisResult> {
    return this.http.post<AnalysisResult>(`${this.baseUrl}/samples/${sampleId}/results`, request);
  }

  getDashboard(): Observable<Dashboard> {
    return this.http.get<Dashboard>(`${this.baseUrl}/dashboard`);
  }
}
