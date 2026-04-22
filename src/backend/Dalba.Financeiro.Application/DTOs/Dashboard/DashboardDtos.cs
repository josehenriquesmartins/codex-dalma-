namespace Dalba.Financeiro.Application.DTOs.Dashboard;

public sealed record DashboardAdminDto(int TotalFornecedores, int Pendentes, int Enviados, int EmConformidade, int ContratosAtivos, int NotificacoesPendentes);
public sealed record DashboardFornecedorDto(string SituacaoMesAtual, int DocumentosFaltantes, int UltimosEnvios, int NotificacoesRecebidas, int AguardandoEnvioNf);
public sealed record DashboardFinanceiroDto(int EmConformidade, int NotasAguardadas, int EmAnalise, int Liberados, int Pagos);
