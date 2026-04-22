using Dalba.Financeiro.Domain.Common;
using Dalba.Financeiro.Domain.Entities;
using Dalba.Financeiro.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dalba.Financeiro.Infrastructure.Persistence.EntityConfigurations;

internal static class EntityConfigurationExtensions
{
    public static void ConfigureBase<T>(this EntityTypeBuilder<T> builder, string tableName, string sequenceName) where T : BaseEntity
    {
        builder.ToTable(tableName);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasDefaultValueSql($"nextval('{sequenceName}')");
        builder.Property(x => x.DataHoraCriacao).HasColumnName("data_hora_criacao").HasColumnType("timestamp").IsRequired();
        builder.Property(x => x.DataHoraAtualizacao).HasColumnName("data_hora_atualizacao").HasColumnType("timestamp");
    }
}

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ConfigureBase("categorias", "sq_categorias");
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(30).IsRequired();
        builder.Property(x => x.Descricao).HasColumnName("descricao").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Ativo).HasColumnName("ativo").IsRequired();
        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class FornecedorConfiguration : IEntityTypeConfiguration<Fornecedor>
{
    public void Configure(EntityTypeBuilder<Fornecedor> builder)
    {
        builder.ConfigureBase("fornecedores", "sq_fornecedores");
        builder.Property(x => x.CodigoFornecedor).HasColumnName("codigo_fornecedor").HasMaxLength(20).IsRequired();
        builder.Property(x => x.TipoPessoa).HasColumnName("tipo_pessoa").HasConversion<int>().IsRequired();
        builder.Property(x => x.PorteEmpresa).HasColumnName("porte_empresa").HasConversion<int?>();
        builder.Property(x => x.CategoriaId).HasColumnName("categoria_id").IsRequired();
        builder.Property(x => x.NomeOuRazaoSocial).HasColumnName("nome_ou_razao_social").HasMaxLength(200).IsRequired();
        builder.Property(x => x.NomeFantasia).HasColumnName("nome_fantasia").HasMaxLength(200);
        builder.Property(x => x.CpfOuCnpj).HasColumnName("cpf_ou_cnpj").HasMaxLength(18).IsRequired();
        builder.Property(x => x.DdiTelefone).HasColumnName("ddi_telefone").HasMaxLength(5).IsRequired();
        builder.Property(x => x.DddTelefone).HasColumnName("ddd_telefone").HasMaxLength(4).IsRequired();
        builder.Property(x => x.NumeroTelefone).HasColumnName("numero_telefone").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(160).IsRequired();
        builder.Property(x => x.Cep).HasColumnName("cep").HasMaxLength(12).IsRequired();
        builder.Property(x => x.Logradouro).HasColumnName("logradouro").HasMaxLength(160).IsRequired();
        builder.Property(x => x.Numero).HasColumnName("numero").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Complemento).HasColumnName("complemento").HasMaxLength(120);
        builder.Property(x => x.Bairro).HasColumnName("bairro").HasMaxLength(120).IsRequired();
        builder.Property(x => x.Cidade).HasColumnName("cidade").HasMaxLength(120).IsRequired();
        builder.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(2).IsRequired();
        builder.Property(x => x.Pais).HasColumnName("pais").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Ativo).HasColumnName("ativo").IsRequired();
        builder.HasIndex(x => x.CodigoFornecedor).IsUnique();
        builder.HasIndex(x => x.CpfOuCnpj).IsUnique();
        builder.HasOne(x => x.Categoria).WithMany(x => x.Fornecedores).HasForeignKey(x => x.CategoriaId);
    }
}

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ConfigureBase("usuarios", "sq_usuarios");
        builder.Property(x => x.Nome).HasColumnName("nome").HasMaxLength(160).IsRequired();
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(160).IsRequired();
        builder.Property(x => x.Login).HasColumnName("login").HasMaxLength(60).IsRequired();
        builder.Property(x => x.SenhaHashSha256).HasColumnName("senha_hash_sha256").HasMaxLength(64).IsRequired();
        builder.Property(x => x.Perfil).HasColumnName("perfil").HasConversion<int>().IsRequired();
        builder.Property(x => x.FornecedorId).HasColumnName("fornecedor_id");
        builder.Property(x => x.Ativo).HasColumnName("ativo").IsRequired();
        builder.HasIndex(x => x.Login).IsUnique();
        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasOne(x => x.Fornecedor).WithMany(x => x.Usuarios).HasForeignKey(x => x.FornecedorId);
    }
}

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ConfigureBase("password_reset_tokens", "sq_password_reset_tokens");
        builder.Property(x => x.UsuarioId).HasColumnName("usuario_id").IsRequired();
        builder.Property(x => x.TokenHashSha256).HasColumnName("token_hash_sha256").HasMaxLength(64).IsRequired();
        builder.Property(x => x.ExpiraEmUtc).HasColumnName("expira_em_utc").HasColumnType("timestamp").IsRequired();
        builder.Property(x => x.UtilizadoEmUtc).HasColumnName("utilizado_em_utc").HasColumnType("timestamp");
        builder.HasIndex(x => x.TokenHashSha256).IsUnique();
        builder.HasIndex(x => new { x.UsuarioId, x.UtilizadoEmUtc, x.ExpiraEmUtc });
        builder.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
    }
}

