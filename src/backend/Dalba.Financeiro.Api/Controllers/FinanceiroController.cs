using Dalba.Financeiro.Application.DTOs.Financeiro;
using Dalba.Financeiro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dalba.Financeiro.Api.Controllers;

[ApiController]
[Route("api/financeiro")]
[Authorize(Policy = "AdminOrFinanceiro")]
public class FinanceiroController : ControllerBase
{
    [HttpGet("liberacoes")]
    public async Task<IActionResult> Get([FromServices] FinanceiroService service, [FromQuery] short mesReferencia, [FromQuery] short anoReferencia, CancellationToken ct) =>
        Ok(await service.ListAsync(mesReferencia, anoReferencia, ct));

    [HttpPut("liberacoes/{id:long}")]
    public async Task<IActionResult> Put([FromServices] FinanceiroService service, long id, [FromBody] AtualizarFinanceiroRequest request, CancellationToken ct)
    {
        await service.AtualizarAsync(id, request, ct);
        return NoContent();
    }
}
