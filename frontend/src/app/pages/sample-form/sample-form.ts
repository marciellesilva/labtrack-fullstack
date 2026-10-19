import { Component, inject, input, numberAttribute, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { SampleRequest } from '../../models/sample.models';
import { SampleService } from '../../services/sample.service';
import { apiErrorMessage } from '../../shared/shared';

/** Formulário de criação/edição de amostra (rota /samples/new ou /samples/:id/edit). */
@Component({
  selector: 'app-sample-form',
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <h1>{{ id() ? 'Editar amostra' : 'Nova amostra' }}</h1>

    <form [formGroup]="form" (ngSubmit)="save()" class="form">
      <label>
        Código
        <input formControlName="code" placeholder="AM-2026-0005" />
        @if (form.controls.code.touched && form.controls.code.invalid) {
          <small class="error">Use 3 a 20 caracteres: letras maiúsculas, números e hífen.</small>
        }
      </label>
      <label>
        Produto
        <input formControlName="productName" placeholder="Soda Cáustica 50%" />
      </label>
      <label>
        Lote
        <input formControlName="batchNumber" placeholder="L-24070" />
      </label>
      <label>
        Data da coleta
        <input type="datetime-local" formControlName="collectedAt" />
      </label>
      <label class="full">
        Observações
        <textarea formControlName="notes" rows="3" placeholder="Ponto de coleta, condições do processo…"></textarea>
      </label>

      @if (error()) {
        <p class="alert full">{{ error() }}</p>
      }

      <div class="actions full">
        <a [routerLink]="id() ? ['/samples', id()] : ['/samples']" class="btn secondary">Cancelar</a>
        <button type="submit" class="btn" [disabled]="form.invalid || saving()">
          {{ saving() ? 'Salvando…' : 'Salvar' }}
        </button>
      </div>
    </form>
  `,
})
export class SampleFormPage implements OnInit {
  private readonly service = inject(SampleService);
  private readonly router = inject(Router);

  /** Preenchido pelo router (withComponentInputBinding) quando em modo edição. */
  readonly id = input(0, { transform: numberAttribute });

  protected readonly saving = signal(false);
  protected readonly error = signal('');

  protected readonly form = inject(FormBuilder).nonNullable.group({
    code: ['', [Validators.required, Validators.pattern(/^[A-Z0-9-]{3,20}$/)]],
    productName: ['', [Validators.required, Validators.maxLength(100)]],
    batchNumber: ['', [Validators.required, Validators.maxLength(30)]],
    collectedAt: [toLocalInput(new Date()), Validators.required],
    notes: [''],
  });

  ngOnInit(): void {
    if (!this.id()) return;
    this.service.getById(this.id()).subscribe({
      next: (s) =>
        this.form.setValue({
          code: s.code,
          productName: s.productName,
          batchNumber: s.batchNumber,
          collectedAt: toLocalInput(new Date(s.collectedAt)),
          notes: s.notes ?? '',
        }),
      error: (err) => this.error.set(apiErrorMessage(err)),
    });
  }

  protected save(): void {
    if (this.form.invalid) return;
    this.saving.set(true);
    this.error.set('');

    const value = this.form.getRawValue();
    const request: SampleRequest = {
      ...value,
      code: value.code.toUpperCase(),
      collectedAt: new Date(value.collectedAt).toISOString(),
      notes: value.notes || null,
    };

    const save$ = this.id() ? this.service.update(this.id(), request) : this.service.create(request);
    save$.subscribe({
      next: (sample) => this.router.navigate(['/samples', sample.id]),
      error: (err) => {
        this.error.set(apiErrorMessage(err));
        this.saving.set(false);
      },
    });
  }
}

/** Converte Date para o formato aceito por <input type="datetime-local">. */
function toLocalInput(date: Date): string {
  const local = new Date(date.getTime() - date.getTimezoneOffset() * 60000);
  return local.toISOString().slice(0, 16);
}
