using Dalba.Financeiro.Application.Abstractions.Audit;
using Dalba.Financeiro.Application.Abstractions.Notifications;
using Dalba.Financeiro.Application.Abstractions.Persistence;
using Dalba.Financeiro.Application.Abstractions.Security;
using Dalba.Financeiro.Application.Abstractions.Storage;
using Dalba.Financeiro.Application.Common;
using Dalba.Financeiro.Application.DTOs.Admin;
using Dalba.Financeiro.Application.DTOs.Dashboard;
using Dalba.Financeiro.Application.DTOs.Financeiro;
using Dalba.Financeiro.Application.DTOs.Notificacoes;
using Dalba.Financeiro.Application.DTOs.Portal;
using Dalba.Financeiro.Domain.Entities;
using Dalba.Financeiro.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Dalba.Financeiro.Application.Services;

public class NotificationService
{
    private static readonly TimeZoneInfo SaoPauloTimeZone = LoadSaoPauloTimeZone();
    private readonly IAppDbContext _context;
    private readonly INotificationDispatcher _dispatcher;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public NotificationService(IAppDbContext context, INotificationDispatcher dispatcher, IAuditService auditService, ICurrentUserService currentUser)
    {
        _context = context;
        _dispatcher = dispatcher;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<NotificacaoResponse>> ListAsync(CancellationToken ct)
    {
        var query = _context.Notificacoes.AsNoTracking();
        if (_currentUser.Perfil != PerfilAcesso.Admin)
        {
            var usuarioId = _currentUser.UserId ?? throw new AppException("Usuário não autenticado.", 401);
            var fornecedorId = _currentUser.FornecedorId;
            query = query.Where(x =>
                x.UsuarioId == usuarioId ||
                x.RemetenteUsuarioId == usuarioId ||
                (fornecedorId.HasValue && x.FornecedorId == fornecedorId));
        }

        var notificacoes = await query.OrderByDescending(x => x.DataHoraCriacao).ToListAsync(ct);
        return notificacoes
            .Select(x => new NotificacaoResponse(
                x.Id,
                x.TipoNotificacao,
                x.Titulo,
                x.Mensagem,
                x.StatusEnvio,
                ToSaoPauloTime(x.DataHoraCriacao),
                ToSaoPauloTime(x.DataHoraEnvio),
                x.Tentativas,
                x.UsuarioId,
                x.RemetenteUsuarioId,
                x.Destinatario,
                x.Erro))
            .ToList();
    }

    public async Task RegistrarEEnviarAsync(long? usuarioId, long? fornecedorId, TipoNotificacao tipo, string destinatario, string titulo, string mensagem, string referenciaEntidade, long? referenciaId, CancellationToken ct)
    {
        var notification = new Notificacao
        {
            UsuarioId = usuarioId,
            RemetenteUsuarioId = _currentUser.UserId,
            FornecedorId = fornecedorId,
            TipoNotificacao = tipo,
            Destinatario = destinatario,
            Titulo = titulo,
            Mensagem = mensagem,
            ReferenciaEntidade = referenciaEntidade,
            ReferenciaId = referenciaId,
            Tentativas = 1
        };

        var result = await _dispatcher.DispatchAsync(tipo, destinatario, titulo, mensagem, ct);
        notification.StatusEnvio = result.Success ? StatusNotificacao.Enviado : StatusNotificacao.Falha;
        notification.DataHoraEnvio = DbClock.Now;
        notification.Erro = result.Error;

        _context.Notificacoes.Add(notification);
        await _context.SaveChangesAsync(ct);
        await _auditService.RegistrarAsync("notificacoes", notification.Id, AcaoAuditoria.Notificacao, $"{tipo} para {destinatario}: {notification.StatusEnvio}.", ct);
    }

    public async Task NotificarFornecedorEmailSmsAsync(Fornecedor fornecedor, string titulo, string mensagem, string referenciaEntidade, long? referenciaId, CancellationToken ct)
    {
        await RegistrarEEnviarAsync(null, fornecedor.Id, TipoNotificacao.Email, fornecedor.Email, titulo, mensagem, referenciaEntidade, referenciaId, ct);
        await RegistrarEEnviarAsync(null, fornecedor.Id, TipoNotificacao.Sms, $"{fornecedor.DddTelefone}{fornecedor.NumeroTelefone}", titulo, mensagem, referenciaEntidade, referenciaId, ct);
    }

    public async Task RegistrarDashboardAdminAsync(string titulo, string mensagem, string referenciaEntidade, long? referenciaId, CancellationToken ct)
    {
        var admins = await _context.Usuarios.AsNoTracking()
            .Where(x => x.Ativo && x.Perfil == PerfilAcesso.Admin)
            .Select(x => new { x.Id, x.Login })
            .ToListAsync(ct);

        foreach (var admin in admins)
        {
            _context.Notificacoes.Add(new Notificacao
            {
                UsuarioId = admin.Id,
                RemetenteUsuarioId = _currentUser.UserId,
                TipoNotificacao = TipoNotificacao.Sistema,
                Destinatario = admin.Login,
                Titulo = titulo,
                Mensagem = mensagem,
                ReferenciaEntidade = referenciaEntidade,
                ReferenciaId = referenciaId,
                StatusEnvio = StatusNotificacao.Pendente,
                Tentativas = 0
            });
        }

        await _context.SaveChangesAsync(ct);
        await _auditService.RegistrarAsync("notificacoes", referenciaId, AcaoAuditoria.Notificacao, $"Dashboard Admin: {titulo}.", ct);
    }

    private static TimeZoneInfo LoadSaoPauloTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        }
    }

    private static DateTime ToSaoPauloTime(DateTime value)
    {
        var utcValue = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        return DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(utcValue, SaoPauloTimeZone), DateTimeKind.Unspecified);
    }

    private static DateTime? ToSaoPauloTime(DateTime? value)
    {
        return value.HasValue ? ToSaoPauloTime(value.Value) : null;
    }
}

