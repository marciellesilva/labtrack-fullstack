import { DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  catchError,
  combineLatest,
  debounceTime,
  distinctUntilChanged,
  map,
  of,
  startWith,
  switchMap,
} from 'rxjs';
import { SampleStatus, SampleSummary } from '../../models/sample.models';
import { SampleService } from '../../services/sample.service';
import { apiErrorMessage, StatusLabelPipe } from '../../shared/shared';

interface ListState {
  loading: boolean;
  samples: SampleSummary[];
  error?: string;
}

@Component({
  selector: 'app-sample-list',
  imports: [ReactiveFormsModule, RouterLink, DatePipe, StatusLabelPipe],
  template: `
    <div class="page-header">
      <h1>Amostras</h1>
      <a routerLink="/samples/new" class="btn">+ Nova amostra</a>
    </div>

    <div class="filters">
      <input type="search" [formControl]="search" placeholder="Buscar por código, produto ou lote…" aria-label="Buscar" />
      <select [formControl]="status" aria-label="Filtrar por status">
        <option value="">Todos os status</option>
        <option value="Pending">Pendente</option>
        <option value="Approved">Aprovada</option>
        <option value="Rejected">Reprovada</option>
      </select>
    </div>

    @let s = state();
    @if (s.error) {
      <p class="alert">{{ s.error }}</p>
    }
    <table [class.loading]="s.loading">
      <thead>
        <tr><th>Código</th><th>Produto</th><th>Lote</th><th>Coleta</th><th>Resultados</th><th>Status</th></tr>
      </thead>
      <tbody>
        @for (sample of s.samples; track sample.id) {
          <tr>
            <td><a [routerLink]="['/samples', sample.id]">{{ sample.code }}</a></td>
            <td>{{ sample.productName }}</td>
            <td>{{ sample.batchNumber }}</td>
            <td>{{ sample.collectedAt | date: 'dd/MM/yyyy' }}</td>
            <td>
              {{ sample.resultsCount }}
              @if (sample.outOfSpecCount > 0) {
                <span class="out">({{ sample.outOfSpecCount }} fora)</span>
              }
            </td>
            <td><span class="badge" [class]="sample.status">{{ sample.status | statusLabel }}</span></td>
          </tr>
        } @empty {
          <tr><td colspan="6" class="muted">{{ s.loading ? 'Carregando…' : 'Nenhuma amostra encontrada.' }}</td></tr>
        }
      </tbody>
    </table>
  `,
})
export class SampleListPage {
  private readonly service = inject(SampleService);

  protected readonly search = new FormControl('', { nonNullable: true });
  protected readonly status = new FormControl<SampleStatus | ''>('', { nonNullable: true });

  /**
   * Busca reativa: espera o usuário parar de digitar (debounceTime),
   * ignora valores repetidos e cancela a requisição anterior (switchMap).
   */
  protected readonly state = toSignal(
    combineLatest([
      this.search.valueChanges.pipe(
        startWith(''),
        debounceTime(300),
        map((v) => v.trim()),
        distinctUntilChanged(),
      ),
      this.status.valueChanges.pipe(startWith<SampleStatus | ''>('')),
    ]).pipe(
      switchMap(([term, status]) =>
        this.service.search(term, status).pipe(
          map((samples): ListState => ({ loading: false, samples })),
          catchError((err) => of<ListState>({ loading: false, samples: [], error: apiErrorMessage(err) })),
          startWith<ListState>({ loading: true, samples: [] }),
        ),
      ),
    ),
    { initialValue: { loading: true, samples: [] } as ListState },
  );
}
