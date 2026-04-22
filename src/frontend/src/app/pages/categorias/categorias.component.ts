import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-categorias',
  templateUrl: './categorias.component.html'
})
export class CategoriasComponent implements OnInit {
  categorias: any[] = [];
  form;
  editingId: number | null = null;

  constructor(private readonly api: ApiService, private readonly fb: FormBuilder) {
    this.form = this.fb.group({ codigo: ['', Validators.required], descricao: ['', Validators.required], ativo: [true] });
  }

  ngOnInit(): void { this.load(); }
  load(): void { this.api.get<any[]>('/categorias').subscribe((res) => this.categorias = res); }
  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const action = this.editingId ? this.api.put(`/categorias/${this.editingId}`, this.form.getRawValue()) : this.api.post('/categorias', this.form.getRawValue());
    action.subscribe(() => { this.cancelEdit(); this.load(); });
  }
  edit(item: any): void { this.editingId = item.id; this.form.patchValue(item); }
  remove(item: any): void {
    if (!confirm(`Excluir categoria ${item.codigo}?`)) return;
    this.api.delete(`/categorias/${item.id}`).subscribe(() => this.load());
  }
  cancelEdit(): void { this.editingId = null; this.form.reset({ ativo: true }); }
}