public class SupplierPortalService
{
    private static readonly string[] AllowedExtensions = [".pdf", ".jpg", ".jpeg", ".png"];
    private const long MaxFileSize = 10 * 1024 * 1024;
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorageService;
    private readonly IAuditService _auditService;
    private readonly NotificationService _notificationService;

    public SupplierPortalService(IAppDbContext context, ICurrentUserService currentUser, IFileStorageService fileStorageService, IAuditService auditService, NotificationService notificationService)
    {
        _context = context;
        _currentUser = currentUser;
        _fileStorageService = fileStorageService;
        _auditService = auditService;
        _notificationService = notificationService;
    }

    public async Task<EnvioMensalResponse> GetOrCreateAsync(CriarEnvioMensalRequest request, CancellationToken ct)
    {
        var fornecedor = await GetCurrentFornecedorAsync(ct);
        var usuarioId = _currentUser.UserId ?? throw new AppException("Usuário não autenticado.", 401);
        if (!request.ContratoId.HasValue) throw new AppException("Contrato é obrigatório para abrir a competência.");

        var contrato = await _context.Contratos.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ContratoId.Value && x.FornecedorId == fornecedor.Id && x.Ativo, ct)
            ?? throw new AppException("Contrato não encontrado para o fornecedor autenticado.");
        var competenciaInicio = new DateOnly(request.AnoReferencia, request.MesReferencia, 1);
        var competenciaFim = competenciaInicio.AddMonths(1).AddDays(-1);
        if (contrato.DataInicio > competenciaFim || (contrato.DataFim.HasValue && contrato.DataFim.Value < competenciaInicio))
        {
            throw new AppException($"A competência {request.MesReferencia:00}/{request.AnoReferencia} está fora do prazo de validade do contrato {contrato.NumeroContrato}.");
        }

        var envio = await _context.DocumentosEnviados
            .Include(x => x.DocumentosRegistrados)
            .ThenInclude(x => x.DocumentoTipo)
            .FirstOrDefaultAsync(x =>
                x.FornecedorId == fornecedor.Id &&
                x.ContratoId == contrato.Id &&
                x.MesReferencia == request.MesReferencia &&
                x.AnoReferencia == request.AnoReferencia, ct);

