import { Component, ElementRef, OnDestroy, ViewChild } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-portal-fornecedor',
  templateUrl: './portal-fornecedor.component.html'
})
export class PortalFornecedorComponent implements OnDestroy {
  @ViewChild('pdfContainer') pdfContainer?: ElementRef<HTMLDivElement>;

  envio: any;
  contratos: any[] = [];
  arquivos: File[] = [];
  uploading = false;
  fileTouched = false;
  documentoVisualizado: any | null = null;
  visualizadorUrl: SafeResourceUrl | null = null;
  visualizadorObjectUrl: string | null = null;
  visualizadorTipo = '';
  visualizadorBlob: Blob | null = null;
  visualizadorErro = '';
  form;
  uploadForm;

  constructor(private readonly api: ApiService, private readonly fb: FormBuilder, private readonly sanitizer: DomSanitizer) {
    this.form = this.fb.group({
      mesReferencia: [new Date().getMonth() + 1, Validators.required],
      anoReferencia: [new Date().getFullYear(), Validators.required],
      contratoId: [null, Validators.required],
      observacao: ['']
    });
    this.uploadForm = this.fb.group({ documentoTipoId: [1, Validators.required] });
  }

  ngOnDestroy(): void {
    this.fecharVisualizador();
  }

  ngOnInit(): void {
    this.api.get<any[]>('/contratos').subscribe((res) => {
      this.contratos = res.filter((item) => item.ativo);
      if (this.contratos.length && !this.form.controls.contratoId.value) {
        this.form.patchValue({ contratoId: this.contratos[0].id });
      }
    });
  }

  carregar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.api.post<any>('/portal-fornecedor/envios', this.form.getRawValue()).subscribe((res) => {
      this.envio = res;
      this.uploadForm.patchValue({ documentoTipoId: this.documentosPendentes[0]?.documentoTipoId ?? null });
    });
  }

  onFileChange(event: Event): void {
    this.fileTouched = true;
    const target = event.target as HTMLInputElement;
    this.arquivos = Array.from(target.files ?? []);
  }

  upload(): void {
    if (this.uploadForm.invalid || !this.arquivos.length || !this.envio) {
      this.uploadForm.markAllAsTouched();
      this.fileTouched = true;
      return;
    }

    this.uploading = true;
    const formData = new FormData();
    this.arquivos.forEach((arquivo) => formData.append('files', arquivo));

    this.api.post(`/portal-fornecedor/envios/${this.envio.id}/upload/${this.uploadForm.value.documentoTipoId}`, formData).subscribe(() => {
      this.uploading = false;
      this.arquivos = [];
      this.fileTouched = false;
      this.carregar();
    }).add(() => {
      this.uploading = false;
    });
  }

  get documentosPendentes(): any[] {
    return this.envio?.documentos?.filter((item: any) => !item.enviado) ?? [];
  }

  visualizarDocumento(documento: any): void {
    if (!documento.documentoRegistradoId) return;

    this.fecharVisualizador();
    this.api.getBlob(`/portal-fornecedor/documentos-registrados/${documento.documentoRegistradoId}/visualizacao`).subscribe((blob) => {
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

  arquivoTamanho(tamanhoBytes: number | null | undefined): string {
    if (!tamanhoBytes) return '';
    if (tamanhoBytes < 1024 * 1024) return `${(tamanhoBytes / 1024).toFixed(1)} KB`;
    return `${(tamanhoBytes / (1024 * 1024)).toFixed(2)} MB`;
  }

  statusEnvioLabel(status: string | null | undefined): string {
    const labels: Record<string, string> = {
      Pendente: 'Pendente',
      Enviado: 'Enviado',
      EmConformidade: 'Aguardando envio de NF'
    };

    return status ? labels[status] ?? status : '';
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
}