public class ContratoConfiguration : IEntityTypeConfiguration<Contrato>
{
    public void Configure(EntityTypeBuilder<Contrato> builder)
    {
        builder.ConfigureBase("contratos", "sq_contratos");
        builder.Property(x => x.FornecedorId).HasColumnName("fornecedor_id").IsRequired();
        builder.Property(x => x.NumeroContrato).HasColumnName("numero_contrato").HasMaxLength(60).IsRequired();
        builder.Property(x => x.Descricao).HasColumnName("descricao").HasMaxLength(300).IsRequired();
        builder.Property(x => x.DataInicio).HasColumnName("data_inicio");
        builder.Property(x => x.DataFim).HasColumnName("data_fim");
        builder.Property(x => x.Ativo).HasColumnName("ativo");
        builder.HasIndex(x => new { x.FornecedorId, x.NumeroContrato }).IsUnique();
        builder.HasOne(x => x.Fornecedor).WithMany(x => x.Contratos).HasForeignKey(x => x.FornecedorId);
    }
}

public class DocumentoTipoConfiguration : IEntityTypeConfiguration<DocumentoTipo>
{
    public void Configure(EntityTypeBuilder<DocumentoTipo> builder)
    {
        builder.ConfigureBase("documentos_tipos", "sq_documentos_tipos");
        builder.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(40).IsRequired();
        builder.Property(x => x.NomeDocumento).HasColumnName("nome_documento").HasMaxLength(160).IsRequired();
        builder.Property(x => x.Descricao).HasColumnName("descricao").HasMaxLength(300);
        builder.Property(x => x.Ativo).HasColumnName("ativo").IsRequired();
        builder.HasIndex(x => x.Codigo).IsUnique();
    }
}

public class DocumentoExigidoConfiguration : IEntityTypeConfiguration<DocumentoExigido>
{
    public void Configure(EntityTypeBuilder<DocumentoExigido> builder)
    {
        builder.ConfigureBase("documentos_exigidos", "sq_documentos_exigidos");
        builder.Property(x => x.DocumentoTipoId).HasColumnName("documento_tipo_id").IsRequired();
        builder.Property(x => x.TipoPessoa).HasColumnName("tipo_pessoa").HasConversion<int>().IsRequired();
        builder.Property(x => x.PorteEmpresa).HasColumnName("porte_empresa").HasConversion<int?>();
        builder.Property(x => x.CategoriaId).HasColumnName("categoria_id").IsRequired();
        builder.Property(x => x.Obrigatorio).HasColumnName("obrigatorio").IsRequired();
        builder.Property(x => x.Ativo).HasColumnName("ativo").IsRequired();
        builder.HasIndex(x => new { x.DocumentoTipoId, x.TipoPessoa, x.PorteEmpresa, x.CategoriaId }).IsUnique();
        builder.HasOne(x => x.DocumentoTipo).WithMany(x => x.DocumentosExigidos).HasForeignKey(x => x.DocumentoTipoId);
        builder.HasOne(x => x.Categoria).WithMany(x => x.DocumentosExigidos).HasForeignKey(x => x.CategoriaId);
    }
}

