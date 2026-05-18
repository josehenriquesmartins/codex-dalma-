using Dalba.Financeiro.Application.DTOs.Financeiro;
using Dalba.Financeiro.Application.Services;
using Dalba.Financeiro.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dalba.Financeiro.Api.Controllers;

[ApiController]
[Route("api/financeiro")]
[Authorize(Policy = "AdminOrFinanceiro")]
public class FinanceiroController : ControllerBase
{
    [HttpGet("liberacoes")]
    public async Task<IActionResult> Get([FromServices] FinanceiroService service, [FromQuery] short mesReferencia, [FromQuery] short anoReferencia, [FromQuery] StatusFinanceiro? statusFinanceiro, CancellationToken ct) =>
        Ok(await service.ListAsync(mesReferencia, anoReferencia, statusFinanceiro, ct));

    [HttpGet("liberacoes/exportacao-excel")]
    public async Task<IActionResult> ExportarExcel([FromServices] FinanceiroService service, [FromQuery] short mesReferencia, [FromQuery] short anoReferencia, [FromQuery] StatusFinanceiro? statusFinanceiro, CancellationToken ct)
        => File(await service.ExportarExcelAsync(mesReferencia, anoReferencia, statusFinanceiro, ct), "application/vnd.ms-excel; charset=utf-8", $"dalba-custos-{anoReferencia}-{mesReferencia:00}.xls");

    [HttpGet("liberacoes/exportacao-pdf")]
    public async Task<IActionResult> ExportarPdf([FromServices] FinanceiroService service, [FromQuery] short mesReferencia, [FromQuery] short anoReferencia, [FromQuery] StatusFinanceiro? statusFinanceiro, CancellationToken ct)
        => File(await service.ExportarPdfAsync(mesReferencia, anoReferencia, statusFinanceiro, ct), "application/pdf", $"dalba-custos-{anoReferencia}-{mesReferencia:00}.pdf");

    [HttpPut("liberacoes/{id:long}")]
    public async Task<IActionResult> Put([FromServices] FinanceiroService service, long id, [FromBody] AtualizarFinanceiroRequest request, CancellationToken ct)
    {
        await service.AtualizarAsync(id, request, ct);
        return NoContent();
    }
}
