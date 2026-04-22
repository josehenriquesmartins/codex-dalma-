using Dalba.Financeiro.Domain.Enums;

namespace Dalba.Financeiro.Application.Abstractions.Security;

public interface ICurrentUserService
{
    long? UserId { get; }
    string? Login { get; }
    PerfilAcesso? Perfil { get; }
    long? FornecedorId { get; }
    string? IpAddress { get; }
}
