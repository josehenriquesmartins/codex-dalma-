using Dalba.Financeiro.Application.Abstractions.Audit;
using Dalba.Financeiro.Application.Abstractions.Notifications;
using Dalba.Financeiro.Application.Abstractions.Persistence;
using Dalba.Financeiro.Application.Abstractions.Security;
using Dalba.Financeiro.Application.Common;
using Dalba.Financeiro.Application.DTOs.Auth;
using Dalba.Financeiro.Domain.Entities;
using Dalba.Financeiro.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Dalba.Financeiro.Application.Services;

public class AuthService
{
    private readonly IAppDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IAuditService _auditService;
    private readonly INotificationDispatcher _notificationDispatcher;

    public AuthService(IAppDbContext context, IJwtTokenGenerator jwtTokenGenerator, IAuditService auditService, INotificationDispatcher notificationDispatcher)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
        _auditService = auditService;
        _notificationDispatcher = notificationDispatcher;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var loginNormalizado = request.Login.Trim();
        var senhaOriginal = request.Senha ?? string.Empty;
        var senhaNormalizada = senhaOriginal.Trim();

        var usuario = await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Login.ToLower() == loginNormalizado.ToLower() && x.Ativo, cancellationToken)
            ?? null;

        if (usuario is null)
        {
            return null;
        }

        var senhaHashOriginal = SecurityHelper.ComputeSha256(senhaOriginal);
        var senhaHashNormalizada = SecurityHelper.ComputeSha256(senhaNormalizada);

        if (usuario.SenhaHashSha256 != senhaHashOriginal && usuario.SenhaHashSha256 != senhaHashNormalizada)
        {
            return null;
        }

        var token = _jwtTokenGenerator.GenerateToken(usuario);
        await _auditService.RegistrarAsync("usuarios", usuario.Id, AcaoAuditoria.Login, $"Login realizado pelo usuário {usuario.Login}.", cancellationToken);

        return new AuthResponse(token, usuario.Nome, usuario.Email, usuario.Perfil, usuario.FornecedorId, DateTime.UtcNow.AddHours(8));
    }

    public async Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var identificador = request.LoginOuEmail.Trim();
        if (string.IsNullOrWhiteSpace(identificador))
        {
            return new ForgotPasswordResponse("Informe o login ou e-mail.", false);
        }

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(x =>
                x.Ativo &&
                (x.Login.ToLower() == identificador.ToLower() || x.Email.ToLower() == identificador.ToLower()),
                cancellationToken);

        const string respostaPadrao = "Se o usuário existir, enviaremos um token de recuperação para o e-mail cadastrado.";
        if (usuario is null)
        {
            return new ForgotPasswordResponse(respostaPadrao);
        }

        if (usuario.Email.EndsWith(".local", StringComparison.OrdinalIgnoreCase))
        {
            return new ForgotPasswordResponse($"O usuário {usuario.Login} está cadastrado com o e-mail interno {usuario.Email}. Atualize o e-mail do usuário para um endereço real antes de solicitar a recuperação.", false, usuario.Email);
        }

        var token = GenerateResetToken();
        var tokenHash = SecurityHelper.ComputeSha256(token);
        var agora = DbClock.Now;
        var expiraEm = agora.AddMinutes(30);

        var tokensPendentes = await _context.PasswordResetTokens
            .Where(x => x.UsuarioId == usuario.Id && x.UtilizadoEmUtc == null && x.ExpiraEmUtc > agora)
            .ToListAsync(cancellationToken);

        foreach (var tokenPendente in tokensPendentes)
        {
            tokenPendente.UtilizadoEmUtc = agora;
            tokenPendente.DataHoraAtualizacao = agora;
        }

        _context.PasswordResetTokens.Add(new PasswordResetToken
        {
            UsuarioId = usuario.Id,
            TokenHashSha256 = tokenHash,
            ExpiraEmUtc = expiraEm
        });

        await _context.SaveChangesAsync(cancellationToken);

        var linkRedefinicao = $"http://localhost:4200/redefinir-senha?token={Uri.EscapeDataString(token)}";
        var mensagem = $"Olá, {usuario.Nome}.\n\nClique no link abaixo para cadastrar uma nova senha:\n{linkRedefinicao}\n\nToken de recuperação: {token}\n\nEle expira em 30 minutos. Se você não solicitou a recuperação, ignore este e-mail.";
        var result = await _notificationDispatcher.DispatchAsync(TipoNotificacao.Email, usuario.Email, "Token de recuperação de senha", mensagem, cancellationToken);

        if (!result.Success)
        {
            return new ForgotPasswordResponse("Não foi possível enviar o e-mail de recuperação. Verifique a configuração SMTP.", false);
        }

        await _auditService.RegistrarAsync("usuarios", usuario.Id, AcaoAuditoria.Notificacao, $"Token de recuperação enviado para {usuario.Email}.", cancellationToken);

        return new ForgotPasswordResponse($"Token de recuperação enviado para {MaskEmail(usuario.Email)}.", true, usuario.Email);
    }

    public async Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var token = request.Token.Trim();
        var novaSenha = request.NovaSenha ?? string.Empty;
        var confirmacaoSenha = request.ConfirmacaoSenha ?? string.Empty;

        if (string.IsNullOrWhiteSpace(token))
        {
            return new ResetPasswordResponse("Informe o token de recuperação.", false);
        }

        if (string.IsNullOrWhiteSpace(novaSenha) || novaSenha.Length < 6)
        {
            return new ResetPasswordResponse("A nova senha deve ter pelo menos 6 caracteres.", false);
        }

        if (novaSenha != confirmacaoSenha)
        {
            return new ResetPasswordResponse("A confirmação da senha não confere.", false);
        }

        var agora = DbClock.Now;
        var tokenHash = SecurityHelper.ComputeSha256(token);
        var resetToken = await _context.PasswordResetTokens
            .Include(x => x.Usuario)
            .FirstOrDefaultAsync(x => x.TokenHashSha256 == tokenHash && x.UtilizadoEmUtc == null && x.ExpiraEmUtc > agora, cancellationToken);

        if (resetToken is null || !resetToken.Usuario.Ativo)
        {
            return new ResetPasswordResponse("Token inválido ou expirado. Solicite uma nova recuperação de senha.", false);
        }

        resetToken.Usuario.SenhaHashSha256 = SecurityHelper.ComputeSha256(novaSenha);
        resetToken.Usuario.DataHoraAtualizacao = agora;
        resetToken.UtilizadoEmUtc = agora;
        resetToken.DataHoraAtualizacao = agora;

        await _context.SaveChangesAsync(cancellationToken);
        await _auditService.RegistrarAsync("usuarios", resetToken.UsuarioId, AcaoAuditoria.Edicao, "Senha redefinida por token de recuperação.", cancellationToken);

        return new ResetPasswordResponse("Senha redefinida com sucesso. Você já pode acessar o sistema.");
    }

    private static string GenerateResetToken()
    {
        Span<byte> bytes = stackalloc byte[24];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).Replace("+", string.Empty).Replace("/", string.Empty).Replace("=", string.Empty);
    }

    private static string MaskEmail(string email)
    {
        var parts = email.Split('@', 2);
        if (parts.Length != 2 || parts[0].Length == 0)
        {
            return email;
        }

        var visible = parts[0].Length == 1 ? parts[0] : parts[0][..Math.Min(2, parts[0].Length)];
        return $"{visible}***@{parts[1]}";
    }
}
