import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  data: Record<string, number | string> = {};
  role = '';

  constructor(private readonly api: ApiService, private readonly auth: AuthService) {}

  ngOnInit(): void {
    this.role = this.auth.role ?? '';
    const path = this.role === 'Fornecedor' ? '/dashboard/fornecedor' : this.role === 'Financeiro' ? '/dashboard/financeiro' : '/dashboard/admin';
    this.api.get<Record<string, number | string>>(path).subscribe((data) => this.data = data);
  }

  entries(): Array<{ key: string; value: number | string }> {
    return Object.entries(this.data).map(([key, value]) => ({ key, value }));
  }

  labelFor(key: string): string {
    const labels: Record<string, string> = {
      totalFornecedores: 'Fornecedores',
      pendentes: 'Pendentes',
      enviados: 'Enviados',
      emConformidade: 'Em conformidade',
      reprovados: 'Reprovados',
      aprovados: 'Aprovados',
      notificacoesPendentes: 'Alertas para Admin',
      aguardandoEnvioNf: 'Aguardando envio de NF',
      documentosPendentes: 'Documentos pendentes',
      documentosEnviados: 'Documentos enviados',
      documentosAprovados: 'Documentos aprovados',
      documentosReprovados: 'Documentos reprovados'
    };

    return labels[key] ?? key
      .replace(/([A-Z])/g, ' $1')
      .replace(/^./, (value) => value.toUpperCase())
      .trim();
  }

  iconFor(index: number): string {
    return ['bi-people', 'bi-clock-history', 'bi-send-check', 'bi-shield-check', 'bi-x-circle', 'bi-check2-circle'][index % 6];
  }

  toneFor(index: number): string {
    return ['tone-blue', 'tone-amber', 'tone-teal', 'tone-green', 'tone-red', 'tone-indigo'][index % 6];
  }
}
