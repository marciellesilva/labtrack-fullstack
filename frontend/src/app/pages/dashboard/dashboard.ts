import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { catchError, map, of, startWith } from 'rxjs';
import { Dashboard } from '../../models/sample.models';
import { SampleService } from '../../services/sample.service';
import { apiErrorMessage, SpecRangePipe } from '../../shared/shared';

type DashboardState =
  | { loading: true }
  | { loading: false; data: Dashboard }
  | { loading: false; error: string };

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, DatePipe, DecimalPipe, SpecRangePipe],
  template: `
    <h1>Painel de Qualidade</h1>

    @let s = state();
    @if (s.loading) {
      <p class="muted">Carregando indicadores…</p>
    } @else if ('error' in s) {
      <p class="alert">{{ s.error }}</p>
    } @else {
      <section class="cards">
        <div class="card"><span class="card-label">Amostras</span><strong>{{ s.data.totalSamples }}</strong></div>
        <div class="card ok"><span class="card-label">Aprovadas</span><strong>{{ s.data.approvedSamples }}</strong></div>
        <div class="card bad"><span class="card-label">Reprovadas</span><strong>{{ s.data.rejectedSamples }}</strong></div>
        <div class="card warn"><span class="card-label">Pendentes</span><strong>{{ s.data.pendingSamples }}</strong></div>
        <div class="card">
          <span class="card-label">Taxa de aprovação</span>
          <strong>{{ s.data.approvalRate | number: '1.0-1' }}%</strong>
        </div>
        <div class="card">
          <span class="card-label">Resultados fora de espec.</span>
          <strong>{{ s.data.outOfSpecResults }} / {{ s.data.totalResults }}</strong>
        </div>
      </section>

      <h2>Últimos resultados fora de especificação</h2>
      @if (s.data.recentOutOfSpec.length === 0) {
        <p class="muted">Nenhum resultado fora de especificação. 🎉</p>
      } @else {
        <table>
          <thead>
            <tr><th>Amostra</th><th>Produto</th><th>Parâmetro</th><th>Valor</th><th>Especificação</th><th>Data</th></tr>
          </thead>
          <tbody>
            @for (item of s.data.recentOutOfSpec; track item.sampleId + item.parameter + item.analyzedAt) {
              <tr>
                <td><a [routerLink]="['/samples', item.sampleId]">{{ item.sampleCode }}</a></td>
                <td>{{ item.productName }}</td>
                <td>{{ item.parameter }}</td>
                <td class="out">{{ item.value | number: '1.0-4' }} {{ item.unit }}</td>
                <td>{{ item.minLimit | specRange: item.maxLimit }}</td>
                <td>{{ item.analyzedAt | date: 'dd/MM/yyyy HH:mm' }}</td>
              </tr>
            }
          </tbody>
        </table>
      }
    }
  `,
})
export class DashboardPage {
  private readonly service = inject(SampleService);

  protected readonly state = toSignal(
    this.service.getDashboard().pipe(
      map((data): DashboardState => ({ loading: false, data })),
      catchError((err) => of<DashboardState>({ loading: false, error: apiErrorMessage(err) })),
      startWith<DashboardState>({ loading: true }),
    ),
    { requireSync: true },
  );
}
