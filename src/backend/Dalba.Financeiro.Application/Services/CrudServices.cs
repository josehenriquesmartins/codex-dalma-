using Dalba.Financeiro.Application.Abstractions.Audit;
using Dalba.Financeiro.Application.Abstractions.Persistence;
using Dalba.Financeiro.Application.Abstractions.Security;
using Dalba.Financeiro.Application.Common;
using Dalba.Financeiro.Application.DTOs.Categorias;
using Dalba.Financeiro.Application.DTOs.Contratos;
using Dalba.Financeiro.Application.DTOs.Documentos;
using Dalba.Financeiro.Application.DTOs.Fornecedores;
using Dalba.Financeiro.Application.DTOs.Usuarios;
using Dalba.Financeiro.Domain.Entities;
using Dalba.Financeiro.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Dalba.Financeiro.Application.Services;

public class UsuarioService
{
    private readonly IAppDbContext _context;
    private readonly IAuditService _auditService;

    public UsuarioService(IAppDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<IReadOnlyCollection<UsuarioResponse>> ListAsync(CancellationToken ct) =>
        await _context.Usuarios.AsNoTracking()
            .OrderBy(x => x.Nome)
            .Select(x => new UsuarioResponse(x.Id, x.Nome, x.Email, x.Login, x.Perfil, x.FornecedorId, x.Ativo, x.DataHoraCriacao))
            .ToListAsync(ct);

    public async Task<long> CreateAsync(UsuarioRequest request, CancellationToken ct)
    {
        await ValidateAsync(request, null, ct);

        var entity = new Usuario
        {
            Nome = request.Nome,
            Email = request.Email,
            Login = request.Login,
            SenhaHashSha256 = SecurityHelper.ComputeSha256(request.Senha),
            Perfil = request.Perfil,
            FornecedorId = request.FornecedorId,
            Ativo = request.Ativo
        };

        _context.Usuarios.Add(entity);
        await _context.SaveChangesAsync(ct);
        await _auditService.RegistrarAsync("usuarios", entity.Id, AcaoAuditoria.Criacao, $"Usuário {entity.Login} criado.", ct);
        return entity.Id;
    }

    public async Task UpdateAsync(long id, UsuarioRequest request, CancellationToken ct)
    {
        var entity = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new AppException("Usuário não encontrado.", 404);
        await ValidateAsync(request, id, ct);

        entity.Nome = request.Nome;
        entity.Email = request.Email;
        entity.Login = request.Login;
        if (!string.IsNullOrWhiteSpace(request.Senha)) entity.SenhaHashSha256 = SecurityHelper.ComputeSha256(request.Senha);
        entity.Perfil = request.Perfil;
        entity.FornecedorId = request.Perfil == PerfilAcesso.Fornecedor ? request.FornecedorId : null;
        entity.Ativo = request.Ativo;
        entity.DataHoraAtualizacao = DbClock.Now;

        await _context.SaveChangesAsync(ct);
        await _auditService.RegistrarAsync("usuarios", entity.Id, AcaoAuditoria.Edicao, $"Usuário {entity.Login} alterado.", ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct)
    {
        var entity = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new AppException("Usuário não encontrado.", 404);
        var hasDependency =
            await _context.DocumentosEnviados.AnyAsync(x => x.UsuarioId == id || x.AvaliadoPorUsuarioId == id, ct) ||
            await _context.DocumentosRegistrados.AnyAsync(x => x.AvaliadoPorUsuarioId == id, ct) ||
            await _context.FinanceiroLiberacoes.AnyAsync(x => x.GeradoPorUsuarioId == id, ct) ||
            await _context.LogsAuditoria.AnyAsync(x => x.UsuarioId == id, ct);

        if (hasDependency) throw new AppException("Usuário possui vínculo operacional e não pode ser excluído. Desative o cadastro.", 409);

        _context.Usuarios.Remove(entity);
        await _context.SaveChangesAsync(ct);
        await _auditService.RegistrarAsync("usuarios", id, AcaoAuditoria.Exclusao, $"Usuário {entity.Login} excluído.", ct);
    }

    private async Task ValidateAsync(UsuarioRequest request, long? id, CancellationToken ct)
    {
        if (!ValidationHelper.IsValidEmail(request.Email)) throw new AppException("E-mail inválido.");
        if (request.Perfil == PerfilAcesso.Fornecedor && !request.FornecedorId.HasValue) throw new AppException("Usuário fornecedor deve estar vinculado a um fornecedor.");
        if (await _context.Usuarios.AnyAsync(x => x.Login == request.Login && x.Id != id, ct)) throw new AppException("Login já cadastrado.");
        if (await _context.Usuarios.AnyAsync(x => x.Email == request.Email && x.Id != id, ct)) throw new AppException("E-mail já cadastrado.");
    }
}

public class CategoriaService
{
    private readonly IAppDbContext _context;
    private readonly IAuditService _auditService;

    public CategoriaService(IAppDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<IReadOnlyCollection<CategoriaResponse>> ListAsync(CancellationToken ct) =>
        await _context.Categorias.AsNoTracking().OrderBy(x => x.Descricao)
            .Select(x => new CategoriaResponse(x.Id, x.Codigo, x.Descricao, x.Ativo)).ToListAsync(ct);

    public async Task<long> CreateAsync(CategoriaRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Descricao)) throw new AppException("Descrição é obrigatória.");
        if (await _context.Categorias.AnyAsync(x => x.Codigo == request.Codigo, ct)) throw new AppException("Código da categoria já cadastrado.");

        var entity = new Categoria { Codigo = request.Codigo, Descricao = request.Descricao, Ativo = request.Ativo };
        _context.Categorias.Add(entity);
        await _context.SaveChangesAsync(ct);
        await _auditService.RegistrarAsync("categorias", entity.Id, AcaoAuditoria.Criacao, $"Categoria {entity.Codigo} criada.", ct);
        return entity.Id;
    }

    public async Task UpdateAsync(long id, CategoriaRequest request, CancellationToken ct)
    {
        var entity = await _context.Categorias.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new AppException("Categoria não encontrada.", 404);
        if (string.IsNullOrWhiteSpace(request.Descricao)) throw new AppException("Descrição é obrigatória.");
        if (await _context.Categorias.AnyAsync(x => x.Codigo == request.Codigo && x.Id != id, ct)) throw new AppException("Código da categoria já cadastrado.");

        entity.Codigo = request.Codigo;
        entity.Descricao = request.Descricao;
        entity.Ativo = request.Ativo;
        entity.DataHoraAtualizacao = DbClock.Now;

        await _context.SaveChangesAsync(ct);
        await _auditService.RegistrarAsync("categorias", entity.Id, AcaoAuditoria.Edicao, $"Categoria {entity.Codigo} alterada.", ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct)
    {
        var entity = await _context.Categorias.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new AppException("Categoria não encontrada.", 404);
        var hasDependency =
            await _context.Fornecedores.AnyAsync(x => x.CategoriaId == id, ct) ||
            await _context.DocumentosExigidos.AnyAsync(x => x.CategoriaId == id, ct);

        if (hasDependency) throw new AppException("Categoria possui vínculos e não pode ser excluída.", 409);

        _context.Categorias.Remove(entity);
        await _context.SaveChangesAsync(ct);
        await _auditService.RegistrarAsync("categorias", id, AcaoAuditoria.Exclusao, $"Categoria {entity.Codigo} excluída.", ct);
    }
}

public class FornecedorService
{
    private static readonly string[] ImportExtensions = [".xlsx", ".csv"];
    private readonly IAppDbContext _context;
    private readonly IAuditService _auditService;

    public FornecedorService(IAppDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<IReadOnlyCollection<FornecedorResponse>> ListAsync(CancellationToken ct) =>
        await _context.Fornecedores.AsNoTracking().Include(x => x.Categoria).OrderBy(x => x.NomeOuRazaoSocial)
            .Select(x => new FornecedorResponse(
                x.Id, x.CodigoFornecedor, x.TipoPessoa, x.PorteEmpresa, x.CategoriaId, x.Categoria!.Descricao,
                x.NomeOuRazaoSocial, x.NomeFantasia, x.CpfOuCnpj, x.DdiTelefone, x.DddTelefone, x.NumeroTelefone,
                x.Email, x.Cep, x.Logradouro, x.Numero, x.Complemento, x.Bairro, x.Cidade, x.Estado, x.Pais, x.Ativo)).ToListAsync(ct);

    public async Task<long> CreateAsync(FornecedorRequest request, CancellationToken ct)
    {
        await ValidateFornecedorAsync(request, null, ct);
        var documentoNormalizado = ValidationHelper.NormalizarDocumento(request.CpfOuCnpj);

        var entity = new Fornecedor
        {
            CodigoFornecedor = request.CodigoFornecedor,
            TipoPessoa = request.TipoPessoa,
            PorteEmpresa = request.PorteEmpresa,
            CategoriaId = request.CategoriaId,
            NomeOuRazaoSocial = request.NomeOuRazaoSocial,
            NomeFantasia = request.NomeFantasia,
            CpfOuCnpj = documentoNormalizado,
            DdiTelefone = request.DdiTelefone,
            DddTelefone = request.DddTelefone,
            NumeroTelefone = request.NumeroTelefone,
            Email = request.Email,
            Cep = request.Cep,
            Logradouro = request.Logradouro,
            Numero = request.Numero,
            Complemento = request.Complemento,
            Bairro = request.Bairro,
            Cidade = request.Cidade,
            Estado = request.Estado,
            Pais = request.Pais,
            Ativo = request.Ativo
        };

        _context.Fornecedores.Add(entity);
        await _context.SaveChangesAsync(ct);
        await EnsureUsuarioFornecedorAsync(entity, documentoNormalizado, isNewSupplier: true, ct);
        await _context.SaveChangesAsync(ct);
        await _auditService.RegistrarAsync("usuarios", await _context.Usuarios.Where(x => x.FornecedorId == entity.Id).Select(x => (long?)x.Id).FirstOrDefaultAsync(ct), AcaoAuditoria.Criacao, $"Usuário automático criado para o fornecedor {entity.CodigoFornecedor}.", ct);
        await _auditService.RegistrarAsync("fornecedores", entity.Id, AcaoAuditoria.Criacao, $"Fornecedor {entity.CodigoFornecedor} criado.", ct);
        return entity.Id;
    }

    public async Task UpdateAsync(long id, FornecedorRequest request, CancellationToken ct)
    {
        var entity = await _context.Fornecedores.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new AppException("Fornecedor não encontrado.", 404);
        await ValidateFornecedorAsync(request, id, ct);
        var documentoNormalizado = ValidationHelper.NormalizarDocumento(request.CpfOuCnpj);

        entity.CodigoFornecedor = request.CodigoFornecedor;
        entity.TipoPessoa = request.TipoPessoa;
        entity.PorteEmpresa = request.PorteEmpresa;
        entity.CategoriaId = request.CategoriaId;
        entity.NomeOuRazaoSocial = request.NomeOuRazaoSocial;
        entity.NomeFantasia = request.NomeFantasia;
        entity.CpfOuCnpj = documentoNormalizado;
        entity.DdiTelefone = request.DdiTelefone;
        entity.DddTelefone = request.DddTelefone;
        entity.NumeroTelefone = request.NumeroTelefone;
        entity.Email = request.Email;
        entity.Cep = request.Cep;
        entity.Logradouro = request.Logradouro;
        entity.Numero = request.Numero;
        entity.Complemento = request.Complemento;
        entity.Bairro = request.Bairro;
        entity.Cidade = request.Cidade;
        entity.Estado = request.Estado;
        entity.Pais = request.Pais;
        entity.Ativo = request.Ativo;
        entity.DataHoraAtualizacao = DbClock.Now;

        await EnsureUsuarioFornecedorAsync(entity, documentoNormalizado, isNewSupplier: false, ct);
        await _context.SaveChangesAsync(ct);
        await _auditService.RegistrarAsync("fornecedores", entity.Id, AcaoAuditoria.Edicao, $"Fornecedor {entity.CodigoFornecedor} alterado.", ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct)
    {
        var entity = await _context.Fornecedores.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new AppException("Fornecedor não encontrado.", 404);
        var hasDependency =
            await _context.Usuarios.AnyAsync(x => x.FornecedorId == id, ct) ||
            await _context.Contratos.AnyAsync(x => x.FornecedorId == id, ct) ||
            await _context.DocumentosEnviados.AnyAsync(x => x.FornecedorId == id, ct) ||
            await _context.Notificacoes.AnyAsync(x => x.FornecedorId == id, ct) ||
            await _context.FinanceiroLiberacoes.AnyAsync(x => x.FornecedorId == id, ct);

        if (hasDependency) throw new AppException("Fornecedor possui vínculos e não pode ser excluído.", 409);

        _context.Fornecedores.Remove(entity);
        await _context.SaveChangesAsync(ct);
        await _auditService.RegistrarAsync("fornecedores", id, AcaoAuditoria.Exclusao, $"Fornecedor {entity.CodigoFornecedor} excluído.", ct);
    }

    public async Task<IReadOnlyCollection<FornecedorImportacaoResultado>> ImportarAsync(IFormFile arquivo, CancellationToken ct)
    {
        if (arquivo.Length == 0) throw new AppException("Arquivo de importação vazio.");
        var extension = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
        if (!ImportExtensions.Contains(extension)) throw new AppException("Envie uma planilha .xlsx ou .csv exportada pelo Excel.");

        var rows = extension == ".xlsx"
            ? await ReadXlsxAsync(arquivo, ct)
            : await ReadCsvAsync(arquivo, ct);

        var resultados = new List<FornecedorImportacaoResultado>();
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var linha = i + 2;
            var codigo = Get(row, "codigo", "codigofornecedor", "codfornecedor");
            var nome = Get(row, "nomeourazaosocial", "nome", "razaosocial", "prestador", "fornecedor");

            try
            {
                var request = await BuildImportRequestAsync(row, ct);
                var existente = await _context.Fornecedores.FirstOrDefaultAsync(x => x.CodigoFornecedor == request.CodigoFornecedor, ct);
                if (existente is null)
                {
                    await CreateAsync(request, ct);
                    resultados.Add(new FornecedorImportacaoResultado(linha, request.CodigoFornecedor, request.NomeOuRazaoSocial, true, "Incluído."));
                }
                else
                {
                    await UpdateAsync(existente.Id, request, ct);
                    resultados.Add(new FornecedorImportacaoResultado(linha, request.CodigoFornecedor, request.NomeOuRazaoSocial, true, "Atualizado."));
                }
            }
            catch (Exception ex)
            {
                resultados.Add(new FornecedorImportacaoResultado(linha, codigo, nome, false, ex.Message));
            }
        }

        await _auditService.RegistrarAsync("fornecedores", null, AcaoAuditoria.Edicao, $"Importação de fornecedores: {resultados.Count(x => x.Importado)} importados de {resultados.Count} linha(s).", ct);
        return resultados;
    }

    private async Task ValidateFornecedorAsync(FornecedorRequest request, long? id, CancellationToken ct)
    {
        if (!await _context.Categorias.AnyAsync(x => x.Id == request.CategoriaId && x.Ativo, ct)) throw new AppException("Categoria obrigatória e inválida.");
        if (!string.IsNullOrWhiteSpace(request.Email) && !ValidationHelper.IsValidEmail(request.Email)) throw new AppException("E-mail inválido.");
        var document = ValidationHelper.NormalizarDocumento(request.CpfOuCnpj);
        if (request.TipoPessoa == TipoPessoa.Fisica && !ValidationHelper.IsValidCpf(document)) throw new AppException("CPF inválido.");
        if (request.TipoPessoa == TipoPessoa.Juridica && !ValidationHelper.IsValidCnpj(document)) throw new AppException("CNPJ inválido.");
        if (request.TipoPessoa == TipoPessoa.Juridica && !request.PorteEmpresa.HasValue) throw new AppException("Porte da empresa é obrigatório para pessoa jurídica.");
        if (request.TipoPessoa == TipoPessoa.Fisica && request.PorteEmpresa.HasValue) throw new AppException("Pessoa física não pode possuir porte.");
        if (await _context.Fornecedores.AnyAsync(x => x.CpfOuCnpj == document && x.Id != id, ct)) throw new AppException("CPF/CNPJ já cadastrado.");
        if (await _context.Fornecedores.AnyAsync(x => x.CodigoFornecedor == request.CodigoFornecedor && x.Id != id, ct)) throw new AppException("Código do fornecedor já cadastrado.");
        var usuarioFornecedorId = id.HasValue
            ? await _context.Usuarios.Where(x => x.FornecedorId == id.Value).Select(x => (long?)x.Id).FirstOrDefaultAsync(ct)
            : null;
        if (!string.IsNullOrWhiteSpace(request.Email) && await _context.Usuarios.AnyAsync(x => x.Email == request.Email && x.Id != usuarioFornecedorId, ct)) throw new AppException("E-mail já cadastrado para outro usuário.");
        if (await _context.Usuarios.AnyAsync(x => x.Login == document && x.Id != usuarioFornecedorId, ct)) throw new AppException("Já existe um usuário com este CPF/CNPJ.");
    }

    private async Task EnsureUsuarioFornecedorAsync(Fornecedor fornecedor, string documentoNormalizado, bool isNewSupplier, CancellationToken ct)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(x => x.FornecedorId == fornecedor.Id, ct);

        var emailUsuario = string.IsNullOrWhiteSpace(fornecedor.Email) ? $"{documentoNormalizado}@sememail.local" : fornecedor.Email;

        if (usuario is null)
        {
            usuario = new Usuario
            {
                Nome = fornecedor.NomeOuRazaoSocial,
                Email = emailUsuario,
                Login = documentoNormalizado,
                SenhaHashSha256 = SecurityHelper.ComputeSha256(documentoNormalizado),
                Perfil = PerfilAcesso.Fornecedor,
                FornecedorId = fornecedor.Id,
                Ativo = fornecedor.Ativo
            };

            _context.Usuarios.Add(usuario);
            return;
        }

        usuario.Nome = fornecedor.NomeOuRazaoSocial;
        usuario.Email = emailUsuario;
        usuario.Login = documentoNormalizado;
        usuario.Perfil = PerfilAcesso.Fornecedor;
        usuario.Ativo = fornecedor.Ativo;
        usuario.DataHoraAtualizacao = DbClock.Now;
    }

    private async Task<FornecedorRequest> BuildImportRequestAsync(Dictionary<string, string> row, CancellationToken ct)
    {
        var tipoPessoa = ParseTipoPessoa(GetRequired(row, "tipopessoa", "tipo"));
        var documento = ValidationHelper.NormalizarDocumento(GetRequired(row, "cpfoucnpj", "cpfcnpj", "documento", tipoPessoa == TipoPessoa.Fisica ? "cpf" : "cnpj"));
        var categoriaId = await ResolveCategoriaIdAsync(GetRequired(row, "categoria", "categoriaid", "categoriacodigo"), ct);
        var telefone = ValidationHelper.SomenteDigitos(Get(row, "telefone", "numerotelefone", "celular"));
        var ddd = ValidationHelper.SomenteDigitos(Get(row, "ddd", "dddtelefone"));
        if (string.IsNullOrWhiteSpace(ddd) && telefone.Length > 9)
        {
            ddd = telefone[..2];
            telefone = telefone[2..];
        }

        return new FornecedorRequest(
            GetRequired(row, "codigo", "codigofornecedor", "codfornecedor"),
            tipoPessoa,
            tipoPessoa == TipoPessoa.Fisica ? null : ParsePorte(Get(row, "porte", "porteempresa")),
            categoriaId,
            GetRequired(row, "nomeourazaosocial", "nome", "razaosocial", "prestador", "fornecedor"),
            Get(row, "nomefantasia", "fantasia"),
            documento,
            Get(row, "ddi", "dditelefone") is { Length: > 0 } ddi ? ddi : "+55",
            string.IsNullOrWhiteSpace(ddd) ? "00" : ddd,
            string.IsNullOrWhiteSpace(telefone) ? "000000000" : telefone,
            GetRequired(row, "email", "e-mail"),
            Get(row, "cep") is { Length: > 0 } cep ? cep : "00000000",
            Get(row, "logradouro", "endereco") is { Length: > 0 } logradouro ? logradouro : "Não informado",
            Get(row, "numero", "numeroendereco") is { Length: > 0 } numero ? numero : "S/N",
            Get(row, "complemento"),
            Get(row, "bairro") is { Length: > 0 } bairro ? bairro : "Não informado",
            Get(row, "cidade", "municipio") is { Length: > 0 } cidade ? cidade : "Não informado",
            Get(row, "estado", "uf") is { Length: > 0 } estado ? estado : "PR",
            Get(row, "pais", "país") is { Length: > 0 } pais ? pais : "Brasil",
            ParseBool(Get(row, "ativo")));
    }

    private async Task<long> ResolveCategoriaIdAsync(string value, CancellationToken ct)
    {
        if (long.TryParse(value, out var id) && await _context.Categorias.AnyAsync(x => x.Id == id, ct)) return id;
        var normalized = NormalizeHeader(value);
        var categoria = await _context.Categorias.FirstOrDefaultAsync(x =>
            x.Codigo.ToLower() == value.ToLower() ||
            x.Descricao.ToLower() == value.ToLower(), ct);
        if (categoria is not null) return categoria.Id;

        var categorias = await _context.Categorias.AsNoTracking().ToListAsync(ct);
        categoria = categorias.FirstOrDefault(x =>
            NormalizeHeader(x.Codigo) == normalized ||
            NormalizeHeader(x.Descricao) == normalized);

        return categoria?.Id ?? throw new AppException($"Categoria '{value}' não encontrada.");
    }

    private static TipoPessoa ParseTipoPessoa(string value)
    {
        var normalized = NormalizeHeader(value);
        if (normalized is "pf" or "fisica" or "pessoafisica") return TipoPessoa.Fisica;
        if (normalized is "pj" or "juridica" or "pessoajuridica") return TipoPessoa.Juridica;
        throw new AppException($"Tipo de pessoa inválido: {value}.");
    }

    private static PorteEmpresa? ParsePorte(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = NormalizeHeader(value);
        return normalized switch
        {
            "mei" => PorteEmpresa.Mei,
            "microempresa" or "micro" => PorteEmpresa.Microempresa,
            "pequenaempresa" or "pequena" => PorteEmpresa.PequenaEmpresa,
            "mediaempresa" or "media" => PorteEmpresa.MediaEmpresa,
            "grandeempresa" or "grande" => PorteEmpresa.GrandeEmpresa,
            _ => throw new AppException($"Porte inválido: {value}.")
        };
    }

    private static bool ParseBool(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return true;
        var normalized = NormalizeHeader(value);
        return normalized is "sim" or "s" or "true" or "1" or "ativo";
    }

    private static string GetRequired(Dictionary<string, string> row, params string[] keys)
    {
        var value = Get(row, keys);
        if (string.IsNullOrWhiteSpace(value)) throw new AppException($"Campo obrigatório ausente: {keys[0]}.");
        return value.Trim();
    }

    private static string Get(Dictionary<string, string> row, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (row.TryGetValue(NormalizeHeader(key), out var value)) return value.Trim();
        }

        return string.Empty;
    }

    private static async Task<List<Dictionary<string, string>>> ReadCsvAsync(IFormFile arquivo, CancellationToken ct)
    {
        await using var stream = arquivo.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var content = await reader.ReadToEndAsync(ct);
        var lines = content.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return [];
        var separator = lines[0].Contains(';') ? ';' : ',';
        var headers = SplitCsvLine(lines[0], separator).Select(NormalizeHeader).ToList();
        return lines.Skip(1).Select(line => ToDictionary(headers, SplitCsvLine(line, separator))).ToList();
    }

    private static async Task<List<Dictionary<string, string>>> ReadXlsxAsync(IFormFile arquivo, CancellationToken ct)
    {
        await using var memory = new MemoryStream();
        await arquivo.CopyToAsync(memory, ct);
        memory.Position = 0;

        using var archive = new ZipArchive(memory, ZipArchiveMode.Read);
        var sharedStrings = ReadSharedStrings(archive);
        var sheetEntry = archive.GetEntry("xl/worksheets/sheet1.xml") ?? throw new AppException("A planilha precisa ter a primeira aba preenchida.");
        await using var sheetStream = sheetEntry.Open();
        var sheet = await XDocument.LoadAsync(sheetStream, LoadOptions.None, ct);
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var rows = sheet.Descendants(ns + "row").ToList();
        if (rows.Count < 2) return [];

        var headers = rows[0].Elements(ns + "c")
            .Select(c => NormalizeHeader(ReadCell(c, sharedStrings, ns)))
            .ToList();

        return rows.Skip(1)
            .Select(row => ToDictionary(headers, row.Elements(ns + "c").Select(c => ReadCell(c, sharedStrings, ns)).ToList(), row.Elements(ns + "c").Select(c => ColumnIndex(c.Attribute("r")?.Value)).ToList()))
            .Where(row => row.Values.Any(v => !string.IsNullOrWhiteSpace(v)))
            .ToList();
    }

    private static List<string> ReadSharedStrings(ZipArchive archive)
    {
        var entry = archive.GetEntry("xl/sharedStrings.xml");
        if (entry is null) return [];
        using var stream = entry.Open();
        var doc = XDocument.Load(stream);
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        return doc.Descendants(ns + "si").Select(si => string.Concat(si.Descendants(ns + "t").Select(t => t.Value))).ToList();
    }

    private static string ReadCell(XElement cell, List<string> sharedStrings, XNamespace ns)
    {
        var value = cell.Element(ns + "v")?.Value ?? string.Empty;
        if (cell.Attribute("t")?.Value == "s" && int.TryParse(value, out var index) && index >= 0 && index < sharedStrings.Count)
        {
            return sharedStrings[index];
        }

        return value;
    }

    private static Dictionary<string, string> ToDictionary(List<string> headers, List<string> values, List<int>? indexes = null)
    {
        var row = new Dictionary<string, string>();
        for (var i = 0; i < values.Count; i++)
        {
            var headerIndex = indexes is null ? i : indexes[i] - 1;
            if (headerIndex < 0 || headerIndex >= headers.Count) continue;
            if (!string.IsNullOrWhiteSpace(headers[headerIndex])) row[headers[headerIndex]] = values[i];
        }

        return row;
    }

    private static int ColumnIndex(string? reference)
    {
        var letters = new string((reference ?? string.Empty).TakeWhile(char.IsLetter).ToArray());
        var index = 0;
        foreach (var letter in letters.ToUpperInvariant())
        {
            index = (index * 26) + letter - 'A' + 1;
        }

        return index;
    }

    private static List<string> SplitCsvLine(string line, char separator)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;
        foreach (var ch in line)
        {
            if (ch == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (ch == separator && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(ch);
        }

        result.Add(current.ToString());
        return result;
    }

    private static string NormalizeHeader(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(ch))
            {
                builder.Append(char.ToLowerInvariant(ch));
            }
        }

        return Regex.Replace(builder.ToString(), "[^a-z0-9]", string.Empty);
    }
}