public class DocumentoEnviadoConfiguration : IEntityTypeConfiguration<DocumentoEnviado>
{
    public void Configure(EntityTypeBuilder<DocumentoEnviado> builder)
    {
        builder.ConfigureBase("documentos_enviados", "sq_documentos_enviados");
        builder.Property(x => x.FornecedorId).HasColumnName("fornecedor_id").IsRequired();
        builder.Property(x => x.UsuarioId).HasColumnName("usuario_id").IsRequired();
        builder.Property(x => x.ContratoId).HasColumnName("contrato_id").IsRequired();
        builder.Property(x => x.MesReferencia).HasColumnName("mes_referencia").IsRequired();
        builder.Property(x => x.AnoReferencia).HasColumnName("ano_referencia").IsRequired();
        builder.Property(x => x.DataHoraRegistro).HasColumnName("data_hora_registro").HasColumnType("timestamp").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(x => x.UsuarioRegistro).HasColumnName("usuario_registro").HasMaxLength(60).IsRequired();
        builder.Property(x => x.Observacao).HasColumnName("observacao").HasMaxLength(500);
        builder.Property(x => x.AvaliadoPorUsuarioId).HasColumnName("avaliado_por_usuario_id");
        builder.Property(x => x.DataHoraValidacaoFinal).HasColumnName("data_hora_validacao_final").HasColumnType("timestamp");
        builder.HasIndex(x => new { x.FornecedorId, x.ContratoId, x.MesReferencia, x.AnoReferencia }).IsUnique();
        builder.HasOne(x => x.Fornecedor).WithMany(x => x.DocumentosEnviados).HasForeignKey(x => x.FornecedorId);
        builder.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
        builder.HasOne(x => x.AvaliadoPorUsuario).WithMany().HasForeignKey(x => x.AvaliadoPorUsuarioId);
        builder.HasOne(x => x.Contrato).WithMany(x => x.DocumentosEnviados).HasForeignKey(x => x.ContratoId);
    }
}

public class DocumentoRegistradoConfiguration : IEntityTypeConfiguration<DocumentoRegistrado>
{
    public void Configure(EntityTypeBuilder<DocumentoRegistrado> builder)
    {
        builder.ConfigureBase("documentos_registrados", "sq_documentos_registrados");
        builder.Property(x => x.DocumentoEnviadoId).HasColumnName("documento_enviado_id").IsRequired();
        builder.Property(x => x.DocumentoTipoId).HasColumnName("documento_tipo_id").IsRequired();
        builder.Property(x => x.NomeOriginalArquivo).HasColumnName("nome_original_arquivo").HasMaxLength(255).IsRequired();
        builder.Property(x => x.NomeArquivoFisico).HasColumnName("nome_arquivo_fisico").HasMaxLength(255).IsRequired();
        builder.Property(x => x.CaminhoArquivo).HasColumnName("caminho_arquivo").HasMaxLength(255).IsRequired();
        builder.Property(x => x.Extensao).HasColumnName("extensao").HasMaxLength(10).IsRequired();
        builder.Property(x => x.TamanhoBytes).HasColumnName("tamanho_bytes").IsRequired();
        builder.Property(x => x.DataHoraUpload).HasColumnName("data_hora_upload").HasColumnType("timestamp").IsRequired();
        builder.Property(x => x.UsuarioUpload).HasColumnName("usuario_upload").HasMaxLength(60).IsRequired();
        builder.Property(x => x.StatusValidacaoDocumento).HasColumnName("status_validacao_documento").HasConversion<int>().IsRequired();
        builder.Property(x => x.AvaliadoPorUsuarioId).HasColumnName("avaliado_por_usuario_id");
        builder.Property(x => x.DataHoraAvaliacao).HasColumnName("data_hora_avaliacao").HasColumnType("timestamp");
        builder.Property(x => x.ObservacaoAvaliacao).HasColumnName("observacao_avaliacao").HasMaxLength(500);
        builder.HasIndex(x => new { x.DocumentoEnviadoId, x.DocumentoTipoId }).IsUnique();
        builder.HasOne(x => x.DocumentoEnviado).WithMany(x => x.DocumentosRegistrados).HasForeignKey(x => x.DocumentoEnviadoId);
        builder.HasOne(x => x.DocumentoTipo).WithMany(x => x.DocumentosRegistrados).HasForeignKey(x => x.DocumentoTipoId);
        builder.HasOne(x => x.AvaliadoPorUsuario).WithMany().HasForeignKey(x => x.AvaliadoPorUsuarioId);
    }
}

