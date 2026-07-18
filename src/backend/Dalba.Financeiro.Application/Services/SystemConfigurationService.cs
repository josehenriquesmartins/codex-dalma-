using Dalba.Financeiro.Application.Abstractions.Audit;
using Dalba.Financeiro.Application.Abstractions.Persistence;
using Dalba.Financeiro.Application.Common;
using Dalba.Financeiro.Application.DTOs.Admin;
using Dalba.Financeiro.Domain.Entities;
using Dalba.Financeiro.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Dalba.Financeiro.Application.Services;

public class SystemConfigurationService
{
    private readonly IAppDbContext _context;
    private readonly IAuditService _auditService;

    public SystemConfigurationService(IAppDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<ConfiguracaoSistemaResponse> GetAsync(CancellationToken ct)
    {
        var parametros = await _context.ParametrosSistema.AsNoTracking()
            .Where(x => ChavesPermitidas.Contains(x.Chave) && x.Ativo)
            .ToDictionaryAsync(x => x.Chave, x => x.Valor, ct);

        return new ConfiguracaoSistemaResponse(
            Get(parametros, SmtpHost),
            Get(parametros, SmtpPorta),
            Get(parametros, SmtpUsuario),
            Get(parametros, SmtpSenha),
            Get(parametros, SmsProvider),
            Get(parametros, SmsConta),
            Get(parametros, SmsToken),
            Get(parametros, SmsSenha),
            Get(parametros, SmsRemetente),
            Get(parametros, SmsEndpoint),
            Get(parametros, IaApiKey),
            Get(parametros, WhatsAppApiKey));
    }

    public async Task<ConfiguracaoSistemaResponse> SaveAsync(SalvarConfiguracaoSistemaRequest request, CancellationToken ct)
    {
        await UpsertAsync(SmtpHost, request.SmtpHost, "Servidor SMTP", ct);
        await UpsertAsync(SmtpPorta, request.SmtpPorta, "Porta SMTP", ct);
        await UpsertAsync(SmtpUsuario, request.SmtpUsuario, "Usuário SMTP", ct);
        await UpsertAsync(SmtpSenha, request.SmtpSenha, "Senha SMTP", ct);
        await UpsertAsync(SmsProvider, request.SmsProvider, "Provedor SMS", ct);
        await UpsertAsync(SmsConta, request.SmsConta, "Conta SMS", ct);
        await UpsertAsync(SmsToken, request.SmsToken, "Token SMS", ct);
        await UpsertAsync(SmsSenha, request.SmsSenha, "Senha SMS", ct);
        await UpsertAsync(SmsRemetente, request.SmsRemetente, "Remetente SMS", ct);
        await UpsertAsync(SmsEndpoint, request.SmsEndpoint, "Endpoint SMS", ct);
        await UpsertAsync(IaApiKey, request.IaApiKey, "API Key IA", ct);
        await UpsertAsync(WhatsAppApiKey, request.WhatsAppApiKey, "API Key WhatsApp", ct);

        await _context.SaveChangesAsync(ct);
        await _auditService.RegistrarAsync("parametros_sistema", null, AcaoAuditoria.Edicao, "Configurações do sistema atualizadas.", ct);
        return await GetAsync(ct);
    }

    private async Task UpsertAsync(string chave, string? valor, string descricao, CancellationToken ct)
    {
        var entity = await _context.ParametrosSistema.FirstOrDefaultAsync(x => x.Chave == chave, ct);
        if (entity is null)
        {
            _context.ParametrosSistema.Add(new ParametroSistema
            {
                Chave = chave,
                Valor = valor?.Trim() ?? string.Empty,
                Descricao = descricao,
                Ativo = true
            });
            return;
        }

        entity.Valor = valor?.Trim() ?? string.Empty;
        entity.Descricao = descricao;
        entity.Ativo = true;
        entity.DataHoraAtualizacao = DbClock.Now;
    }

    private static string? Get(IReadOnlyDictionary<string, string> dados, string chave) =>
        dados.TryGetValue(chave, out var valor) ? valor : null;

    private const string SmtpHost = "CFG_SMTP_HOST";
    private const string SmtpPorta = "CFG_SMTP_PORTA";
    private const string SmtpUsuario = "CFG_SMTP_USUARIO";
    private const string SmtpSenha = "CFG_SMTP_SENHA";
    private const string SmsProvider = "CFG_SMS_PROVIDER";
    private const string SmsConta = "CFG_SMS_CONTA";
    private const string SmsToken = "CFG_SMS_TOKEN";
    private const string SmsSenha = "CFG_SMS_SENHA";
    private const string SmsRemetente = "CFG_SMS_REMETENTE";
    private const string SmsEndpoint = "CFG_SMS_ENDPOINT";
    private const string IaApiKey = "CFG_IA_API_KEY";
    private const string WhatsAppApiKey = "CFG_WHATSAPP_API_KEY";

    private static readonly HashSet<string> ChavesPermitidas =
    [
        SmtpHost,
        SmtpPorta,
        SmtpUsuario,
        SmtpSenha,
        SmsProvider,
        SmsConta,
        SmsToken,
        SmsSenha,
        SmsRemetente,
        SmsEndpoint,
        IaApiKey,
        WhatsAppApiKey
    ];
}
