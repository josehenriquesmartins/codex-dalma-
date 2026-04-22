using Dalba.Financeiro.Application.Abstractions.Audit;
using Dalba.Financeiro.Application.Abstractions.Persistence;
using Dalba.Financeiro.Application.Abstractions.Security;
using Dalba.Financeiro.Domain.Entities;
using Dalba.Financeiro.Domain.Enums;

namespace Dalba.Financeiro.Infrastructure.Audit;

public class AuditService : IAuditService
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AuditService(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task RegistrarAsync(string entidade, long? entidadeId, AcaoAuditoria acao, string dadosResumidos, CancellationToken cancellationToken)
    {
        _context.LogsAuditoria.Add(new LogAuditoria
        {
            UsuarioId = _currentUser.UserId,
            Entidade = entidade,
            EntidadeId = entidadeId,
            Acao = acao,
            DadosResumidos = dadosResumidos,
            IpOrigem = _currentUser.IpAddress
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