        if (envio is null)
        {
            envio = new DocumentoEnviado
            {
                FornecedorId = fornecedor.Id,
                UsuarioId = usuarioId,
                ContratoId = contrato.Id,
                MesReferencia = request.MesReferencia,
                AnoReferencia = request.AnoReferencia,
                Observacao = request.Observacao,
                UsuarioRegistro = _currentUser.Login ?? "sistema",
                Status = StatusEnvioMensal.Pendente
            };
            _context.DocumentosEnviados.Add(envio);
            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex, "uq_documentos_enviados_ref"))
            {
                _context.Entry(envio).State = EntityState.Detached;
                var envioExistente = await _context.DocumentosEnviados.AsNoTracking()
                    .Where(x => x.FornecedorId == fornecedor.Id && x.MesReferencia == request.MesReferencia && x.AnoReferencia == request.AnoReferencia)
                    .OrderByDescending(x => x.ContratoId == contrato.Id)
                    .FirstOrDefaultAsync(ct)
                    ?? throw new AppException("Já existe competência aberta para este contrato, mês e ano. Abra a competência existente para ver os documentos enviados e pendentes.", 409);

                return await BuildResponseAsync(envioExistente.Id, "Já existe competência aberta. Os documentos enviados e pendentes estão listados abaixo.", ct);
            }

            await _auditService.RegistrarAsync("documentos_enviados", envio.Id, AcaoAuditoria.Criacao, $"Envio mensal {request.MesReferencia}/{request.AnoReferencia} criado.", ct);
        }
        else
        {
            return await BuildResponseAsync(envio.Id, "Já existe competência aberta. Os documentos enviados e pendentes estão listados abaixo.", ct);
        }

        return await BuildResponseAsync(envio.Id, ct);
    }

    public async Task<UploadDocumentoResponse> UploadAsync(long envioId, long documentoTipoId, IEnumerable<IFormFile> files, CancellationToken ct)
    {
        var fornecedor = await GetCurrentFornecedorAsync(ct);
        var envio = await _context.DocumentosEnviados.Include(x => x.DocumentosRegistrados)
            .FirstOrDefaultAsync(x => x.Id == envioId && x.FornecedorId == fornecedor.Id, ct)
            ?? throw new AppException("Envio mensal não encontrado.", 404);

        if (envio.Status == StatusEnvioMensal.EmConformidade) throw new AppException("Envio em conformidade não pode ser alterado.");
        var arquivos = files.Where(x => x is not null).ToList();
        if (arquivos.Count == 0) throw new AppException("Selecione ao menos um arquivo.");

        var documentoExistente = await _context.DocumentosRegistrados.AsNoTracking()
            .FirstOrDefaultAsync(x => x.DocumentoEnviadoId == envio.Id && x.DocumentoTipoId == documentoTipoId, ct);

        if (documentoExistente is not null && documentoExistente.StatusValidacaoDocumento != StatusValidacaoDocumento.Reprovado)
        {
            throw new AppException("Este documento já foi enviado e não pode ser alterado enquanto existir um arquivo válido para análise ou aprovado.");
        }

        if (documentoExistente is not null)
        {
            var registroAnterior = await _context.DocumentosRegistrados.FirstAsync(x => x.Id == documentoExistente.Id, ct);
            _context.DocumentosRegistrados.Remove(registroAnterior);
            await _fileStorageService.DeleteAsync(registroAnterior.CaminhoArquivo, ct);
        }

        DocumentoRegistrado? ultimoArquivoRegistrado = null;

        foreach (var file in arquivos)
        {
            if (file.Length == 0 || file.Length > MaxFileSize) throw new AppException("Arquivo inválido ou acima do limite de 10MB.");
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension)) throw new AppException("Extensão de arquivo não permitida.");

            var stored = await _fileStorageService.SaveAsync(fornecedor.CodigoFornecedor, envio.AnoReferencia, envio.MesReferencia, file, ct);
            var novoArquivo = new DocumentoRegistrado
            {
                DocumentoEnviadoId = envio.Id,
                DocumentoTipoId = documentoTipoId,
                NomeOriginalArquivo = stored.OriginalFileName,
                NomeArquivoFisico = stored.FileName,
                CaminhoArquivo = stored.RelativePath.Replace("\\", "/"),
                Extensao = stored.Extension,
                TamanhoBytes = stored.SizeBytes,
                DataHoraUpload = DbClock.Now,
                UsuarioUpload = _currentUser.Login ?? "sistema",
                StatusValidacaoDocumento = StatusValidacaoDocumento.Pendente,
                ObservacaoAvaliacao = null,
                AvaliadoPorUsuarioId = null,
                DataHoraAvaliacao = null
            };
            _context.DocumentosRegistrados.Add(novoArquivo);
            ultimoArquivoRegistrado = novoArquivo;
        }

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex, "uq_documentos_registrados_item"))
        {
            throw new AppException("Este documento já foi enviado para esta competência. Confira a lista de documentos para ver o que já está enviado e o que ainda está pendente.", 409);
        }

        envio.Status = await ResolveStatusAsync(envio.Id, fornecedor.Id, ct);
        await _context.SaveChangesAsync(ct);

        await _auditService.RegistrarAsync("documentos_registrados", ultimoArquivoRegistrado?.Id, AcaoAuditoria.Upload, $"Upload de {arquivos.Count} arquivo(s) do documento tipo {documentoTipoId}.", ct);
        var documentoTipo = await _context.DocumentosTipos.AsNoTracking().FirstAsync(x => x.Id == documentoTipoId, ct);
        var mensagemFornecedor = envio.Status == StatusEnvioMensal.Enviado
            ? $"Recebemos o documento {documentoTipo.NomeDocumento}. Todos os documentos obrigatórios da competência {envio.MesReferencia:00}/{envio.AnoReferencia} foram enviados e seguirão para análise administrativa."
            : $"Recebemos o documento {documentoTipo.NomeDocumento} da competência {envio.MesReferencia:00}/{envio.AnoReferencia}. Ainda existem documentos pendentes para completar o envio.";
        await _notificationService.NotificarFornecedorEmailSmsAsync(fornecedor, "Documento recebido", mensagemFornecedor, "documentos_enviados", envio.Id, ct);
        await _notificationService.RegistrarDashboardAdminAsync(
            "Documento enviado pelo fornecedor",
            $"{fornecedor.NomeOuRazaoSocial} enviou {arquivos.Count} arquivo(s) para {documentoTipo.NomeDocumento} na competência {envio.MesReferencia:00}/{envio.AnoReferencia}.",
            "documentos_enviados",
            envio.Id,
            ct);
        return new UploadDocumentoResponse(ultimoArquivoRegistrado?.Id ?? 0, envio.Id, envio.Status);
    }

    public async Task<DocumentoVisualizacaoDto> VisualizarDocumentoAsync(long documentoRegistradoId, CancellationToken ct)
    {
        var fornecedor = await GetCurrentFornecedorAsync(ct);
        var documento = await _context.DocumentosRegistrados.AsNoTracking()
            .Include(x => x.DocumentoEnviado)
            .FirstOrDefaultAsync(x => x.Id == documentoRegistradoId && x.DocumentoEnviado!.FornecedorId == fornecedor.Id, ct)
            ?? throw new AppException("Documento não encontrado para o fornecedor autenticado.", 404);

        var stream = await _fileStorageService.OpenReadAsync(documento.CaminhoArquivo, ct)
            ?? throw new AppException("Arquivo não encontrado no armazenamento.", 404);

        return new DocumentoVisualizacaoDto(documento.NomeOriginalArquivo, GetContentType(documento.Extensao), stream);
    }

    private static string GetContentType(string extension) =>
        extension.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };

    private static bool IsUniqueViolation(DbUpdateException exception, string constraintName)
    {
        var innerException = exception.InnerException;
        var sqlState = innerException?.GetType().GetProperty("SqlState")?.GetValue(innerException)?.ToString();
        var dbConstraintName = innerException?.GetType().GetProperty("ConstraintName")?.GetValue(innerException)?.ToString();

        return sqlState == "23505" &&
            string.Equals(dbConstraintName, constraintName, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<EnvioMensalResponse> BuildResponseAsync(long envioId, CancellationToken ct) =>
        await BuildResponseAsync(envioId, null, ct);

    private async Task<EnvioMensalResponse> BuildResponseAsync(long envioId, string? mensagem, CancellationToken ct)
    {
        var envio = await _context.DocumentosEnviados.AsNoTracking()
            .Include(x => x.Fornecedor)
            .Include(x => x.Contrato)
            .Include(x => x.DocumentosRegistrados)
            .FirstOrDefaultAsync(x => x.Id == envioId, ct)
            ?? throw new AppException("Envio não encontrado.", 404);

        var requiredDocs = await _context.DocumentosExigidos.AsNoTracking()
            .Include(x => x.DocumentoTipo)
            .Where(x =>
                x.Ativo &&
                x.CategoriaId == envio.Fornecedor!.CategoriaId &&
                x.TipoPessoa == envio.Fornecedor.TipoPessoa &&
                (envio.Fornecedor.TipoPessoa == TipoPessoa.Fisica || x.PorteEmpresa == envio.Fornecedor.PorteEmpresa))
            .ToListAsync(ct);

        var docs = requiredDocs.Select(req =>
        {
            var uploaded = envio.DocumentosRegistrados.FirstOrDefault(x => x.DocumentoTipoId == req.DocumentoTipoId);
            var enviado = uploaded is not null && uploaded.StatusValidacaoDocumento != StatusValidacaoDocumento.Reprovado;
            return new DocumentoObrigatorioDto(
                req.DocumentoTipoId,
                uploaded?.Id,
                req.DocumentoTipo!.Codigo,
                req.DocumentoTipo.NomeDocumento,
                req.Obrigatorio,
                enviado,
                uploaded?.StatusValidacaoDocumento,
                uploaded?.NomeOriginalArquivo,
                uploaded?.Extensao,
                uploaded?.TamanhoBytes,
                uploaded?.DataHoraUpload);
        }).ToList();

        return new EnvioMensalResponse(
            envio.Id,
            envio.FornecedorId,
            envio.UsuarioId,
            envio.ContratoId,
            envio.Contrato?.NumeroContrato,
            envio.Contrato?.Descricao,
            envio.MesReferencia,
            envio.AnoReferencia,
            envio.Status,
            envio.Observacao,
            mensagem,
            docs);
    }

    private async Task<Fornecedor> GetCurrentFornecedorAsync(CancellationToken ct)
    {
        if (_currentUser.Perfil != PerfilAcesso.Fornecedor || !_currentUser.FornecedorId.HasValue) throw new AppException("Acesso restrito ao fornecedor autenticado.", 403);
        return await _context.Fornecedores.FirstAsync(x => x.Id == _currentUser.FornecedorId.Value, ct);
    }

    private async Task<StatusEnvioMensal> ResolveStatusAsync(long envioId, long fornecedorId, CancellationToken ct)
    {
        var fornecedor = await _context.Fornecedores.AsNoTracking().FirstAsync(x => x.Id == fornecedorId, ct);
        var requiredIds = await _context.DocumentosExigidos.AsNoTracking()
            .Where(x => x.Ativo && x.Obrigatorio && x.CategoriaId == fornecedor.CategoriaId && x.TipoPessoa == fornecedor.TipoPessoa
                && (fornecedor.TipoPessoa == TipoPessoa.Fisica || x.PorteEmpresa == fornecedor.PorteEmpresa))
            .Select(x => x.DocumentoTipoId).ToListAsync(ct);

        var sentIds = await _context.DocumentosRegistrados.AsNoTracking()
            .Where(x => x.DocumentoEnviadoId == envioId && x.StatusValidacaoDocumento != StatusValidacaoDocumento.Reprovado)
            .Select(x => x.DocumentoTipoId).ToListAsync(ct);

        return requiredIds.All(id => sentIds.Contains(id)) ? StatusEnvioMensal.Enviado : StatusEnvioMensal.Pendente;
    }
}

public class AdminValidationService
{
    private static readonly IReadOnlyDictionary<string, string> PreviewContentTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png"
    };

    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly NotificationService _notificationService;
    private readonly IAuditService _auditService;
    private readonly IFileStorageService _fileStorageService;

    public AdminValidationService(IAppDbContext context, ICurrentUserService currentUser, NotificationService notificationService, IAuditService auditService, IFileStorageService fileStorageService)
    {
        _context = context;
        _currentUser = currentUser;
        _notificationService = notificationService;
        _auditService = auditService;
        _fileStorageService = fileStorageService;
    }

    public async Task<IReadOnlyCollection<EnvioParaValidacaoDto>> ListPendentesAsync(short mesReferencia, short anoReferencia, CancellationToken ct)
    {
        var envios = await _context.DocumentosEnviados.AsNoTracking()
            .Include(x => x.Fornecedor)
            .Include(x => x.Contrato)
            .Include(x => x.DocumentosRegistrados)
            .Where(x => x.MesReferencia == mesReferencia && x.AnoReferencia == anoReferencia)
            .OrderByDescending(x => x.DataHoraRegistro)
            .ToListAsync(ct);

        if (envios.Count == 0)
        {
            return [];
        }

        var requisitos = await _context.DocumentosExigidos.AsNoTracking()
            .Where(x => x.Ativo && x.Obrigatorio)
            .Select(x => new { x.CategoriaId, x.TipoPessoa, x.PorteEmpresa, x.DocumentoTipoId })
            .ToListAsync(ct);

        return envios
            .Where(envio =>
            {
                var fornecedor = envio.Fornecedor!;
                var obrigatorios = requisitos
                    .Where(x =>
                        x.CategoriaId == fornecedor.CategoriaId &&
                        x.TipoPessoa == fornecedor.TipoPessoa &&
                        (fornecedor.TipoPessoa == TipoPessoa.Fisica || x.PorteEmpresa == fornecedor.PorteEmpresa))
                    .Select(x => x.DocumentoTipoId)
                    .ToList();

                var enviados = envio.DocumentosRegistrados.Select(x => x.DocumentoTipoId).ToHashSet();
                return obrigatorios.Count > 0 && obrigatorios.All(enviados.Contains);
            })
            .Select(x => new EnvioParaValidacaoDto(
                x.Id,
                x.Fornecedor!.NomeOuRazaoSocial,
                x.Contrato?.NumeroContrato,
                x.MesReferencia,
                x.AnoReferencia,
                x.Status,
                x.DataHoraRegistro))
            .ToList();
    }

    public async Task<EnvioValidacaoDetalheDto> GetDetalheAsync(long envioId, CancellationToken ct)
    {
        var envio = await _context.DocumentosEnviados.AsNoTracking()
            .Include(x => x.Fornecedor)
            .Include(x => x.Contrato)
            .Include(x => x.DocumentosRegistrados)
            .ThenInclude(x => x.DocumentoTipo)
            .FirstOrDefaultAsync(x => x.Id == envioId, ct)
            ?? throw new AppException("Envio não encontrado.", 404);

        var documentos = envio.DocumentosRegistrados
            .OrderBy(x => x.DocumentoTipo!.NomeDocumento)
            .ThenByDescending(x => x.DataHoraUpload)
            .Select(x => new DocumentoValidacaoDetalheDto(
                x.Id,
                x.DocumentoTipoId,
                x.DocumentoTipo!.NomeDocumento,
                x.NomeOriginalArquivo,
                x.CaminhoArquivo,
                x.Extensao,
                x.TamanhoBytes,
                x.DataHoraUpload,
                x.StatusValidacaoDocumento,
                x.ObservacaoAvaliacao,
                x.AvaliadoPorUsuarioId,
                x.DataHoraAvaliacao))
            .ToList();

        return new EnvioValidacaoDetalheDto(
            envio.Id,
            envio.Fornecedor!.NomeOuRazaoSocial,
            envio.Contrato?.NumeroContrato,
            envio.MesReferencia,
            envio.AnoReferencia,
            envio.Status,
            envio.DataHoraRegistro,
            documentos);
    }

    public async Task<DocumentoVisualizacaoDto> VisualizarDocumentoAsync(long documentoRegistradoId, CancellationToken ct)
    {
        var documento = await _context.DocumentosRegistrados.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == documentoRegistradoId, ct)
            ?? throw new AppException("Documento não encontrado.", 404);

        var stream = await _fileStorageService.OpenReadAsync(documento.CaminhoArquivo, ct)
            ?? throw new AppException("Arquivo não encontrado no armazenamento.", 404);

        var contentType = PreviewContentTypes.TryGetValue(documento.Extensao, out var mappedContentType)
            ? mappedContentType
            : "application/octet-stream";

        return new DocumentoVisualizacaoDto(documento.NomeOriginalArquivo, contentType, stream);
    }

    public async Task ValidarDocumentoAsync(long documentoRegistradoId, ValidarDocumentoRequest request, CancellationToken ct)
    {
        if (request.Status == StatusValidacaoDocumento.Pendente)
        {
            throw new AppException("Na validação administrativa o documento só pode ser aprovado ou reprovado.", 400);
        }

        if (request.Status == StatusValidacaoDocumento.Reprovado && string.IsNullOrWhiteSpace(request.ObservacaoAvaliacao))
        {
            throw new AppException("Informe a justificativa para reprovar o documento.", 400);
        }

        var documento = await _context.DocumentosRegistrados
            .Include(x => x.DocumentoEnviado)
            .ThenInclude(x => x!.Fornecedor)
            .Include(x => x.DocumentoTipo)
            .FirstOrDefaultAsync(x => x.Id == documentoRegistradoId, ct)
            ?? throw new AppException("Documento não encontrado.", 404);

        if (request.Status == StatusValidacaoDocumento.Reprovado)
        {
            var nomeDocumentoReprovado = documento.DocumentoTipo?.NomeDocumento ?? "Documento";
            var envioReprovado = documento.DocumentoEnviado!;

            documento.StatusValidacaoDocumento = request.Status;
            documento.ObservacaoAvaliacao = request.ObservacaoAvaliacao;
            documento.AvaliadoPorUsuarioId = _currentUser.UserId;
            documento.DataHoraAvaliacao = DbClock.Now;
            await AtualizarStatusEnvioAsync(envioReprovado, ct, [nomeDocumentoReprovado]);
            await _context.SaveChangesAsync(ct);

            await _auditService.RegistrarAsync("documentos_registrados", documentoRegistradoId, AcaoAuditoria.Validacao, $"Documento reprovado para reenvio: {nomeDocumentoReprovado}.", ct);
            return;
        }

        documento.StatusValidacaoDocumento = request.Status;
        documento.ObservacaoAvaliacao = request.ObservacaoAvaliacao;
        documento.AvaliadoPorUsuarioId = _currentUser.UserId;
        documento.DataHoraAvaliacao = DbClock.Now;

        var envio = documento.DocumentoEnviado!;
        await AtualizarStatusEnvioAsync(envio, ct);
        await _context.SaveChangesAsync(ct);

        await _auditService.RegistrarAsync("documentos_registrados", documento.Id, AcaoAuditoria.Validacao, $"Documento validado com status {request.Status}.", ct);
        if (envio.Status != StatusEnvioMensal.EmConformidade)
        {
            var fornecedor = envio.Fornecedor ?? await _context.Fornecedores.FirstAsync(x => x.Id == envio.FornecedorId, ct);
            var nomeDocumento = documento.DocumentoTipo?.NomeDocumento ?? "Documento";
            var mensagem = $"O documento {nomeDocumento} da competência {envio.MesReferencia:00}/{envio.AnoReferencia} foi aprovado pela administração.";
            await _notificationService.NotificarFornecedorEmailSmsAsync(fornecedor, "Documento aprovado", mensagem, "documentos_registrados", documento.Id, ct);
            await _notificationService.RegistrarDashboardAdminAsync(
                "Documento aprovado",
                $"{fornecedor.NomeOuRazaoSocial}: {nomeDocumento} aprovado na competência {envio.MesReferencia:00}/{envio.AnoReferencia}.",
                "documentos_registrados",
                documento.Id,
                ct);
        }
    }

    private async Task AtualizarStatusEnvioAsync(DocumentoEnviado envio, CancellationToken ct, IReadOnlyCollection<string>? documentosReprovadosForcados = null)
    {
        await _context.Entry(envio).Reference(x => x.Fornecedor!).LoadAsync(ct);

        var requiredDocs = await _context.DocumentosExigidos.AsNoTracking()
            .Where(x => x.Ativo && x.Obrigatorio && x.CategoriaId == envio.Fornecedor!.CategoriaId && x.TipoPessoa == envio.Fornecedor.TipoPessoa &&
                (envio.Fornecedor.TipoPessoa == TipoPessoa.Fisica || x.PorteEmpresa == envio.Fornecedor.PorteEmpresa))
            .Select(x => new { x.DocumentoTipoId, Nome = x.DocumentoTipo!.NomeDocumento })
            .ToListAsync(ct);

        var requiredIds = requiredDocs.Select(x => x.DocumentoTipoId).ToList();

        var registered = await _context.DocumentosRegistrados
            .Include(x => x.DocumentoTipo)
            .Where(x => x.DocumentoEnviadoId == envio.Id)
            .ToListAsync(ct);

        var documentosFaltantes = requiredDocs
            .Where(req => registered.All(x => x.DocumentoTipoId != req.DocumentoTipoId))
            .Select(req => req.Nome)
            .Distinct()
            .ToList();

        var documentosReprovados = registered
            .Where(x => requiredIds.Contains(x.DocumentoTipoId) && x.StatusValidacaoDocumento == StatusValidacaoDocumento.Reprovado)
            .Select(x => x.DocumentoTipo!.NomeDocumento)
            .Distinct()
            .ToList();

        if (documentosReprovadosForcados is not null && documentosReprovadosForcados.Count != 0)
        {
            documentosReprovados = documentosReprovados
                .Concat(documentosReprovadosForcados)
                .Distinct()
                .ToList();
        }

        var documentosPendentes = registered
            .Where(x =>
                requiredIds.Contains(x.DocumentoTipoId) &&
                x.StatusValidacaoDocumento == StatusValidacaoDocumento.Pendente &&
                x.DataHoraAvaliacao.HasValue)
            .Select(x => x.DocumentoTipo!.NomeDocumento)
            .Distinct()
            .ToList();

        var missingRequired = documentosFaltantes.Count != 0;
        var reprovado = documentosReprovados.Count != 0;
        var pendenteAposAvaliacao = registered.Any(x =>
            requiredIds.Contains(x.DocumentoTipoId) &&
            x.StatusValidacaoDocumento == StatusValidacaoDocumento.Pendente &&
            x.DataHoraAvaliacao.HasValue);
        var allApproved = requiredIds.All(req => registered.Any(x => x.DocumentoTipoId == req && x.StatusValidacaoDocumento == StatusValidacaoDocumento.Aprovado));

        if (missingRequired || reprovado || pendenteAposAvaliacao)
        {
            envio.Status = StatusEnvioMensal.Pendente;
            await NotificarPendenciaAsync(envio, documentosFaltantes, documentosPendentes, documentosReprovados, ct);
            return;
        }

        if (allApproved)
        {
            envio.Status = StatusEnvioMensal.EmConformidade;
            envio.AvaliadoPorUsuarioId = _currentUser.UserId;
            envio.DataHoraValidacaoFinal = DbClock.Now;

            var liberacaoExistente = await _context.FinanceiroLiberacoes.AnyAsync(x => x.DocumentoEnviadoId == envio.Id, ct);
            if (!liberacaoExistente)
            {
                _context.FinanceiroLiberacoes.Add(new FinanceiroLiberacao
                {
                    DocumentoEnviadoId = envio.Id,
                    FornecedorId = envio.FornecedorId,
                    ContratoId = envio.ContratoId,
                    GeradoPorUsuarioId = _currentUser.UserId ?? 0,
                    StatusFinanceiro = StatusFinanceiro.AguardandoEnvioNf,
                    Observacao = "Gerado automaticamente após conformidade documental."
                });
            }

            var fornecedor = envio.Fornecedor!;
            var mensagem = $"Todos os documentos da competência {envio.MesReferencia:00}/{envio.AnoReferencia} foram aprovados. Você está liberado para enviar a nota fiscal ao financeiro.";
            await _notificationService.NotificarFornecedorEmailSmsAsync(fornecedor, "Fornecedor em conformidade", mensagem, "documentos_enviados", envio.Id, ct);
            await _notificationService.RegistrarDashboardAdminAsync(
                "Fornecedor em conformidade",
                $"{fornecedor.NomeOuRazaoSocial} ficou em conformidade na competência {envio.MesReferencia:00}/{envio.AnoReferencia} e foi liberado para envio de NF.",
                "documentos_enviados",
                envio.Id,
                ct);
        }
    }

    private async Task NotificarPendenciaAsync(
        DocumentoEnviado envio,
        IReadOnlyCollection<string> documentosFaltantes,
        IReadOnlyCollection<string> documentosPendentes,
        IReadOnlyCollection<string> documentosReprovados,
        CancellationToken ct)
    {
        var fornecedor = envio.Fornecedor ?? await _context.Fornecedores.FirstAsync(x => x.Id == envio.FornecedorId, ct);
        var pendencias = new List<string>();

        if (documentosFaltantes.Count != 0)
        {
            pendencias.Add($"Faltantes: {string.Join(", ", documentosFaltantes)}.");
        }

        if (documentosPendentes.Count != 0)
        {
            pendencias.Add($"Pendentes para reenvio: {string.Join(", ", documentosPendentes)}.");
        }

        if (documentosReprovados.Count != 0)
        {
            pendencias.Add($"Reprovados: {string.Join(", ", documentosReprovados)}.");
        }

        var texto = $"Há pendências no envio documental mensal da competência {envio.MesReferencia:00}/{envio.AnoReferencia}. {string.Join(" ", pendencias)}".Trim();
        await _notificationService.NotificarFornecedorEmailSmsAsync(fornecedor, "Pendência documental", texto, "documentos_enviados", envio.Id, ct);
        await _notificationService.RegistrarDashboardAdminAsync(
            "Pendência documental",
            $"{fornecedor.NomeOuRazaoSocial} possui pendência documental na competência {envio.MesReferencia:00}/{envio.AnoReferencia}. {string.Join(" ", pendencias)}".Trim(),
            "documentos_enviados",
            envio.Id,
            ct);
    }
}

