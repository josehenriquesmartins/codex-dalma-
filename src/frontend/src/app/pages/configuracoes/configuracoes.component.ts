import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-configuracoes',
  templateUrl: './configuracoes.component.html'
})
export class ConfiguracoesComponent implements OnInit {
  carregando = false;
  salvando = false;
  visibleSecrets = new Set<string>();

  form;

  constructor(private readonly api: ApiService, private readonly fb: FormBuilder) {
    this.form = this.fb.group({
      smtpHost: [''],
      smtpPorta: ['587'],
      smtpUsuario: [''],
      smtpSenha: [''],
      smsProvider: [''],
      smsConta: [''],
      smsToken: [''],
      smsSenha: [''],
      smsRemetente: [''],
      smsEndpoint: [''],
      iaApiKey: [''],
      whatsAppApiKey: ['']
    });
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.carregando = true;
    this.api.get<any>('/admin/configuracoes').subscribe({
      next: (res) => {
        this.form.patchValue(res ?? {});
        this.carregando = false;
      },
      error: () => {
        this.carregando = false;
      }
    });
  }

  save(): void {
    this.salvando = true;
    this.api.put<any>('/admin/configuracoes', this.form.getRawValue()).subscribe({
      next: (res) => {
        this.form.patchValue(res ?? {});
        this.salvando = false;
      },
      error: () => {
        this.salvando = false;
      }
    });
  }

  toggleSecret(field: string): void {
    if (this.visibleSecrets.has(field)) {
      this.visibleSecrets.delete(field);
      return;
    }

    this.visibleSecrets.add(field);
  }

  secretType(field: string): 'text' | 'password' {
    return this.visibleSecrets.has(field) ? 'text' : 'password';
  }

  secretButtonLabel(field: string): string {
    return this.visibleSecrets.has(field) ? 'Ocultar' : 'Visualizar';
  }
}
