using Dalba.Financeiro.Application.DTOs.Admin;
using Dalba.Financeiro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dalba.Financeiro.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdminOrFinanceiro")]
public class AdminController : ControllerBase
{
    [HttpGet("envios")]
    public async Task<IActionResult> GetEnvios([FromServices] AdminValidationService service, [FromQuery] short mesReferencia, [FromQuery] short anoReferencia, CancellationToken ct) =>
        Ok(await service.ListPendentesAsync(mesReferencia, anoReferencia, ct));

    [HttpGet("envios/{envioId:long}")]
    public async Task<IActionResult> GetEnvioDetalhe([FromServices] AdminValidationService service, long envioId, CancellationToken ct) =>
        Ok(await service.GetDetalheAsync(envioId, ct));

    [HttpGet("documentos-registrados/{documentoRegistradoId:long}/visualizacao")]
    public async Task<IActionResult> VisualizarDocumento([FromServices] AdminValidationService service, long documentoRegistradoId, CancellationToken ct)
    {
        var documento = await service.VisualizarDocumentoAsync(documentoRegistradoId, ct);
        return File(documento.Conteudo, documento.ContentType, enableRangeProcessing: true);
    }

    [HttpPut("documentos-registrados/{documentoRegistradoId:long}/validacao")]
    public async Task<IActionResult> Validar([FromServices] AdminValidationService service, long documentoRegistradoId, [FromBody] ValidarDocumentoRequest request, CancellationToken ct)
    {
        await service.ValidarDocumentoAsync(documentoRegistradoId, request, ct);
        return NoContent();
    }

    [HttpPost("documentos-registrados/{documentoRegistradoId:long}/analise-ia")]
    public async Task<IActionResult> AnalisarIa([FromServices] AdminValidationService service, long documentoRegistradoId, CancellationToken ct) =>
        Ok(await service.AnalisarDocumentoIaAsync(documentoRegistradoId, ct));

    [HttpGet("configuracoes")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetConfiguracoes([FromServices] SystemConfigurationService service, CancellationToken ct) =>
        Ok(await service.GetAsync(ct));

    [HttpPut("configuracoes")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SalvarConfiguracoes([FromServices] SystemConfigurationService service, [FromBody] SalvarConfiguracaoSistemaRequest request, CancellationToken ct) =>
        Ok(await service.SaveAsync(request, ct));
}
