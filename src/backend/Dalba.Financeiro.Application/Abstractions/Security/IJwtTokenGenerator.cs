using Dalba.Financeiro.Domain.Entities;

namespace Dalba.Financeiro.Application.Abstractions.Security;

public interface IJwtTokenGenerator
{
    string GenerateToken(Usuario usuario);
}
