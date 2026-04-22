using Dalba.Financeiro.Domain.Enums;

namespace Dalba.Financeiro.Application.DTOs.Auth;

public sealed record LoginRequest(string Login, string Senha);

public sealed record ForgotPasswordRequest(string LoginOuEmail);

public sealed record ResetPasswordRequest(string Token, string NovaSenha, string ConfirmacaoSenha);

public sealed record AuthResponse(
    string Token,
    string Nome,
    string Email,
    PerfilAcesso Perfil,
    long? FornecedorId,
    DateTime ExpiresAtUtc);

public sealed record ForgotPasswordResponse(string Message, bool Success = true, string? Email = null);

public sealed record ResetPasswordResponse(string Message, bool Success = true);
