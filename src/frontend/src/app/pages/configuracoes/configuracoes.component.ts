import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-configuracoes',
  templateUrl: './configuracoes.component.html'
})
export class ConfiguracoesComponent implements OnInit {
  carregando = false;
  salvando = false;

  form;

  constructor(private readonly api: ApiService, private readonly fb: FormBuilder) {
    this.form = this.fb.group({
      smtpHost: ['', Validators.required],
      smtpPorta: ['587', Validators.required],
      smtpUsuario: [''],
      smtpSenha: [''],
      smsProvider: [''],
      smsConta: [''],
      smsToken: [''],
      smsRemetente: [''],
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
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

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
}
