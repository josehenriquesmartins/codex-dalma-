import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-contratos',
  templateUrl: './contratos.component.html'
})
export class ContratosComponent implements OnInit {
  contratos: any[] = [];
  fornecedores: any[] = [];
  pagina = 1;
  tamanho = 10;
  filtro = '';
  modalAberto = false;
  form;
  editingId: number | null = null;

  constructor(private readonly api: ApiService, private readonly fb: FormBuilder, private readonly auth: AuthService) {
    this.form = this.fb.group({
      fornecedorId: [1, Validators.required],
      numeroContrato: ['', Validators.required],
      descricao: ['', Validators.required],
      dataInicio: ['', Validators.required],
      dataFim: [''],
      ativo: [true]
    });
  }

  ngOnInit(): void {
    this.load();
    if (!this.isFornecedor) {
      this.api.get<any[]>('/fornecedores').subscribe((res) => this.fornecedores = res);
    }
  }

  load(): void { this.api.get<any[]>('/contratos').subscribe((res) => this.contratos = res); }
  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const action = this.editingId ? this.api.put(`/contratos/${this.editingId}`, this.form.getRawValue()) : this.api.post('/contratos', this.form.getRawValue());
    action.subscribe(() => { this.cancelEdit(); this.load(); });
  }
  abrirNovo(): void { this.cancelEdit(); this.modalAberto = true; }
  edit(item: any): void { this.editingId = item.id; this.form.patchValue(item); this.modalAberto = true; }
  remove(item: any): void {
    if (!confirm(`Excluir contrato ${item.numeroContrato}?`)) return;
    this.api.delete(`/contratos/${item.id}`).subscribe(() => this.load());
  }
  cancelEdit(): void { this.editingId = null; this.modalAberto = false; this.form.reset({ fornecedorId: 1, ativo: true }); }

  get isFornecedor(): boolean {
    return this.auth.role === 'Fornecedor';
  }
}
