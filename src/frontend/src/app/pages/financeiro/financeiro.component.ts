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
    anoReferencia: this.hoje.getFullYear(),
    statusFinanceiro: ''
  };

  registros: any[] = [];
  selecionado: any | null = null;
  statusFinanceiro = 'EmAnaliseFinanceira';
  numeroNotaFiscal = '';
  observacao = '';
  attempted = false;
  filtroTocado = false;
  statusTocado = false;
  carregando = false;

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.filtroTocado = true;
    if (!this.filtroValido) return;

    this.carregando = true;
    this.selecionado = null;
    this.api
      .get<any[]>(`/financeiro/liberacoes?${this.queryString()}`)
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
    this.statusTocado = false;
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

  exportarExcel(): void {
    this.download(`/financeiro/liberacoes/exportacao-excel?${this.queryString()}`, `dalba-custos-${this.filtro.anoReferencia}-${String(this.filtro.mesReferencia).padStart(2, '0')}.xls`);
  }

  exportarPdf(): void {
    this.download(`/financeiro/liberacoes/exportacao-pdf?${this.queryString()}`, `dalba-custos-${this.filtro.anoReferencia}-${String(this.filtro.mesReferencia).padStart(2, '0')}.pdf`);
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

  get mesInvalido(): boolean {
    const mes = Number(this.filtro.mesReferencia);
    return !Number.isInteger(mes) || mes < 1 || mes > 12;
  }

  get anoInvalido(): boolean {
    const ano = Number(this.filtro.anoReferencia);
    return !Number.isInteger(ano) || ano < 2020 || ano > 2100;
  }

  get filtroValido(): boolean {
    return !this.mesInvalido && !this.anoInvalido;
  }

  private queryString(): string {
    const params = new URLSearchParams({
      mesReferencia: String(this.filtro.mesReferencia),
      anoReferencia: String(this.filtro.anoReferencia)
    });
    if (this.filtro.statusFinanceiro) params.set('statusFinanceiro', this.filtro.statusFinanceiro);
    return params.toString();
  }

  private download(path: string, fileName: string): void {
    this.api.getBlob(path).subscribe((blob) => {
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = fileName;
      link.click();
      window.URL.revokeObjectURL(url);
    });
  }
}
