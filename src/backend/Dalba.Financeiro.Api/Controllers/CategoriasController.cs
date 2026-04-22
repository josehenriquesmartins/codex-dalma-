using Dalba.Financeiro.Application.DTOs.Categorias;
using Dalba.Financeiro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dalba.Financeiro.Api.Controllers;

[ApiController]
[Route("api/categorias")]
[Authorize]
public class CategoriasController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromServices] CategoriaService service, CancellationToken ct) => Ok(await service.ListAsync(ct));

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Post([FromServices] CategoriaService service, [FromBody] CategoriaRequest request, CancellationToken ct)
        => Ok(new { id = await service.CreateAsync(request, ct) });

    [HttpPut("{id:long}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Put([FromServices] CategoriaService service, long id, [FromBody] CategoriaRequest request, CancellationToken ct)
    {
        await service.UpdateAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete([FromServices] CategoriaService service, long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }
}