public class ContratoService
{
    private readonly IAppDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public ContratoService(IAppDbContext context, IAuditService auditService, ICurrentUserService currentUser)
    {
        _context = context;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<ContratoResponse>> ListAsync(CancellationToken ct)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var query = _context.Contratos.AsNoTracking().Include(x => x.Fornecedor).AsQueryable();

        if (_currentUser.Perfil == PerfilAcesso.Fornecedor && _currentUser.FornecedorId.HasValue)
        {
            query = query.Where(x => x.FornecedorId == _currentUser.FornecedorId.Value);
        }

        return await query.OrderByDescending(x => x.DataInicio)
            .Select(x => new ContratoResponse(x.Id, x.FornecedorId, x.Fornecedor!.NomeOuRazaoSocial, x.NumeroContrato, x.Descricao, x.DataInicio, x.DataFim, x.Ativo, !x.DataFim.HasValue || x.DataFim >= hoje))
            .ToListAsync(ct);
    }

    public async Task<long> CreateAsync(ContratoRequest request, CancellationToken ct)
    {
        if (!await _context.Fornecedores.AnyAsync(x => x.Id == request.FornecedorId, ct)) throw new AppException("Fornecedor do contrato é obrigatório.");
        if (request.DataFim.HasValue && request.DataFim.Value < request.DataInicio) throw new AppException("Data fim não pode ser menor que data início.");

        var entity = new Contrato
        {
            FornecedorId = request.FornecedorId,
            NumeroContrato = request.NumeroContrato,
            Descricao = request.Descricao,
            DataInicio = request.DataInicio,
            DataFim = request.DataFim,
            Ativo = request.Ativo
        };

        _context.Contratos.Add(entity);
        await _context.SaveChangesAsync(ct);
        await _auditService.RegistrarAsync("contratos", entity.Id, AcaoAuditoria.Criacao, $"Contrato {entity.NumeroContrato} criado.", ct);
        return entity.Id;
    }

