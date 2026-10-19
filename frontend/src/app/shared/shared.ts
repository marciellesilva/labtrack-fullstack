import { HttpErrorResponse } from '@angular/common/http';
import { Pipe, PipeTransform } from '@angular/core';
import { SampleStatus, STATUS_LABELS } from '../models/sample.models';

/** Converte o status da API para o rótulo em português. */
@Pipe({ name: 'statusLabel' })
export class StatusLabelPipe implements PipeTransform {
  transform(status: SampleStatus): string {
    return STATUS_LABELS[status] ?? status;
  }
}

/** Formata a faixa de especificação, ex.: "6,5 – 7,5", "≤ 50", "≥ 99,5". */
@Pipe({ name: 'specRange' })
export class SpecRangePipe implements PipeTransform {
  transform(min: number | null, max: number | null): string {
    const fmt = (v: number) => v.toLocaleString('pt-BR');
    if (min != null && max != null) return `${fmt(min)} – ${fmt(max)}`;
    if (min != null) return `≥ ${fmt(min)}`;
    if (max != null) return `≤ ${fmt(max)}`;
    return '—';
  }
}

/** Extrai a mensagem amigável de um ProblemDetails retornado pela API. */
export function apiErrorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    if (error.status === 0) return 'Não foi possível conectar à API.';
    const body = error.error;
    if (body?.errors) return Object.values(body.errors).flat().join(' ');
    if (body?.detail) return body.detail;
    if (body?.title) return body.title;
  }
  return 'Ocorreu um erro inesperado.';
}
