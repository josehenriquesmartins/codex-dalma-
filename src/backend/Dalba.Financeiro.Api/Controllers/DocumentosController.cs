using Dalba.Financeiro.Application.DTOs.Documentos;
using Dalba.Financeiro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dalba.Financeiro.Api.Controllers;

[ApiController]
[Route("api/documentos")]
[Authorize]
public class DocumentosController : ControllerBase
{
    [HttpGet("tipos")]
    public async Task<IActionResult> GetTipos([FromServices] DocumentoCatalogService service, CancellationToken ct) => Ok(await service.ListTiposAsync(ct));

    [HttpGet("exigidos")]
    public async Task<IActionResult> GetExigidos([FromServices] DocumentoCatalogService service, CancellationToken ct) => Ok(await service.ListExigidosAsync(ct));

    [HttpPost("tipos")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> PostTipo([FromServices] DocumentoCatalogService service, [FromBody] DocumentoTipoRequest request, CancellationToken ct)
        => Ok(new { id = await service.CreateTipoAsync(request, ct) });

    [HttpPut("tipos/{id:long}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> PutTipo([FromServices] DocumentoCatalogService service, long id, [FromBody] DocumentoTipoRequest request, CancellationToken ct)
    {
        await service.UpdateTipoAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("tipos/{id:long}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteTipo([FromServices] DocumentoCatalogService service, long id, CancellationToken ct)
    {
        await service.DeleteTipoAsync(id, ct);
        return NoContent();
    }

    [HttpPost("exigidos")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> PostExigido([FromServices] DocumentoCatalogService service, [FromBody] DocumentoExigidoRequest request, CancellationToken ct)
        => Ok(new { id = await service.CreateExigidoAsync(request, ct) });

    [HttpPut("exigidos/{id:long}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> PutExigido([FromServices] DocumentoCatalogService service, long id, [FromBody] DocumentoExigidoRequest request, CancellationToken ct)
    {
        await service.UpdateExigidoAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("exigidos/{id:long}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteExigido([FromServices] DocumentoCatalogService service, long id, CancellationToken ct)
    {
        await service.DeleteExigidoAsync(id, ct);
        return NoContent();
    }
}
