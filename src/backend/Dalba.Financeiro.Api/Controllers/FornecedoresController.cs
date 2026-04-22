using Dalba.Financeiro.Application.DTOs.Fornecedores;
using Dalba.Financeiro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dalba.Financeiro.Api.Controllers;

[ApiController]
[Route("api/fornecedores")]
[Authorize]
public class FornecedoresController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "AdminOrFinanceiro")]
    public async Task<IActionResult> Get([FromServices] FornecedorService service, CancellationToken ct) => Ok(await service.ListAsync(ct));

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Post([FromServices] FornecedorService service, [FromBody] FornecedorRequest request, CancellationToken ct)
        => Ok(new { id = await service.CreateAsync(request, ct) });

    [HttpPut("{id:long}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Put([FromServices] FornecedorService service, long id, [FromBody] FornecedorRequest request, CancellationToken ct)
    {
        await service.UpdateAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete([FromServices] FornecedorService service, long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }
}
