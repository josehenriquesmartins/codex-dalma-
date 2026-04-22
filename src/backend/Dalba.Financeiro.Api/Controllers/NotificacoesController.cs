using Dalba.Financeiro.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dalba.Financeiro.Api.Controllers;

[ApiController]
[Route("api/notificacoes")]
[Authorize]
public class NotificacoesController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromServices] NotificationService service, CancellationToken ct) => Ok(await service.ListAsync(ct));
}
