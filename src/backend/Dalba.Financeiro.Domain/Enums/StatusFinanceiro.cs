namespace Dalba.Financeiro.Domain.Enums;

public enum StatusFinanceiro
{
    AguardandoEnvioNf = 1,
    AguardandoPagamento = 2,
    EmAnaliseFinanceira = 3,
    LiberadoParaPagamento = 4,
    Pago = 5
}