public class NotificacaoConfiguration : IEntityTypeConfiguration<Notificacao>
{
    public void Configure(EntityTypeBuilder<Notificacao> builder)
    {
        builder.ConfigureBase("notificacoes", "sq_notificacoes");
        builder.Property(x => x.UsuarioId).HasColumnName("usuario_id");
        builder.Property(x => x.RemetenteUsuarioId).HasColumnName("remetente_usuario_id");
        builder.Property(x => x.FornecedorId).HasColumnName("fornecedor_id");
        builder.Property(x => x.TipoNotificacao).HasColumnName("tipo_notificacao").HasConversion<int>().IsRequired();
        builder.Property(x => x.Titulo).HasColumnName("titulo").HasMaxLength(180).IsRequired();
        builder.Property(x => x.Mensagem).HasColumnName("mensagem").HasMaxLength(1000).IsRequired();
        builder.Property(x => x.StatusEnvio).HasColumnName("status_envio").HasConversion<int>().IsRequired();
        builder.Property(x => x.DataHoraEnvio).HasColumnName("data_hora_envio").HasColumnType("timestamp");
        builder.Property(x => x.Tentativas).HasColumnName("tentativas").IsRequired();
        builder.Property(x => x.ReferenciaEntidade).HasColumnName("referencia_entidade").HasMaxLength(60);
        builder.Property(x => x.ReferenciaId).HasColumnName("referencia_id");
        builder.Property(x => x.Destinatario).HasColumnName("destinatario").HasMaxLength(180);
        builder.Property(x => x.Erro).HasColumnName("erro").HasMaxLength(500);
        builder.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
        builder.HasOne(x => x.RemetenteUsuario).WithMany().HasForeignKey(x => x.RemetenteUsuarioId);
        builder.HasOne(x => x.Fornecedor).WithMany(x => x.Notificacoes).HasForeignKey(x => x.FornecedorId);
    }
}

