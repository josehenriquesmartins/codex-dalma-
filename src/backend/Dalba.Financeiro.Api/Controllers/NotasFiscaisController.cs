using Dalba.Financeiro.Application.DTOs.Financeiro;
using Dalba.Financeiro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dalba.Financeiro.Api.Controllers;

[ApiController]
[Route("api/notas-fiscais")]
[Authorize(Policy = "FornecedorOnly")]
public class NotasFiscaisController : ControllerBase
{
    [HttpGet("minhas-liberacoes")]
    public async Task<IActionResult> GetMinhasLiberacoes([FromServices] FinanceiroService service, CancellationToken ct) =>
        Ok(await service.ListFornecedorAsync(ct));

    [HttpPut("liberacoes/{id:long}/envio")]
    public async Task<IActionResult> EnviarNotaFiscal([FromServices] FinanceiroService service, long id, [FromForm] EnviarNotaFiscalRequest request, CancellationToken ct)
    {
        await service.EnviarNotaFiscalAsync(id, request, ct);
        return NoContent();
    }
}
