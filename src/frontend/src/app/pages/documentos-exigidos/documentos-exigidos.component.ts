import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-documentos-exigidos',
  templateUrl: './documentos-exigidos.component.html'
})
export class DocumentosExigidosComponent implements OnInit {
  exigidos: any[] = [];
  tipos: any[] = [];
  categorias: any[] = [];
  exigidoForm;
  editingExigidoId: number | null = null;

  constructor(private readonly api: ApiService, private readonly fb: FormBuilder) {
    this.exigidoForm = this.fb.group({
      documentoTipoId: [1, Validators.required],
      tipoPessoa: ['Juridica', Validators.required],
      porteEmpresa: ['Microempresa'],
      categoriaId: [1, Validators.required],
      obrigatorio: [true],
      ativo: [true]
    });
  }

  ngOnInit(): void {
    this.load();
    this.api.get<any[]>('/documentos/tipos').subscribe((res) => this.tipos = res);
    this.api.get<any[]>('/categorias').subscribe((res) => this.categorias = res);
    this.exigidoForm.get('tipoPessoa')?.valueChanges.subscribe((tipoPessoa) => this.applyTipoPessoaRules(tipoPessoa));
  }

  load(): void {
    this.api.get<any[]>('/documentos/exigidos').subscribe((res) => this.exigidos = res);
  }

  save(): void {
    if (this.exigidoForm.invalid) {
      this.exigidoForm.markAllAsTouched();
      return;
    }

    const request = this.buildRequest();
    const action = this.editingExigidoId
      ? this.api.put(`/documentos/exigidos/${this.editingExigidoId}`, request)
      : this.api.post('/documentos/exigidos', request);

    action.subscribe(() => {
      this.cancelEdit();
      this.load();
    });
  }

  edit(item: any): void {
    this.editingExigidoId = item.id;
    this.exigidoForm.patchValue(item);
    this.applyTipoPessoaRules(item.tipoPessoa);
  }

  remove(item: any): void {
    if (!confirm(`Excluir regra ${item.documentoNome}?`)) return;
    this.api.delete(`/documentos/exigidos/${item.id}`).subscribe(() => this.load());
  }

  cancelEdit(): void {
    this.editingExigidoId = null;
    this.exigidoForm.reset({ documentoTipoId: 1, tipoPessoa: 'Juridica', porteEmpresa: 'Microempresa', categoriaId: 1, obrigatorio: true, ativo: true });
    this.applyTipoPessoaRules('Juridica');
  }

  get isPessoaFisica(): boolean {
    return this.exigidoForm.get('tipoPessoa')?.value === 'Fisica';
  }

  private applyTipoPessoaRules(tipoPessoa: string | null): void {
    const porteControl = this.exigidoForm.get('porteEmpresa');
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
    const value = this.exigidoForm.getRawValue();
    return {
      ...value,
      porteEmpresa: value.tipoPessoa === 'Fisica' ? null : value.porteEmpresa
    };
  }
}