    public async Task UpdateAsync(long id, ContratoRequest request, CancellationToken ct)
    {
        var entity = await _context.Contratos.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new AppException("Contrato não encontrado.", 404);
        if (!await _context.Fornecedores.AnyAsync(x => x.Id == request.FornecedorId, ct)) throw new AppException("Fornecedor do contrato é obrigatório.");
        if (request.DataFim.HasValue && request.DataFim.Value < request.DataInicio) throw new AppException("Data fim não pode ser menor que data início.");
        if (await _context.Contratos.AnyAsync(x => x.FornecedorId == request.FornecedorId && x.NumeroContrato == request.NumeroContrato && x.Id != id, ct))
            throw new AppException("Número do contrato já cadastrado para este fornecedor.");

        entity.FornecedorId = request.FornecedorId;
        entity.NumeroContrato = request.NumeroContrato;
        entity.Descricao = request.Descricao;
        entity.DataInicio = request.DataInicio;
        entity.DataFim = request.DataFim;
        entity.Ativo = request.Ativo;
        entity.DataHoraAtualizacao = DbClock.Now;

        await _context.SaveChangesAsync(ct);
        await _auditService.RegistrarAsync("contratos", entity.Id, AcaoAuditoria.Edicao, $"Contrato {entity.NumeroContrato} alterado.", ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct)
    {
        var entity = await _context.Contratos.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new AppException("Contrato não encontrado.", 404);
        var hasDependency =
            await _context.DocumentosEnviados.AnyAsync(x => x.ContratoId == id, ct) ||
            await _context.FinanceiroLiberacoes.AnyAsync(x => x.ContratoId == id, ct);

        if (hasDependency) throw new AppException("Contrato possui vínculos e não pode ser excluído.", 409);

        _context.Contratos.Remove(entity);
        await _context.SaveChangesAsync(ct);
        await _auditService.RegistrarAsync("contratos", id, AcaoAuditoria.Exclusao, $"Contrato {entity.NumeroContrato} excluído.", ct);
    }
}

public class DocumentoCatalogService
{
    private readonly IAppDbContext _context;
    private readonly IAuditService _auditService;