public class FinanceiroService
{
    private static readonly string[] NotaFiscalExtensions = [".pdf", ".xml"];
    private const long MaxNotaFiscalFileSize = 10 * 1024 * 1024;
    private readonly IAppDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorageService;
    private readonly NotificationService _notificationService;

    public FinanceiroService(IAppDbContext context, IAuditService auditService, ICurrentUserService currentUser, IFileStorageService fileStorageService, NotificationService notificationService)
    {
        _context = context;
        _auditService = auditService;
        _currentUser = currentUser;
        _fileStorageService = fileStorageService;
        _notificationService = notificationService;
    }

    public async Task<IReadOnlyCollection<FinanceiroLiberacaoResponse>> ListAsync(short mesReferencia, short anoReferencia, CancellationToken ct) =>
        await _context.FinanceiroLiberacoes.AsNoTracking()
            .Include(x => x.Fornecedor)
            .Include(x => x.Contrato)
            .Include(x => x.DocumentoEnviado)
            .Where(x => x.DocumentoEnviado!.MesReferencia == mesReferencia && x.DocumentoEnviado.AnoReferencia == anoReferencia)
            .OrderByDescending(x => x.DataHoraGeracao)
            .Select(x => new FinanceiroLiberacaoResponse(
                x.Id,
                x.DocumentoEnviadoId,
                x.Fornecedor!.NomeOuRazaoSocial,
                x.Contrato != null ? x.Contrato.NumeroContrato : null,
                x.DocumentoEnviado!.MesReferencia,
                x.DocumentoEnviado.AnoReferencia,
                x.StatusFinanceiro,
                x.NumeroNotaFiscal,
                x.NomeOriginalNotaFiscal,
                x.ExtensaoNotaFiscal,
                x.TamanhoBytesNotaFiscal,
                x.DataHoraUploadNotaFiscal,
                x.DataHoraGeracao))
            .ToListAsync(ct);

