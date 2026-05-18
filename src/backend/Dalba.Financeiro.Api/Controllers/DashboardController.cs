using Dalba.Financeiro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dalba.Financeiro.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    [HttpGet("admin")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Admin([FromServices] DashboardService service, [FromQuery] short? mesReferencia, [FromQuery] short? anoReferencia, CancellationToken ct) =>
        Ok(await service.AdminAsync(mesReferencia, anoReferencia, ct));

    [HttpGet("fornecedor")]
    [Authorize(Policy = "FornecedorOnly")]
    public async Task<IActionResult> Fornecedor([FromServices] DashboardService service, [FromQuery] short? mesReferencia, [FromQuery] short? anoReferencia, CancellationToken ct) =>
        Ok(await service.FornecedorAsync(mesReferencia, anoReferencia, ct));

    [HttpGet("financeiro")]
    [Authorize(Policy = "AdminOrFinanceiro")]
    public async Task<IActionResult> Financeiro([FromServices] DashboardService service, [FromQuery] short? mesReferencia, [FromQuery] short? anoReferencia, CancellationToken ct) =>
        Ok(await service.FinanceiroAsync(mesReferencia, anoReferencia, ct));
}
