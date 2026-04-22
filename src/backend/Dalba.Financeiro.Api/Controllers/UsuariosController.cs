using Dalba.Financeiro.Application.DTOs.Usuarios;
using Dalba.Financeiro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dalba.Financeiro.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Policy = "AdminOnly")]
public class UsuariosController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromServices] UsuarioService service, CancellationToken ct) => Ok(await service.ListAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Post([FromServices] UsuarioService service, [FromBody] UsuarioRequest request, CancellationToken ct)
        => Ok(new { id = await service.CreateAsync(request, ct) });

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Put([FromServices] UsuarioService service, long id, [FromBody] UsuarioRequest request, CancellationToken ct)
    {
        await service.UpdateAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete([FromServices] UsuarioService service, long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }
}