    public async Task<IReadOnlyCollection<FinanceiroLiberacaoResponse>> ListFornecedorAsync(CancellationToken ct)
    {
        var fornecedorId = _currentUser.FornecedorId ?? throw new AppException("Fornecedor não identificado.", 403);

        return await _context.FinanceiroLiberacoes.AsNoTracking()
            .Include(x => x.Fornecedor)
            .Include(x => x.Contrato)
            .Include(x => x.DocumentoEnviado)
            .Where(x => x.FornecedorId == fornecedorId)
            .OrderByDescending(x => x.DocumentoEnviado!.AnoReferencia)
            .ThenByDescending(x => x.DocumentoEnviado!.MesReferencia)
            .Select(x => new FinanceiroLiberacaoResponse(
                x.Id,
                x.DocumentoEnviadoId,
                x.Fornecedor!.NomeOuRazaoSocial,
                x.Contrato != null ? x.Contrato.NumeroContrato : null,
                x.DocumentoEnviado!.MesReferencia,
                x.DocumentoEnviado.AnoReferencia,
                x.StatusFinanceiro,
                x.NumeroNotaFiscal,
                x.NomeOriginalNotaFiscal,
                x.ExtensaoNotaFiscal,
                x.TamanhoBytesNotaFiscal,
                x.DataHoraUploadNotaFiscal,
                x.DataHoraGeracao))
            .ToListAsync(ct);
    }

