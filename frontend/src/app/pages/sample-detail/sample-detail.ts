import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, input, numberAttribute, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { BehaviorSubject, catchError, combineLatest, map, of, switchMap } from 'rxjs';
import { SampleDetail } from '../../models/sample.models';
import { SampleService } from '../../services/sample.service';
import { apiErrorMessage, SpecRangePipe, StatusLabelPipe } from '../../shared/shared';

type DetailState = { sample: SampleDetail; error?: undefined } | { sample?: undefined; error: string };

@Component({
  selector: 'app-sample-detail',
  imports: [ReactiveFormsModule, RouterLink, DatePipe, DecimalPipe, StatusLabelPipe, SpecRangePipe],
  template: `
    @let s = state();
    @if (s?.error; as error) {
      <p class="alert">{{ error }}</p>
      <a routerLink="/samples">← Voltar</a>
    } @else if (s?.sample; as sample) {
      <div class="page-header">
        <div>
          <a routerLink="/samples" class="muted">← Amostras</a>
          <h1>{{ sample.code }} <span class="badge" [class]="sample.status">{{ sample.status | statusLabel }}</span></h1>
        </div>
        <div class="actions">
          <a [routerLink]="['/samples', sample.id, 'edit']" class="btn secondary">Editar</a>
          <button class="btn danger" (click)="remove(sample)">Excluir</button>
        </div>
      </div>

      <dl class="details">
        <div><dt>Produto</dt><dd>{{ sample.productName }}</dd></div>
        <div><dt>Lote</dt><dd>{{ sample.batchNumber }}</dd></div>
        <div><dt>Coleta</dt><dd>{{ sample.collectedAt | date: 'dd/MM/yyyy HH:mm' }}</dd></div>
        <div><dt>Observações</dt><dd>{{ sample.notes || '—' }}</dd></div>
      </dl>

      <h2>Resultados de análise</h2>
      <table>
        <thead>
          <tr><th>Parâmetro</th><th>Valor</th><th>Especificação</th><th>Situação</th><th>Analista</th><th>Data</th></tr>
        </thead>
        <tbody>
          @for (r of sample.results; track r.id) {
            <tr>
              <td>{{ r.parameter }}</td>
              <td [class.out]="!r.isWithinSpec">{{ r.value | number: '1.0-4' }} {{ r.unit }}</td>
              <td>{{ r.minLimit | specRange: r.maxLimit }} {{ r.unit }}</td>
              <td>
                <span class="badge" [class]="r.isWithinSpec ? 'Approved' : 'Rejected'">
                  {{ r.isWithinSpec ? 'Conforme' : 'Fora de espec.' }}
                </span>
              </td>
              <td>{{ r.analyst }}</td>
              <td>{{ r.analyzedAt | date: 'dd/MM/yyyy HH:mm' }}</td>
            </tr>
          } @empty {
            <tr><td colspan="6" class="muted">Nenhum resultado registrado. A amostra está pendente.</td></tr>
          }
        </tbody>
      </table>

      <h2>Registrar resultado</h2>
      <form [formGroup]="resultForm" (ngSubmit)="addResult(sample.id)" class="form compact">
        <label>Parâmetro <input formControlName="parameter" placeholder="pH" /></label>
        <label>Unidade <input formControlName="unit" placeholder="%, cP, ppm…" /></label>
        <label>Valor <input type="number" step="any" formControlName="value" /></label>
        <label>Mínimo <input type="number" step="any" formControlName="minLimit" /></label>
        <label>Máximo <input type="number" step="any" formControlName="maxLimit" /></label>
        <label>Analista <input formControlName="analyst" /></label>
        @if (formError()) {
          <p class="alert full">{{ formError() }}</p>
        }
        <div class="actions full">
          <button type="submit" class="btn" [disabled]="resultForm.invalid">Adicionar resultado</button>
        </div>
      </form>
    } @else {
      <p class="muted">Carregando…</p>
    }
  `,
})
export class SampleDetailPage {
  private readonly service = inject(SampleService);
  private readonly router = inject(Router);
  private readonly refresh$ = new BehaviorSubject<void>(undefined);

  /** Id vindo da rota (withComponentInputBinding). */
  readonly id = input.required({ transform: numberAttribute });

  protected readonly formError = signal('');

  /** Recarrega a amostra quando o id muda ou após registrar um resultado. */
  protected readonly state = toSignal(
    combineLatest([toObservable(this.id), this.refresh$]).pipe(
      switchMap(([id]) =>
        this.service.getById(id).pipe(
          map((sample): DetailState => ({ sample })),
          catchError((err) => of<DetailState>({ error: apiErrorMessage(err) })),
        ),
      ),
    ),
  );

  protected readonly resultForm = inject(FormBuilder).group({
    parameter: ['', [Validators.required, Validators.maxLength(50)]],
    unit: [''],
    value: [null as number | null, Validators.required],
    minLimit: [null as number | null],
    maxLimit: [null as number | null],
    analyst: ['', [Validators.required, Validators.maxLength(100)]],
  });

  protected addResult(sampleId: number): void {
    const v = this.resultForm.getRawValue();
    this.formError.set('');
    this.service
      .addResult(sampleId, {
        parameter: v.parameter ?? '',
        unit: v.unit ?? '',
        value: v.value!,
        minLimit: v.minLimit,
        maxLimit: v.maxLimit,
        analyst: v.analyst ?? '',
      })
      .subscribe({
        next: () => {
          // Mantém o analista preenchido para agilizar lançamentos seguidos.
          this.resultForm.reset({ analyst: v.analyst });
          this.refresh$.next();
        },
        error: (err) => this.formError.set(apiErrorMessage(err)),
      });
  }

  protected remove(sample: SampleDetail): void {
    if (!confirm(`Excluir a amostra ${sample.code} e todos os seus resultados?`)) return;
    this.service.delete(sample.id).subscribe({
      next: () => this.router.navigate(['/samples']),
      error: (err) => this.formError.set(apiErrorMessage(err)),
    });
  }
}