public class FinanceiroLiberacaoConfiguration : IEntityTypeConfiguration<FinanceiroLiberacao>
{
    public void Configure(EntityTypeBuilder<FinanceiroLiberacao> builder)
    {
        builder.ConfigureBase("financeiro_liberacoes", "sq_financeiro_liberacoes");
        builder.Property(x => x.DocumentoEnviadoId).HasColumnName("documento_enviado_id").IsRequired();
        builder.Property(x => x.FornecedorId).HasColumnName("fornecedor_id").IsRequired();
        builder.Property(x => x.ContratoId).HasColumnName("contrato_id");
        builder.Property(x => x.StatusFinanceiro).HasColumnName("status_financeiro").HasConversion<int>().IsRequired();
        builder.Property(x => x.DataHoraGeracao).HasColumnName("data_hora_geracao").HasColumnType("timestamp").IsRequired();
        builder.Property(x => x.GeradoPorUsuarioId).HasColumnName("gerado_por_usuario_id").IsRequired();
        builder.Property(x => x.Observacao).HasColumnName("observacao").HasMaxLength(500);
        builder.Property(x => x.NumeroNotaFiscal).HasColumnName("numero_nota_fiscal").HasMaxLength(60);
        builder.Property(x => x.NomeOriginalNotaFiscal).HasColumnName("nome_original_nota_fiscal").HasMaxLength(255);
        builder.Property(x => x.NomeArquivoFisicoNotaFiscal).HasColumnName("nome_arquivo_fisico_nota_fiscal").HasMaxLength(255);
        builder.Property(x => x.CaminhoArquivoNotaFiscal).HasColumnName("caminho_arquivo_nota_fiscal").HasMaxLength(255);
        builder.Property(x => x.ExtensaoNotaFiscal).HasColumnName("extensao_nota_fiscal").HasMaxLength(10);
        builder.Property(x => x.TamanhoBytesNotaFiscal).HasColumnName("tamanho_bytes_nota_fiscal");
        builder.Property(x => x.DataRecebimentoNotaFiscal).HasColumnName("data_recebimento_nota_fiscal").HasColumnType("timestamp");
        builder.Property(x => x.DataHoraUploadNotaFiscal).HasColumnName("data_hora_upload_nota_fiscal").HasColumnType("timestamp");
        builder.HasIndex(x => x.DocumentoEnviadoId).IsUnique();
        builder.HasOne(x => x.DocumentoEnviado).WithMany(x => x.LiberacoesFinanceiras).HasForeignKey(x => x.DocumentoEnviadoId);
        builder.HasOne(x => x.Fornecedor).WithMany(x => x.LiberacoesFinanceiras).HasForeignKey(x => x.FornecedorId);
        builder.HasOne(x => x.Contrato).WithMany(x => x.LiberacoesFinanceiras).HasForeignKey(x => x.ContratoId);
        builder.HasOne(x => x.GeradoPorUsuario).WithMany().HasForeignKey(x => x.GeradoPorUsuarioId);
    }
}

public class LogAuditoriaConfiguration : IEntityTypeConfiguration<LogAuditoria>
{
    public void Configure(EntityTypeBuilder<LogAuditoria> builder)
    {
        builder.ConfigureBase("logs_auditoria", "sq_logs_auditoria");
        builder.Property(x => x.UsuarioId).HasColumnName("usuario_id");
        builder.Property(x => x.Entidade).HasColumnName("entidade").HasMaxLength(60).IsRequired();
        builder.Property(x => x.EntidadeId).HasColumnName("entidade_id");
        builder.Property(x => x.Acao).HasColumnName("acao").HasConversion<int>().IsRequired();
        builder.Property(x => x.DadosResumidos).HasColumnName("dados_resumidos").HasMaxLength(1000).IsRequired();
        builder.Property(x => x.IpOrigem).HasColumnName("ip_origem").HasMaxLength(50);
        builder.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
    }
}

public class ParametroSistemaConfiguration : IEntityTypeConfiguration<ParametroSistema>
{
    public void Configure(EntityTypeBuilder<ParametroSistema> builder)
    {
        builder.ConfigureBase("parametros_sistema", "sq_parametros_sistema");
        builder.Property(x => x.Chave).HasColumnName("chave").HasMaxLength(80).IsRequired();
        builder.Property(x => x.Valor).HasColumnName("valor").HasMaxLength(500).IsRequired();
        builder.Property(x => x.Descricao).HasColumnName("descricao").HasMaxLength(250);
        builder.Property(x => x.Ativo).HasColumnName("ativo").IsRequired();
        builder.HasIndex(x => x.Chave).IsUnique();
    }
}
