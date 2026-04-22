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
  observacao = '';
  arquivoNotaFiscal: File | null = null;
  attempted = false;
  carregando = false;
  enviando = false;

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.carregando = true;
    this.selecionada = null;
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
    this.observacao = '';
    this.arquivoNotaFiscal = null;
    this.attempted = false;
  }

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.arquivoNotaFiscal = input.files?.[0] ?? null;
  }

  enviar(): void {
    this.attempted = true;
    if (!this.selecionada || !this.numeroNotaFiscal.trim() || !this.arquivoNotaFiscal) return;

    const formData = new FormData();
    formData.append('numeroNotaFiscal', this.numeroNotaFiscal.trim());
    formData.append('observacao', this.observacao || '');
    formData.append('arquivoNotaFiscal', this.arquivoNotaFiscal);

    this.enviando = true;
    fetch(`${environment.apiUrl}/notas-fiscais/liberacoes/${this.selecionada.id}/envio`, {
      method: 'PUT',
      headers: { Authorization: `Bearer ${localStorage.getItem('dalba_auth') ? JSON.parse(localStorage.getItem('dalba_auth') as string).token : ''}` },
      body: formData
    }).then((response) => {
      this.enviando = false;
      if (!response.ok) {
        throw new Error('Falha ao enviar nota fiscal.');
      }
      this.load();
    }).catch(() => {
      this.enviando = false;
    });
  }

  podeEnviar(item: any): boolean {
    return item.statusFinanceiro === 'AguardandoEnvioNf';
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
