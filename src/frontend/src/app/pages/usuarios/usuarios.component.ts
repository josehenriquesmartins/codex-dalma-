import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-usuarios',
  templateUrl: './usuarios.component.html'
})
export class UsuariosComponent implements OnInit {
  usuarios: any[] = [];
  form;
  editingId: number | null = null;

  constructor(private readonly api: ApiService, private readonly fb: FormBuilder) {
    this.form = this.fb.group({
      nome: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      login: ['', Validators.required],
      senha: ['', Validators.required],
      perfil: ['Admin', Validators.required],
      fornecedorId: [null],
      ativo: [true]
    });
  }

  ngOnInit(): void {
    this.load();
    this.form.get('perfil')?.valueChanges.subscribe((perfil) => this.applyPerfilRules(perfil));
    this.applyPerfilRules(this.form.get('perfil')?.value ?? null);
  }

  load(): void { this.api.get<any[]>('/usuarios').subscribe((res) => this.usuarios = res); }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const request = this.buildRequest();
    const action = this.editingId ? this.api.put(`/usuarios/${this.editingId}`, request) : this.api.post('/usuarios', request);
    action.subscribe(() => {
      this.cancelEdit();
      this.load();
    });
  }

  edit(item: any): void {
    this.editingId = item.id;
    this.form.get('senha')?.clearValidators();
    this.form.get('senha')?.updateValueAndValidity();
    this.form.patchValue({ ...item, senha: '' });
    this.applyPerfilRules(item.perfil);
  }

  remove(item: any): void {
    if (!confirm(`Excluir usuário ${item.login}?`)) return;
    this.api.delete(`/usuarios/${item.id}`).subscribe(() => this.load());
  }

  cancelEdit(): void {
    this.editingId = null;
    this.form.get('senha')?.setValidators(Validators.required);
    this.form.get('senha')?.updateValueAndValidity();
    this.form.reset({ perfil: 'Admin', ativo: true, fornecedorId: null });
    this.applyPerfilRules('Admin');
  }

  get isPerfilFornecedor(): boolean {
    return this.form.get('perfil')?.value === 'Fornecedor';
  }

  private applyPerfilRules(perfil: string | null): void {
    const fornecedorControl = this.form.get('fornecedorId');
    if (!fornecedorControl) return;

    if (perfil === 'Fornecedor') {
      fornecedorControl.setValidators(Validators.required);
      fornecedorControl.enable({ emitEvent: false });
    } else {
      fornecedorControl.clearValidators();
      fornecedorControl.setValue(null, { emitEvent: false });
      fornecedorControl.disable({ emitEvent: false });
    }

    fornecedorControl.updateValueAndValidity({ emitEvent: false });
  }

  private buildRequest(): any {
    const value = this.form.getRawValue();
    return {
      ...value,
      fornecedorId: value.perfil === 'Fornecedor' ? value.fornecedorId : null
    };
  }
}