    public async Task EnviarNotaFiscalAsync(long id, EnviarNotaFiscalRequest request, CancellationToken ct)
    {
        var fornecedorId = _currentUser.FornecedorId ?? throw new AppException("Fornecedor não identificado.", 403);
        if (string.IsNullOrWhiteSpace(request.NumeroNotaFiscal)) throw new AppException("Número da nota fiscal é obrigatório.");

        var entity = await _context.FinanceiroLiberacoes
            .Include(x => x.Fornecedor)
            .Include(x => x.DocumentoEnviado)
            .FirstOrDefaultAsync(x => x.Id == id && x.FornecedorId == fornecedorId, ct)
            ?? throw new AppException("Liberação financeira não encontrada.", 404);

        if (entity.StatusFinanceiro != StatusFinanceiro.AguardandoEnvioNf)
        {
            throw new AppException("Esta liberação não está aguardando nota fiscal.");
        }

        var arquivo = request.ArquivoNotaFiscal ?? throw new AppException("Anexe a nota fiscal em PDF ou XML.");
        if (arquivo.Length == 0 || arquivo.Length > MaxNotaFiscalFileSize) throw new AppException("Arquivo da nota fiscal inválido ou acima do limite de 10MB.");
        var extension = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
        if (!NotaFiscalExtensions.Contains(extension)) throw new AppException("A nota fiscal deve ser enviada em PDF ou XML.");

        var envio = entity.DocumentoEnviado ?? throw new AppException("Envio documental vinculado não encontrado.", 404);
        var fornecedor = entity.Fornecedor ?? await _context.Fornecedores.FirstAsync(x => x.Id == fornecedorId, ct);
        var stored = await _fileStorageService.SaveAsync(fornecedor.CodigoFornecedor, envio.AnoReferencia, envio.MesReferencia, arquivo, ct);

        entity.NumeroNotaFiscal = request.NumeroNotaFiscal.Trim();
        entity.Observacao = request.Observacao;
        entity.NomeOriginalNotaFiscal = stored.OriginalFileName;
        entity.NomeArquivoFisicoNotaFiscal = stored.FileName;
        entity.CaminhoArquivoNotaFiscal = stored.RelativePath.Replace("\\", "/");
        entity.ExtensaoNotaFiscal = stored.Extension;
        entity.TamanhoBytesNotaFiscal = stored.SizeBytes;
        entity.StatusFinanceiro = StatusFinanceiro.AguardandoPagamento;
        entity.DataRecebimentoNotaFiscal = DbClock.Now;
        entity.DataHoraUploadNotaFiscal = DbClock.Now;
        entity.DataHoraAtualizacao = DbClock.Now;

        await _context.SaveChangesAsync(ct);
        await _auditService.RegistrarAsync("financeiro_liberacoes", entity.Id, AcaoAuditoria.LiberacaoFinanceiro, $"Nota fiscal {entity.NumeroNotaFiscal} enviada pelo fornecedor.", ct);
        var mensagem = $"Recebemos a nota fiscal {entity.NumeroNotaFiscal} da competência {envio.MesReferencia:00}/{envio.AnoReferencia}. Ela foi encaminhada para análise do financeiro.";
        await _notificationService.NotificarFornecedorEmailSmsAsync(fornecedor, "Nota fiscal recebida", mensagem, "financeiro_liberacoes", entity.Id, ct);
        await _notificationService.RegistrarDashboardAdminAsync(
            "Nota fiscal enviada",
            $"{fornecedor.NomeOuRazaoSocial} enviou a NF {entity.NumeroNotaFiscal} da competência {envio.MesReferencia:00}/{envio.AnoReferencia}.",
            "financeiro_liberacoes",
            entity.Id,
            ct);
    }

