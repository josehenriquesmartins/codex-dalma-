using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dalba.Financeiro.Application.Abstractions.Security;
using Dalba.Financeiro.Domain.Entities;
using Dalba.Financeiro.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Dalba.Financeiro.Infrastructure.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _settings;

    public JwtTokenGenerator(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public string GenerateToken(Usuario usuario)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Login),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Perfil.ToString())
        };

        if (usuario.FornecedorId.HasValue)
        {
            claims.Add(new Claim("fornecedorId", usuario.FornecedorId.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_settings.Issuer, _settings.Audience, claims, expires: DateTime.UtcNow.AddHours(_settings.ExpirationHours), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
