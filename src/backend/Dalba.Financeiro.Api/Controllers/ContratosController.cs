using Dalba.Financeiro.Application.DTOs.Contratos;
using Dalba.Financeiro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dalba.Financeiro.Api.Controllers;

[ApiController]
[Route("api/contratos")]
[Authorize]
public class ContratosController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromServices] ContratoService service, CancellationToken ct) => Ok(await service.ListAsync(ct));

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Post([FromServices] ContratoService service, [FromBody] ContratoRequest request, CancellationToken ct)
        => Ok(new { id = await service.CreateAsync(request, ct) });

    [HttpPut("{id:long}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Put([FromServices] ContratoService service, long id, [FromBody] ContratoRequest request, CancellationToken ct)
    {
        await service.UpdateAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete([FromServices] ContratoService service, long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }
}
