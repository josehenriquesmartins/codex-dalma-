import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-admin-validacao',
  templateUrl: './admin-validacao.component.html'
})
export class AdminValidacaoComponent implements OnInit, OnDestroy {
  @ViewChild('pdfContainer') pdfContainer?: ElementRef<HTMLDivElement>;

  readonly hoje = new Date();

  filtro = {
    mesReferencia: this.hoje.getMonth() + 1,
    anoReferencia: this.hoje.getFullYear()
  };

  envios: any[] = [];
  envioSelecionado: any | null = null;
  documentoVisualizado: any | null = null;
  visualizadorUrl: SafeResourceUrl | null = null;
  visualizadorObjectUrl: string | null = null;
  visualizadorTipo = '';
  visualizadorBlob: Blob | null = null;
  visualizadorErro = '';
  selectedEnvioId: number | null = null;
  carregandoLista = false;
  carregandoDetalhe = false;
  processandoDocumentoId: number | null = null;
  acaoProcessando: string | null = null;
  filtroTocado = false;

  constructor(private readonly api: ApiService, private readonly sanitizer: DomSanitizer) {}

  ngOnInit(): void {
    this.buscarEnvios();
  }

  ngOnDestroy(): void {
    this.fecharVisualizador();
  }

  buscarEnvios(limparSelecao = true): void {
    this.filtroTocado = true;
    if (!this.filtro.mesReferencia || !this.filtro.anoReferencia) return;

    this.carregandoLista = true;
    if (limparSelecao) {
      this.envioSelecionado = null;
      this.selectedEnvioId = null;
    }

    this.api
      .get<any[]>(`/admin/envios?mesReferencia=${this.filtro.mesReferencia}&anoReferencia=${this.filtro.anoReferencia}`)
      .subscribe({
        next: (res) => {
          this.envios = res;
          this.carregandoLista = false;
        },
        error: () => {
          this.envios = [];
          this.carregandoLista = false;
        }
      });
  }

  selecionarEnvio(envioId: number): void {
    this.selectedEnvioId = envioId;
    this.carregandoDetalhe = true;

    this.api.get<any>(`/admin/envios/${envioId}`).subscribe({
      next: (res) => {
        this.envioSelecionado = res;
        this.fecharVisualizador();
        this.carregandoDetalhe = false;
      },
      error: () => {
        this.envioSelecionado = null;
        this.fecharVisualizador();
        this.carregandoDetalhe = false;
      }
    });
  }

  validarDocumento(documento: any, status: string): void {
    const observacao = (documento.observacaoRascunho || '').trim();
    documento.observacaoErro = '';

    if (status === 'Reprovado' && !observacao) {
      documento.observacaoErro = 'Informe a justificativa para reprovar.';
      return;
    }

    this.processandoDocumentoId = documento.id;
    this.acaoProcessando = status;

    this.api.put(`/admin/documentos-registrados/${documento.id}/validacao`, {
      status,
      observacaoAvaliacao: observacao || null
    }).subscribe({
      next: () => {
        const envioId = this.selectedEnvioId;
        if (!envioId) {
          this.finalizarProcessamentoValidacao();
          this.buscarEnvios(false);
          return;
        }

        this.api.get<any>(`/admin/envios/${envioId}`).subscribe({
          next: (res) => {
            this.atualizarStatusLista(envioId, res.status);
            if (res.status === 'EmConformidade') {
              this.envioSelecionado = null;
              this.selectedEnvioId = null;
              this.fecharVisualizador();
            } else {
              this.envioSelecionado = res;
            }

            this.finalizarProcessamentoValidacao();
            this.buscarEnvios(false);
          },
          error: () => {
            this.envioSelecionado = null;
            this.selectedEnvioId = null;
            this.finalizarProcessamentoValidacao();
            this.buscarEnvios(false);
          }
        });
      },
      error: () => {
        this.finalizarProcessamentoValidacao();
      }
    });
  }

