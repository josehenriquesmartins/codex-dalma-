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
  roleLabel = '';
  filtro = {
    mesReferencia: new Date().getMonth() + 1,
    anoReferencia: new Date().getFullYear()
  };
  loading = false;

  constructor(private readonly api: ApiService, private readonly auth: AuthService) {}

  ngOnInit(): void {
    this.role = this.auth.role ?? '';
    this.roleLabel = this.role === 'Financeiro' ? 'Custo' : this.role;
    this.load();
  }

  load(): void {
    const mes = Number(this.filtro.mesReferencia);
    const ano = Number(this.filtro.anoReferencia);
    if (mes < 1 || mes > 12 || ano < 2000) return;

    const path = this.role === 'Fornecedor' ? '/dashboard/fornecedor' : this.role === 'Financeiro' ? '/dashboard/financeiro' : '/dashboard/admin';
    const query = `?mesReferencia=${mes}&anoReferencia=${ano}`;
    this.loading = true;
    this.api.get<Record<string, number | string>>(`${path}${query}`).subscribe({
      next: (data) => this.data = data,
      complete: () => this.loading = false,
      error: () => this.loading = false
    });
  }

  entries(): Array<{ key: string; value: number | string }> {
    return Object.entries(this.data).map(([key, value]) => ({ key, value }));
  }

  isNumber(value: number | string): boolean {
    return typeof value === 'number';
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
      documentosReprovados: 'Documentos reprovados',
      situacaoMesAtual: 'Situação do mês atual',
      ultimosEnvios: 'Últimos envios',
      notificacoesRecebidas: 'Notificações recebidas',
      notasAguardadas: 'Notas aguardadas',
      emAnalise: 'Em análise',
      liberados: 'Liberados',
      pagos: 'Pagos',
      contratosAtivos: 'Contratos ativos'
    };

    return labels[key] ?? this.humanize(key);
  }

  valueLabel(value: number | string): string {
    if (typeof value !== 'string') return String(value);
    const labels: Record<string, string> = {
      EmConformidade: 'Em Conformidade',
      AguardandoEnvioNf: 'Aguardando envio de NF',
      AguardandoPagamento: 'Aguardando pagamento',
      EmAnaliseFinanceira: 'Em análise',
      LiberadoParaPagamento: 'Liberado para pagamento',
      Pago: 'Pago',
      Pendente: 'Pendente',
      Enviado: 'Enviado',
      SEM_ENVIO: 'Sem envio'
    };

    return labels[value] ?? this.humanize(value);
  }

  private humanize(value: string): string {
    return value
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
