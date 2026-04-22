using Dalba.Financeiro.Application.Abstractions.Persistence;
using Dalba.Financeiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Dalba.Financeiro.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Contrato> Contratos => Set<Contrato>();
    public DbSet<DocumentoTipo> DocumentosTipos => Set<DocumentoTipo>();
    public DbSet<DocumentoExigido> DocumentosExigidos => Set<DocumentoExigido>();
    public DbSet<DocumentoEnviado> DocumentosEnviados => Set<DocumentoEnviado>();
    public DbSet<DocumentoRegistrado> DocumentosRegistrados => Set<DocumentoRegistrado>();
    public DbSet<Notificacao> Notificacoes => Set<Notificacao>();
    public DbSet<FinanceiroLiberacao> FinanceiroLiberacoes => Set<FinanceiroLiberacao>();
    public DbSet<LogAuditoria> LogsAuditoria => Set<LogAuditoria>();
    public DbSet<ParametroSistema> ParametrosSistema => Set<ParametroSistema>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    public new EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class => base.Entry(entity);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
