import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../core/api.service';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-envio-nf',
  templateUrl: './envio-nf.component.html'
})
export class EnvioNfComponent implements OnInit {
  liberacoes: any[] = [];
  selecionada: any | null = null;
  numeroNotaFiscal = '';
  numeroAf = '';
  observacao = '';
  arquivoNotaFiscal: File | null = null;
  boletoSelecionado: any | null = null;
  arquivoBoleto: File | null = null;
  attempted = false;
  attemptedBoleto = false;
  nfTocada = false;
  afTocada = false;
  nfArquivoTocado = false;
  boletoArquivoTocado = false;
  carregando = false;
  enviando = false;
  enviandoBoleto = false;
  checklist: Array<{ codigo?: string; titulo: string; ok: boolean; valorEncontrado?: string | null }> = [];

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.carregando = true;
    this.selecionada = null;
    this.boletoSelecionado = null;
    this.api.get<any[]>('/notas-fiscais/minhas-liberacoes').subscribe({
      next: (res) => {
        this.liberacoes = res;
        this.carregando = false;
      },
      error: () => {
        this.liberacoes = [];
        this.carregando = false;
      }
    });
  }

  selecionar(item: any): void {
    this.selecionada = item;
    this.numeroNotaFiscal = item.numeroNotaFiscal || '';
    this.numeroAf = item.numeroAf || '';
    this.observacao = '';
    this.arquivoNotaFiscal = null;
    this.attempted = false;
    this.nfTocada = false;
    this.afTocada = false;
    this.nfArquivoTocado = false;
    this.checklist = [];
  }

  selecionarBoleto(item: any): void {
    this.boletoSelecionado = item;
    this.observacao = '';
    this.arquivoBoleto = null;
    this.attemptedBoleto = false;
    this.boletoArquivoTocado = false;
  }

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.arquivoNotaFiscal = input.files?.[0] ?? null;
    this.nfArquivoTocado = true;
    this.checklist = [];
  }

  onBoletoFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.arquivoBoleto = input.files?.[0] ?? null;
    this.boletoArquivoTocado = true;
  }

  enviar(): void {
    this.attempted = true;
    this.checklist = [];
    if (!this.selecionada || !this.numeroNotaFiscal.trim() || !this.numeroAf.trim() || !this.arquivoNotaFiscal) {
      this.checklist = this.buildChecklistLocal();
      return;
    }

    const formData = new FormData();
    formData.append('numeroNotaFiscal', this.numeroNotaFiscal.trim());
    formData.append('numeroAf', this.numeroAf.trim());
    formData.append('observacao', this.observacao || '');
    formData.append('arquivoNotaFiscal', this.arquivoNotaFiscal);

    this.enviando = true;
    fetch(`${environment.apiUrl}/notas-fiscais/liberacoes/${this.selecionada.id}/envio`, {
      method: 'PUT',
      headers: { Authorization: `Bearer ${localStorage.getItem('dalba_auth') ? JSON.parse(localStorage.getItem('dalba_auth') as string).token : ''}` },
      body: formData
    }).then(async (response) => {
      const body = await response.json().catch(() => null);
      if (body?.checklist) {
        this.checklist = body.checklist.map((item: any) => ({
          codigo: item.codigo,
          titulo: item.titulo,
          ok: item.ok,
          valorEncontrado: item.valorEncontrado
        }));
      }
      this.enviando = false;
      if (!response.ok) {
        if (!this.checklist.length) {
          this.checklist = this.buildChecklistLocal();
        }
        return;
      }
      this.load();
    }).catch(() => {
      this.checklist = this.buildChecklistLocal();
      this.enviando = false;
    });
  }

  enviarBoleto(): void {
    this.attemptedBoleto = true;
    if (!this.boletoSelecionado || !this.arquivoBoleto) return;

    const formData = new FormData();
    formData.append('observacao', this.observacao || '');
    formData.append('arquivoBoleto', this.arquivoBoleto);

    this.enviandoBoleto = true;
    fetch(`${environment.apiUrl}/notas-fiscais/liberacoes/${this.boletoSelecionado.id}/boleto`, {
      method: 'PUT',
      headers: { Authorization: `Bearer ${localStorage.getItem('dalba_auth') ? JSON.parse(localStorage.getItem('dalba_auth') as string).token : ''}` },
      body: formData
    }).then((response) => {
      this.enviandoBoleto = false;
      if (!response.ok) {
        throw new Error('Falha ao enviar boleto.');
      }
      this.load();
    }).catch(() => {
      this.enviandoBoleto = false;
    });
  }

  podeEnviar(item: any): boolean {
    return item.statusFinanceiro === 'AguardandoEnvioNf';
  }

  podeEnviarBoleto(item: any): boolean {
    return item.statusFinanceiro === 'AguardandoPagamento';
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

  private buildChecklistLocal(): Array<{ codigo: string; titulo: string; ok: boolean }> {
    return [
      { codigo: 'NF_NUMERO', titulo: 'Número da NF', ok: false },
      { codigo: 'AF_EXISTE', titulo: 'AF existe na NF', ok: false },
      { codigo: 'AF_IGUAL', titulo: 'Número da AF igual a AF da NF', ok: false },
      { codigo: 'CNPJ_CONFERE', titulo: 'CNPJ confere', ok: false },
      { codigo: 'CHAVE_ACESSO', titulo: 'Chave de Acesso Existe', ok: false }
    ];
  }
}
