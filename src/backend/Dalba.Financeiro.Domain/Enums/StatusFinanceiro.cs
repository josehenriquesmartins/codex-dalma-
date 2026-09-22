namespace Dalba.Financeiro.Domain.Enums;

public enum StatusFinanceiro
{
    AguardandoEnvioNf = 1,
    AguardandoPagamento = 2,
    EmAnaliseFinanceira = 3,
    LiberadoParaPagamento = 4,
    Pago = 5,
    // Status de consulta, calculado pela situação documental; não persistir.
    PendenciaDocumental = 6
}