  visualizarDocumento(documento: any): void {
    this.fecharVisualizador();
    this.api.getBlob(`/admin/documentos-registrados/${documento.id}/visualizacao`).subscribe((blob) => {
      this.visualizadorBlob = blob;
      this.visualizadorObjectUrl = URL.createObjectURL(blob);
      this.visualizadorUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.visualizadorObjectUrl);
      this.visualizadorTipo = this.detectarTipoArquivo(blob, documento.nomeOriginalArquivo);
      this.visualizadorErro = '';
      this.documentoVisualizado = documento;

      if (blob.type === 'application/pdf') {
        setTimeout(() => this.renderPdf(blob));
      }
    });
  }

  fecharVisualizador(): void {
    if (this.visualizadorObjectUrl) {
      URL.revokeObjectURL(this.visualizadorObjectUrl);
    }

    this.documentoVisualizado = null;
    this.visualizadorUrl = null;
    this.visualizadorObjectUrl = null;
    this.visualizadorTipo = '';
    this.visualizadorBlob = null;
    this.visualizadorErro = '';
  }

  isVisualizadorPdf(): boolean {
    return this.visualizadorTipo === 'application/pdf';
  }

  isVisualizadorImagem(): boolean {
    return this.visualizadorTipo.startsWith('image/');
  }

  private detectarTipoArquivo(blob: Blob, nomeArquivo: string | null | undefined): string {
    const nomeNormalizado = (nomeArquivo || '').toLowerCase();
    if (blob.type && blob.type !== 'application/octet-stream') return blob.type;
    if (nomeNormalizado.endsWith('.pdf')) return 'application/pdf';
    if (nomeNormalizado.endsWith('.jpg') || nomeNormalizado.endsWith('.jpeg')) return 'image/jpeg';
    if (nomeNormalizado.endsWith('.png')) return 'image/png';
    return blob.type || 'application/octet-stream';
  }

  private async renderPdf(blob: Blob): Promise<void> {
    if (!this.pdfContainer) return;

    const container = this.pdfContainer.nativeElement;
    container.innerHTML = '';

    try {
      const pdfjsLib = await import('pdfjs-dist');

      const pdf = await pdfjsLib.getDocument({ data: await blob.arrayBuffer(), disableWorker: true } as any).promise;
      const width = Math.max(container.clientWidth - 24, 320);

      for (let pageNumber = 1; pageNumber <= pdf.numPages; pageNumber++) {
        const page = await pdf.getPage(pageNumber);
        const viewport = page.getViewport({ scale: 1 });
        const scale = width / viewport.width;
        const scaledViewport = page.getViewport({ scale });
        const canvas = document.createElement('canvas');
        const context = canvas.getContext('2d');
        if (!context) continue;

        canvas.width = scaledViewport.width;
        canvas.height = scaledViewport.height;
        canvas.className = 'pdf-page-canvas';
        container.appendChild(canvas);

        await page.render({ canvas, canvasContext: context, viewport: scaledViewport }).promise;
      }
    } catch {
      container.innerHTML = '';
      this.visualizadorErro = 'Não foi possível carregar a pré-visualização do PDF. Use Abrir em nova aba para consultar o arquivo.';
    }
  }

  arquivoTamanho(tamanhoBytes: number): string {
    if (!tamanhoBytes) {
      return '0 KB';
    }

    if (tamanhoBytes < 1024 * 1024) {
      return `${(tamanhoBytes / 1024).toFixed(1)} KB`;
    }

    return `${(tamanhoBytes / (1024 * 1024)).toFixed(2)} MB`;
  }

  statusClasse(status: string | null | undefined): string {
    switch (status) {
      case 'Aprovado':
        return 'status-pill';
      case 'Reprovado':
        return 'status-pill rejected';
      default:
        return 'status-pill pending';
    }
  }

  statusEnvioLabel(status: string | null | undefined): string {
    const labels: Record<string, string> = {
      Pendente: 'Pendente',
      Enviado: 'Enviado',
      EmConformidade: 'Aguardando envio de NF'
    };

    return status ? labels[status] ?? status : '';
  }

  private atualizarStatusLista(envioId: number, status: string): void {
    this.envios = this.envios.map((item) => item.id === envioId ? { ...item, status } : item);
  }

  private finalizarProcessamentoValidacao(): void {
    this.processandoDocumentoId = null;
    this.acaoProcessando = null;
  }
}
