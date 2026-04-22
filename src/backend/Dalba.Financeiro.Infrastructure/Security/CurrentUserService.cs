using System.Security.Claims;
using Dalba.Financeiro.Application.Abstractions.Security;
using Dalba.Financeiro.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Dalba.Financeiro.Infrastructure.Security;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long? UserId => ParseLong(ClaimTypes.NameIdentifier);
    public string? Login => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
    public PerfilAcesso? Perfil => Enum.TryParse<PerfilAcesso>(_httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role), out var value) ? value : null;
    public long? FornecedorId => ParseLong("fornecedorId");
    public string? IpAddress => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    private long? ParseLong(string claim)
    {
        var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(claim);
        return long.TryParse(value, out var result) ? result : null;
    }
}
