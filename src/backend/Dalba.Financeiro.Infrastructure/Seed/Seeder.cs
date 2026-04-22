using Dalba.Financeiro.Application.Common;
using Dalba.Financeiro.Domain.Entities;
using Dalba.Financeiro.Domain.Enums;
using Dalba.Financeiro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dalba.Financeiro.Infrastructure.Seed;

public static class Seeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.EnsureCreatedAsync();
        await context.Database.ExecuteSqlRawAsync("ALTER TABLE IF EXISTS documentos_registrados DROP CONSTRAINT IF EXISTS uq_documentos_registrados_item;");
        await context.Database.ExecuteSqlRawAsync("DROP INDEX IF EXISTS \"IX_documentos_registrados_DocumentoEnviadoId_DocumentoTipoId\";");
        await context.Database.ExecuteSqlRawAsync("DROP INDEX IF EXISTS ix_documentos_registrados_envio_tipo;");
        await context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS ix_documentos_registrados_envio_tipo ON documentos_registrados (documento_enviado_id, documento_tipo_id);");
        await context.Database.ExecuteSqlRawAsync("ALTER TABLE financeiro_liberacoes ADD COLUMN IF NOT EXISTS nome_original_nota_fiscal VARCHAR(255) NULL;");
        await context.Database.ExecuteSqlRawAsync("ALTER TABLE financeiro_liberacoes ADD COLUMN IF NOT EXISTS nome_arquivo_fisico_nota_fiscal VARCHAR(255) NULL;");
        await context.Database.ExecuteSqlRawAsync("ALTER TABLE financeiro_liberacoes ADD COLUMN IF NOT EXISTS caminho_arquivo_nota_fiscal VARCHAR(255) NULL;");
        await context.Database.ExecuteSqlRawAsync("ALTER TABLE financeiro_liberacoes ADD COLUMN IF NOT EXISTS extensao_nota_fiscal VARCHAR(10) NULL;");
        await context.Database.ExecuteSqlRawAsync("ALTER TABLE financeiro_liberacoes ADD COLUMN IF NOT EXISTS tamanho_bytes_nota_fiscal BIGINT NULL;");
        await context.Database.ExecuteSqlRawAsync("ALTER TABLE financeiro_liberacoes ADD COLUMN IF NOT EXISTS data_hora_upload_nota_fiscal TIMESTAMP NULL;");

        if (!context.Categorias.Any())
        {
            context.Categorias.AddRange(
                new Categoria { Codigo = "SERV", Descricao = "Serviços contínuos", Ativo = true },
                new Categoria { Codigo = "OBRA", Descricao = "Obras e manutenção", Ativo = true },
                new Categoria { Codigo = "CONS", Descricao = "Consultoria", Ativo = true });
            await context.SaveChangesAsync();
        }

        if (!context.DocumentosTipos.Any())
        {
            context.DocumentosTipos.AddRange(
                new DocumentoTipo { Codigo = "DOC_FISCAL", NomeDocumento = "Certidão Fiscal", Ativo = true },
                new DocumentoTipo { Codigo = "DOC_TRAB", NomeDocumento = "Comprovante Trabalhista", Ativo = true },
                new DocumentoTipo { Codigo = "DOC_CONTRATO", NomeDocumento = "Contrato Assinado", Ativo = true },
                new DocumentoTipo { Codigo = "DOC_BANCARIO", NomeDocumento = "Comprovante Bancário", Ativo = true });
            await context.SaveChangesAsync();
        }

        if (!context.Fornecedores.Any())
        {
            var categoria = context.Categorias.First();
            context.Fornecedores.Add(new Fornecedor
            {
                CodigoFornecedor = "000123",
                TipoPessoa = TipoPessoa.Juridica,
                PorteEmpresa = PorteEmpresa.Microempresa,
                CategoriaId = categoria.Id,
                NomeOuRazaoSocial = "Fornecedor Exemplo LTDA",
                NomeFantasia = "Fornecedor Exemplo",
                CpfOuCnpj = "12345678000195",
                DdiTelefone = "+55",
                DddTelefone = "11",
                NumeroTelefone = "999999999",
                Email = "fornecedor@dalba.local",
                Cep = "01001000",
                Logradouro = "Rua Exemplo",
                Numero = "100",
                Bairro = "Centro",
                Cidade = "Sao Paulo",
                Estado = "SP",
                Pais = "Brasil",
                Ativo = true
            });
            await context.SaveChangesAsync();
        }

        if (!context.Usuarios.Any())
        {
            var fornecedor = context.Fornecedores.First();
            context.Usuarios.AddRange(
                new Usuario
                {
                    Nome = "Administrador Dalba",
                    Email = "admin@dalba.local",
                    Login = "admin",
                    SenhaHashSha256 = SecurityHelper.ComputeSha256("Admin@123"),
                    Perfil = PerfilAcesso.Admin,
                    Ativo = true
                },
                new Usuario
                {
                    Nome = "Financeiro Dalba",
                    Email = "financeiro@dalba.local",
                    Login = "financeiro",
                    SenhaHashSha256 = SecurityHelper.ComputeSha256("Financeiro@123"),
                    Perfil = PerfilAcesso.Financeiro,
                    Ativo = true
                },
                new Usuario
                {
                    Nome = "Fornecedor Exemplo",
                    Email = "fornecedor@dalba.local",
                    Login = "fornecedor",
                    SenhaHashSha256 = SecurityHelper.ComputeSha256("Fornecedor@123"),
                    Perfil = PerfilAcesso.Fornecedor,
                    FornecedorId = fornecedor.Id,
                    Ativo = true
                });
            await context.SaveChangesAsync();
        }

        if (!context.DocumentosExigidos.Any())
        {
            var categoria = context.Categorias.First();
            var docs = context.DocumentosTipos.ToList();
            foreach (var doc in docs)
            {
                context.DocumentosExigidos.Add(new DocumentoExigido
                {
                    DocumentoTipoId = doc.Id,
                    TipoPessoa = TipoPessoa.Juridica,
                    PorteEmpresa = PorteEmpresa.Microempresa,
                    CategoriaId = categoria.Id,
                    Obrigatorio = true,
                    Ativo = true
                });
            }

            context.DocumentosExigidos.Add(new DocumentoExigido
            {
                DocumentoTipoId = docs.First().Id,
                TipoPessoa = TipoPessoa.Fisica,
                CategoriaId = categoria.Id,
                Obrigatorio = true,
                Ativo = true
            });

            await context.SaveChangesAsync();
        }
    }
}
