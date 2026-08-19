import { Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';

@Component({
  selector: 'app-paginacao',
  template: `
  <div class="paginacao" *ngIf="total > 0">
    <div class="paginacao-info">
      Mostrando <strong>{{ inicio }}</strong>–<strong>{{ fim }}</strong> de <strong>{{ total }}</strong>
    </div>
    <div class="paginacao-controles">
      <label class="paginacao-tamanho">
        <span>Itens por página</span>
        <select class="form-select form-select-sm" [ngModel]="tamanho" (ngModelChange)="onTamanho($event)">
          <option *ngFor="let t of tamanhos" [ngValue]="t">{{ t }}</option>
        </select>
      </label>
      <div class="paginacao-nav">
        <button type="button" class="btn btn-outline-primary btn-sm" [disabled]="pagina <= 1" (click)="ir(1)" title="Primeira página" aria-label="Primeira página">«</button>
        <button type="button" class="btn btn-outline-primary btn-sm" [disabled]="pagina <= 1" (click)="ir(pagina - 1)" title="Página anterior" aria-label="Página anterior">‹</button>
        <span class="paginacao-pagina">Página {{ pagina }} de {{ totalPaginas }}</span>
        <button type="button" class="btn btn-outline-primary btn-sm" [disabled]="pagina >= totalPaginas" (click)="ir(pagina + 1)" title="Próxima página" aria-label="Próxima página">›</button>
        <button type="button" class="btn btn-outline-primary btn-sm" [disabled]="pagina >= totalPaginas" (click)="ir(totalPaginas)" title="Última página" aria-label="Última página">»</button>
      </div>
    </div>
  </div>
  `,
  styles: [`
    .paginacao {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      justify-content: space-between;
      gap: 12px;
      margin-top: 1rem;
      padding-top: 1rem;
      border-top: 1px solid var(--color-border);
    }
    .paginacao-info { color: var(--color-muted); font-size: 0.85rem; }
    .paginacao-controles { display: flex; flex-wrap: wrap; align-items: center; gap: 16px; }
    .paginacao-tamanho { display: flex; align-items: center; gap: 8px; margin: 0; color: var(--color-muted); font-size: 0.85rem; font-weight: 600; }
    .paginacao-tamanho select { width: auto; min-height: 38px; }
    .paginacao-nav { display: flex; align-items: center; gap: 6px; }
    .paginacao-nav .btn { min-height: 38px; min-width: 40px; padding: 0 10px; font-weight: 700; line-height: 1; }
    .paginacao-pagina { padding: 0 8px; color: var(--color-text); font-size: 0.85rem; font-weight: 600; white-space: nowrap; }
  `]
})
export class PaginacaoComponent implements OnChanges {
  @Input() total = 0;
  @Input() pagina = 1;
  @Input() tamanho = 10;
  @Output() paginaChange = new EventEmitter<number>();
  @Output() tamanhoChange = new EventEmitter<number>();

  readonly tamanhos = [10, 25, 50, 100, 500];

  get totalPaginas(): number { return Math.max(1, Math.ceil(this.total / this.tamanho)); }
  get inicio(): number { return this.total === 0 ? 0 : (this.pagina - 1) * this.tamanho + 1; }
  get fim(): number { return Math.min(this.pagina * this.tamanho, this.total); }

  ngOnChanges(): void {
    // Se a página atual ficou fora do intervalo (ex.: após excluir o último item da última página),
    // corrige para a última página válida sem quebrar o ciclo de detecção de mudanças.
    if (this.pagina > this.totalPaginas) {
      Promise.resolve().then(() => this.paginaChange.emit(this.totalPaginas));
    }
  }

  ir(pagina: number): void {
    const alvo = Math.min(Math.max(1, pagina), this.totalPaginas);
    if (alvo !== this.pagina) { this.paginaChange.emit(alvo); }
  }

  onTamanho(tamanho: number): void {
    this.tamanhoChange.emit(tamanho);
  }
}
