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
using Microsoft.EntityFrameworkCore;

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
        var documentoNormalizado = ValidationHelper.SomenteDigitos(request.CpfOuCnpj);

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
        var documentoNormalizado = ValidationHelper.SomenteDigitos(request.CpfOuCnpj);

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

    private async Task ValidateFornecedorAsync(FornecedorRequest request, long? id, CancellationToken ct)
    {
        if (!await _context.Categorias.AnyAsync(x => x.Id == request.CategoriaId && x.Ativo, ct)) throw new AppException("Categoria obrigatória e inválida.");
        if (!ValidationHelper.IsValidEmail(request.Email)) throw new AppException("E-mail inválido.");
        var document = ValidationHelper.SomenteDigitos(request.CpfOuCnpj);
        if (request.TipoPessoa == TipoPessoa.Fisica && !ValidationHelper.IsValidCpf(document)) throw new AppException("CPF inválido.");
        if (request.TipoPessoa == TipoPessoa.Juridica && !ValidationHelper.IsValidCnpj(document)) throw new AppException("CNPJ inválido.");
        if (request.TipoPessoa == TipoPessoa.Juridica && !request.PorteEmpresa.HasValue) throw new AppException("Porte da empresa é obrigatório para pessoa jurídica.");
        if (request.TipoPessoa == TipoPessoa.Fisica && request.PorteEmpresa.HasValue) throw new AppException("Pessoa física não pode possuir porte.");
        if (await _context.Fornecedores.AnyAsync(x => x.CpfOuCnpj == document && x.Id != id, ct)) throw new AppException("CPF/CNPJ já cadastrado.");
        if (await _context.Fornecedores.AnyAsync(x => x.CodigoFornecedor == request.CodigoFornecedor && x.Id != id, ct)) throw new AppException("Código do fornecedor já cadastrado.");
        var usuarioFornecedorId = id.HasValue
            ? await _context.Usuarios.Where(x => x.FornecedorId == id.Value).Select(x => (long?)x.Id).FirstOrDefaultAsync(ct)
            : null;
        if (await _context.Usuarios.AnyAsync(x => x.Email == request.Email && x.Id != usuarioFornecedorId, ct)) throw new AppException("E-mail já cadastrado para outro usuário.");
        if (await _context.Usuarios.AnyAsync(x => x.Login == request.Email && x.Id != usuarioFornecedorId, ct)) throw new AppException("Login derivado do e-mail já está em uso.");
        if (string.IsNullOrWhiteSpace(request.Pais)) throw new AppException("País é obrigatório.");
    }

    private async Task EnsureUsuarioFornecedorAsync(Fornecedor fornecedor, string documentoNormalizado, bool isNewSupplier, CancellationToken ct)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(x => x.FornecedorId == fornecedor.Id, ct);

        if (usuario is null)
        {
            var senhaInicial = documentoNormalizado[..Math.Min(6, documentoNormalizado.Length)];
            usuario = new Usuario
            {
                Nome = fornecedor.NomeOuRazaoSocial,
                Email = fornecedor.Email,
                Login = fornecedor.Email,
                SenhaHashSha256 = SecurityHelper.ComputeSha256(senhaInicial),
                Perfil = PerfilAcesso.Fornecedor,
                FornecedorId = fornecedor.Id,
                Ativo = fornecedor.Ativo
            };

            _context.Usuarios.Add(usuario);
            return;
        }

        usuario.Nome = fornecedor.NomeOuRazaoSocial;
        usuario.Email = fornecedor.Email;
        usuario.Login = fornecedor.Email;
        usuario.Perfil = PerfilAcesso.Fornecedor;
        usuario.Ativo = fornecedor.Ativo;
        usuario.DataHoraAtualizacao = DbClock.Now;
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
