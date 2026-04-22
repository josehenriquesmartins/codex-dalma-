import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-documentos-catalogo',
  templateUrl: './documentos-catalogo.component.html'
})
export class DocumentosCatalogoComponent implements OnInit {
  tipos: any[] = [];
  tipoForm;
  editingTipoId: number | null = null;

  constructor(private readonly api: ApiService, private readonly fb: FormBuilder) {
    this.tipoForm = this.fb.group({
      codigo: ['', Validators.required],
      nomeDocumento: ['', Validators.required],
      descricao: [''],
      ativo: [true]
    });
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.get<any[]>('/documentos/tipos').subscribe((res) => this.tipos = res);
  }

  save(): void {
    if (this.tipoForm.invalid) {
      this.tipoForm.markAllAsTouched();
      return;
    }

    const action = this.editingTipoId
      ? this.api.put(`/documentos/tipos/${this.editingTipoId}`, this.tipoForm.getRawValue())
      : this.api.post('/documentos/tipos', this.tipoForm.getRawValue());

    action.subscribe(() => {
      this.cancelEdit();
      this.load();
    });
  }

  edit(item: any): void {
    this.editingTipoId = item.id;
    this.tipoForm.patchValue(item);
  }

  remove(item: any): void {
    if (!confirm(`Excluir tipo ${item.codigo}?`)) return;
    this.api.delete(`/documentos/tipos/${item.id}`).subscribe(() => this.load());
  }

  cancelEdit(): void {
    this.editingTipoId = null;
    this.tipoForm.reset({ ativo: true });
  }
}