    public async Task AtualizarAsync(long id, AtualizarFinanceiroRequest request, CancellationToken ct)
    {
        var entity = await _context.FinanceiroLiberacoes.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException("Registro financeiro não encontrado.", 404);

        entity.StatusFinanceiro = request.StatusFinanceiro;
        entity.NumeroNotaFiscal = request.NumeroNotaFiscal;
        entity.Observacao = request.Observacao;
        if (!string.IsNullOrWhiteSpace(request.NumeroNotaFiscal)) entity.DataRecebimentoNotaFiscal = DbClock.Now;
        entity.DataHoraAtualizacao = DbClock.Now;

        await _context.SaveChangesAsync(ct);
        await _auditService.RegistrarAsync("financeiro_liberacoes", entity.Id, AcaoAuditoria.LiberacaoFinanceiro, $"Status financeiro alterado para {entity.StatusFinanceiro}.", ct);
    }
}

public class DashboardService
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DashboardService(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<DashboardAdminDto> AdminAsync(CancellationToken ct) =>
        new(
            await _context.Fornecedores.CountAsync(ct),
            await _context.DocumentosEnviados.CountAsync(x => x.Status == StatusEnvioMensal.Pendente, ct),
            await _context.DocumentosEnviados.CountAsync(x => x.Status == StatusEnvioMensal.Enviado, ct),
            await _context.DocumentosEnviados.CountAsync(x => x.Status == StatusEnvioMensal.EmConformidade, ct),
            await _context.Contratos.CountAsync(x => x.Ativo, ct),
            await _context.Notificacoes.CountAsync(x => x.TipoNotificacao == TipoNotificacao.Sistema && x.StatusEnvio == StatusNotificacao.Pendente, ct));

    public async Task<DashboardFornecedorDto> FornecedorAsync(CancellationToken ct)
    {
        var fornecedorId = _currentUser.FornecedorId ?? throw new AppException("Fornecedor não identificado.", 403);
        var now = DateTime.UtcNow;
        var envio = await _context.DocumentosEnviados.AsNoTracking().FirstOrDefaultAsync(x => x.FornecedorId == fornecedorId && x.MesReferencia == now.Month && x.AnoReferencia == now.Year, ct);
        var docsFaltantes = 0;

        if (envio is not null)
        {
            var fornecedor = await _context.Fornecedores.AsNoTracking().FirstAsync(x => x.Id == fornecedorId, ct);
            var required = await _context.DocumentosExigidos.AsNoTracking()
                .Where(x => x.Ativo && x.Obrigatorio && x.CategoriaId == fornecedor.CategoriaId && x.TipoPessoa == fornecedor.TipoPessoa &&
                    (fornecedor.TipoPessoa == TipoPessoa.Fisica || x.PorteEmpresa == fornecedor.PorteEmpresa))
                .CountAsync(ct);
            var uploaded = await _context.DocumentosRegistrados.AsNoTracking().CountAsync(x => x.DocumentoEnviadoId == envio.Id, ct);
            docsFaltantes = Math.Max(required - uploaded, 0);
        }

        return new DashboardFornecedorDto(
            envio?.Status.ToString() ?? "SEM_ENVIO",
            docsFaltantes,
            await _context.DocumentosEnviados.CountAsync(x => x.FornecedorId == fornecedorId, ct),
            await _context.Notificacoes.CountAsync(x => x.FornecedorId == fornecedorId, ct),
            await _context.FinanceiroLiberacoes.CountAsync(x => x.FornecedorId == fornecedorId && x.StatusFinanceiro == StatusFinanceiro.AguardandoEnvioNf, ct));
    }

    public async Task<DashboardFinanceiroDto> FinanceiroAsync(CancellationToken ct) =>
        new(
            await _context.DocumentosEnviados.CountAsync(x => x.Status == StatusEnvioMensal.EmConformidade, ct),
            await _context.FinanceiroLiberacoes.CountAsync(x => x.StatusFinanceiro == StatusFinanceiro.AguardandoEnvioNf, ct),
            await _context.FinanceiroLiberacoes.CountAsync(x => x.StatusFinanceiro == StatusFinanceiro.EmAnaliseFinanceira, ct),
            await _context.FinanceiroLiberacoes.CountAsync(x => x.StatusFinanceiro == StatusFinanceiro.LiberadoParaPagamento, ct),
            await _context.FinanceiroLiberacoes.CountAsync(x => x.StatusFinanceiro == StatusFinanceiro.Pago, ct));
}
