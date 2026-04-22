using Dalba.Financeiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Dalba.Financeiro.Application.Abstractions.Persistence;

public interface IAppDbContext
{
    DbSet<Usuario> Usuarios { get; }
    DbSet<Fornecedor> Fornecedores { get; }
    DbSet<Categoria> Categorias { get; }
    DbSet<Contrato> Contratos { get; }
    DbSet<DocumentoTipo> DocumentosTipos { get; }
    DbSet<DocumentoExigido> DocumentosExigidos { get; }
    DbSet<DocumentoEnviado> DocumentosEnviados { get; }
    DbSet<DocumentoRegistrado> DocumentosRegistrados { get; }
    DbSet<Notificacao> Notificacoes { get; }
    DbSet<FinanceiroLiberacao> FinanceiroLiberacoes { get; }
    DbSet<LogAuditoria> LogsAuditoria { get; }
    DbSet<ParametroSistema> ParametrosSistema { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    EntityEntry Entry(object entity);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
