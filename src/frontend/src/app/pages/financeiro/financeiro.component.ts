import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-financeiro',
  templateUrl: './financeiro.component.html'
})
export class FinanceiroComponent implements OnInit {
  readonly hoje = new Date();
  filtro = {
    mesReferencia: this.hoje.getMonth() + 1,
    anoReferencia: this.hoje.getFullYear()
  };

  registros: any[] = [];
  selecionado: any | null = null;
  statusFinanceiro = 'EmAnaliseFinanceira';
  numeroNotaFiscal = '';
  observacao = '';
  attempted = false;
  carregando = false;

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.carregando = true;
    this.selecionado = null;
    this.api
      .get<any[]>(`/financeiro/liberacoes?mesReferencia=${this.filtro.mesReferencia}&anoReferencia=${this.filtro.anoReferencia}`)
      .subscribe({
        next: (res) => {
          this.registros = res;
          this.carregando = false;
        },
        error: () => {
          this.registros = [];
          this.carregando = false;
        }
      });
  }

  editar(item: any): void {
    this.selecionado = item;
    this.statusFinanceiro = item.statusFinanceiro;
    this.numeroNotaFiscal = item.numeroNotaFiscal || '';
    this.observacao = '';
    this.attempted = false;
  }

  salvar(): void {
    this.attempted = true;
    if (!this.selecionado || !this.statusFinanceiro) return;

    this.api.put(`/financeiro/liberacoes/${this.selecionado.id}`, {
      statusFinanceiro: this.statusFinanceiro,
      numeroNotaFiscal: this.numeroNotaFiscal || null,
      observacao: this.observacao || null
    }).subscribe(() => this.load());
  }

  statusLabel(status: string): string {
    const labels: Record<string, string> = {
      AguardandoEnvioNf: 'Aguardando envio de NF',
      AguardandoPagamento: 'Aguardando pagamento',
      EmAnaliseFinanceira: 'Em análise financeira',
      LiberadoParaPagamento: 'Liberado para pagamento',
      Pago: 'Pago'
    };

    return labels[status] ?? status;
  }
}
