import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-fornecedores',
  templateUrl: './fornecedores.component.html'
})
export class FornecedoresComponent implements OnInit {
  fornecedores: any[] = [];
  categorias: any[] = [];
  pagina = 1;
  tamanho = 10;
  form;
  editingId: number | null = null;
  arquivoImportacao: File | null = null;
  importando = false;
  resultadoImportacao: any[] = [];

  constructor(private readonly api: ApiService, private readonly fb: FormBuilder) {
    this.form = this.fb.group({
      codigoFornecedor: ['', Validators.required],
      tipoPessoa: ['Juridica', Validators.required],
      porteEmpresa: ['Microempresa'],
      categoriaId: [1, Validators.required],
      nomeOuRazaoSocial: ['', Validators.required],
      nomeFantasia: [''],
      cpfOuCnpj: ['', Validators.required],
      ddiTelefone: ['+55', Validators.required],
      dddTelefone: ['11', Validators.required],
      numeroTelefone: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      cep: ['', Validators.required],
      logradouro: ['', Validators.required],
      numero: ['', Validators.required],
      complemento: [''],
      bairro: ['', Validators.required],
      cidade: ['', Validators.required],
      estado: ['SP', Validators.required],
      pais: ['Brasil', Validators.required],
      ativo: [true]
    });

    this.form.get('tipoPessoa')?.valueChanges.subscribe((tipoPessoa) => this.applyTipoPessoaRules(tipoPessoa));
  }

  ngOnInit(): void {
    this.load();
    this.api.get<any[]>('/categorias').subscribe((res) => this.categorias = res);
  }

  load(): void { this.api.get<any[]>('/fornecedores').subscribe((res) => this.fornecedores = res); }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const request = this.buildRequest();
    const action = this.editingId ? this.api.put(`/fornecedores/${this.editingId}`, request) : this.api.post('/fornecedores', request);
    action.subscribe(() => { this.cancelEdit(); this.load(); });
  }

  edit(item: any): void {
    this.editingId = item.id;
    this.form.patchValue(item);
    this.applyTipoPessoaRules(item.tipoPessoa);
  }

  remove(item: any): void {
    if (!confirm(`Excluir fornecedor ${item.codigoFornecedor}?`)) return;
    this.api.delete(`/fornecedores/${item.id}`).subscribe(() => this.load());
  }

  onImportFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.arquivoImportacao = input.files?.[0] ?? null;
  }

  importarExcel(): void {
    if (!this.arquivoImportacao) return;
    const formData = new FormData();
    formData.append('arquivo', this.arquivoImportacao);
    this.importando = true;
    fetch(`${environment.apiUrl}/fornecedores/importacao-excel`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${localStorage.getItem('dalba_auth') ? JSON.parse(localStorage.getItem('dalba_auth') as string).token : ''}` },
      body: formData
    }).then(async (response) => {
      this.importando = false;
      if (!response.ok) throw new Error('Falha na importação.');
      this.resultadoImportacao = await response.json();
      this.load();
    }).catch(() => {
      this.importando = false;
    });
  }

  cancelEdit(): void {
    this.editingId = null;
    this.form.reset({ tipoPessoa: 'Juridica', porteEmpresa: 'Microempresa', categoriaId: 1, ddiTelefone: '+55', dddTelefone: '11', estado: 'SP', pais: 'Brasil', ativo: true });
    this.applyTipoPessoaRules('Juridica');
  }

  get isPessoaFisica(): boolean {
    return this.form.get('tipoPessoa')?.value === 'Fisica';
  }

  get documentoLabel(): string {
    return this.isPessoaFisica ? 'CPF' : 'CNPJ';
  }

  get documentoPlaceholder(): string {
    return this.isPessoaFisica ? 'Informe o CPF' : 'Informe o CNPJ';
  }

  private applyTipoPessoaRules(tipoPessoa: string | null): void {
    const porteControl = this.form.get('porteEmpresa');
    if (!porteControl) return;

    if (tipoPessoa === 'Fisica') {
      porteControl.setValue(null, { emitEvent: false });
      porteControl.disable({ emitEvent: false });
      return;
    }

    porteControl.enable({ emitEvent: false });
    if (!porteControl.value) {
      porteControl.setValue('Microempresa', { emitEvent: false });
    }
  }

  private buildRequest(): any {
    const value = this.form.getRawValue();
    return {
      ...value,
      porteEmpresa: value.tipoPessoa === 'Fisica' ? null : value.porteEmpresa
    };
  }
}