    public DocumentoCatalogService(IAppDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<IReadOnlyCollection<DocumentoTipoResponse>> ListTiposAsync(CancellationToken ct) =>
        await _context.DocumentosTipos.AsNoTracking().OrderBy(x => x.NomeDocumento)
            .Select(x => new DocumentoTipoResponse(x.Id, x.Codigo, x.NomeDocumento, x.Descricao, x.Ativo)).ToListAsync(ct);

    public async Task<IReadOnlyCollection<DocumentoExigidoResponse>> ListExigidosAsync(CancellationToken ct) =>
        await _context.DocumentosExigidos.AsNoTracking().Include(x => x.DocumentoTipo).Include(x => x.Categoria)
            .OrderBy(x => x.DocumentoTipo!.NomeDocumento)
            .Select(x => new DocumentoExigidoResponse(x.Id, x.DocumentoTipoId, x.DocumentoTipo!.NomeDocumento, x.TipoPessoa, x.PorteEmpresa, x.CategoriaId, x.Categoria!.Descricao, x.Obrigatorio, x.Ativo))
            .ToListAsync(ct);

    public async Task<long> CreateTipoAsync(DocumentoTipoRequest request, CancellationToken ct)
    {
        var entity = new DocumentoTipo { Codigo = request.Codigo, NomeDocumento = request.NomeDocumento, Descricao = request.Descricao, Ativo = request.Ativo };
        _context.DocumentosTipos.Add(entity);
        await _context.SaveChangesAsync(ct);
        await _auditService.RegistrarAsync("documentos_tipos", entity.Id, AcaoAuditoria.Criacao, $"Documento tipo {entity.Codigo} criado.", ct);
        return entity.Id;
    }

    public async Task UpdateTipoAsync(long id, DocumentoTipoRequest request, CancellationToken ct)
    {
        var entity = await _context.DocumentosTipos.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new AppException("Tipo de documento não encontrado.", 404);
        if (await _context.DocumentosTipos.AnyAsync(x => x.Codigo == request.Codigo && x.Id != id, ct)) throw new AppException("Código do documento já cadastrado.");

        entity.Codigo = request.Codigo;
        entity.NomeDocumento = request.NomeDocumento;
        entity.Descricao = request.Descricao;
        entity.Ativo = request.Ativo;
        entity.DataHoraAtualizacao = DbClock.Now;

        await _context.SaveChangesAsync(ct);
        await _auditService.RegistrarAsync("documentos_tipos", entity.Id, AcaoAuditoria.Edicao, $"Documento tipo {entity.Codigo} alterado.", ct);
    }

    public async Task DeleteTipoAsync(long id, CancellationToken ct)
    {
        var entity = await _context.DocumentosTipos.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new AppException("Tipo de documento não encontrado.", 404);
        var hasDependency =
            await _context.DocumentosExigidos.AnyAsync(x => x.DocumentoTipoId == id, ct) ||
            await _context.DocumentosRegistrados.AnyAsync(x => x.DocumentoTipoId == id, ct);
        if (hasDependency) throw new AppException("Tipo de documento possui vínculos e não pode ser excluído.", 409);

        _context.DocumentosTipos.Remove(entity);
        await _context.SaveChangesAsync(ct);
        await _auditService.RegistrarAsync("documentos_tipos", id, AcaoAuditoria.Exclusao, $"Documento tipo {entity.Codigo} excluído.", ct);
    }

    public async Task<long> CreateExigidoAsync(DocumentoExigidoRequest request, CancellationToken ct)
    {
        if (request.TipoPessoa == TipoPessoa.Fisica && request.PorteEmpresa.HasValue) throw new AppException("Pessoa física não pode ter porte.");
        if (request.TipoPessoa == TipoPessoa.Juridica && !request.PorteEmpresa.HasValue) throw new AppException("Porte é obrigatório para pessoa jurídica.");

        var entity = new DocumentoExigido
        {
            DocumentoTipoId = request.DocumentoTipoId,
            TipoPessoa = request.TipoPessoa,
            PorteEmpresa = request.PorteEmpresa,
            CategoriaId = request.CategoriaId,
            Obrigatorio = request.Obrigatorio,
            Ativo = request.Ativo
        };

        _context.DocumentosExigidos.Add(entity);
        await _context.SaveChangesAsync(ct);
        await _auditService.RegistrarAsync("documentos_exigidos", entity.Id, AcaoAuditoria.Criacao, $"Documento exigido {entity.DocumentoTipoId} criado.", ct);
        return entity.Id;
    }

    public async Task UpdateExigidoAsync(long id, DocumentoExigidoRequest request, CancellationToken ct)
    {
        if (request.TipoPessoa == TipoPessoa.Fisica && request.PorteEmpresa.HasValue) throw new AppException("Pessoa física não pode ter porte.");
        if (request.TipoPessoa == TipoPessoa.Juridica && !request.PorteEmpresa.HasValue) throw new AppException("Porte é obrigatório para pessoa jurídica.");
        var entity = await _context.DocumentosExigidos.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new AppException("Regra de documento não encontrada.", 404);
        if (await _context.DocumentosExigidos.AnyAsync(x => x.DocumentoTipoId == request.DocumentoTipoId && x.TipoPessoa == request.TipoPessoa && x.PorteEmpresa == request.PorteEmpresa && x.CategoriaId == request.CategoriaId && x.Id != id, ct))
            throw new AppException("Já existe uma regra para essa combinação.");

        entity.DocumentoTipoId = request.DocumentoTipoId;
        entity.TipoPessoa = request.TipoPessoa;
        entity.PorteEmpresa = request.PorteEmpresa;
        entity.CategoriaId = request.CategoriaId;
        entity.Obrigatorio = request.Obrigatorio;
        entity.Ativo = request.Ativo;
        entity.DataHoraAtualizacao = DbClock.Now;

        await _context.SaveChangesAsync(ct);
        await _auditService.RegistrarAsync("documentos_exigidos", entity.Id, AcaoAuditoria.Edicao, $"Documento exigido {entity.DocumentoTipoId} alterado.", ct);
    }

    public async Task DeleteExigidoAsync(long id, CancellationToken ct)
    {
        var entity = await _context.DocumentosExigidos.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new AppException("Regra de documento não encontrada.", 404);
        _context.DocumentosExigidos.Remove(entity);
        await _context.SaveChangesAsync(ct);
        await _auditService.RegistrarAsync("documentos_exigidos", id, AcaoAuditoria.Exclusao, $"Documento exigido {entity.DocumentoTipoId} excluído.", ct);
    }
}
