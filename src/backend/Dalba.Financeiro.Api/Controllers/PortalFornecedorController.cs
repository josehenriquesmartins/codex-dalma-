using Dalba.Financeiro.Application.DTOs.Portal;
using Dalba.Financeiro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dalba.Financeiro.Api.Controllers;

[ApiController]
[Route("api/portal-fornecedor")]
[Authorize(Policy = "FornecedorOnly")]
public class PortalFornecedorController : ControllerBase
{
    [HttpPost("envios")]
    public async Task<IActionResult> CriarOuConsultar([FromServices] SupplierPortalService service, [FromBody] CriarEnvioMensalRequest request, CancellationToken ct)
        => Ok(await service.GetOrCreateAsync(request, ct));

    [HttpPost("envios/{envioId:long}/upload/{documentoTipoId:long}")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload([FromServices] SupplierPortalService service, long envioId, long documentoTipoId, [FromForm] List<IFormFile> files, CancellationToken ct)
        => Ok(await service.UploadAsync(envioId, documentoTipoId, files, ct));

    [HttpGet("documentos-registrados/{documentoRegistradoId:long}/visualizacao")]
    public async Task<IActionResult> VisualizarDocumento([FromServices] SupplierPortalService service, long documentoRegistradoId, CancellationToken ct)
    {
        var documento = await service.VisualizarDocumentoAsync(documentoRegistradoId, ct);
        return File(documento.Conteudo, documento.ContentType, enableRangeProcessing: true);
    }
}
