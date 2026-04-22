using Dalba.Financeiro.Application.DTOs.Auth;
using Dalba.Financeiro.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dalba.Financeiro.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromServices] AuthService service, [FromBody] LoginRequest request, CancellationToken ct)
    {
        var response = await service.LoginAsync(request, ct);
        return response is null
            ? Unauthorized(new { message = "Login ou senha inválidos." })
            : Ok(response);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromServices] AuthService service, [FromBody] ForgotPasswordRequest request, CancellationToken ct)
        => Ok(await service.ForgotPasswordAsync(request, ct));

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromServices] AuthService service, [FromBody] ResetPasswordRequest request, CancellationToken ct)
        => Ok(await service.ResetPasswordAsync(request, ct));
}
