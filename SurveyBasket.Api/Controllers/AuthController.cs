using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SurveyBasket.Api.Abstractions;
using SurveyBasket.Api.Contracts.Authentication;
using SurveyBasket.Authentication;

namespace SurveyBasket.Controllers;

[Route("[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var authResult = await _authService.GetTokenAsync(request.Email, request.Password, cancellationToken);

        return authResult.IsSuccess
            ? Ok(authResult.Value) : authResult.ToProblem(StatusCodes.Status400BadRequest);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var authResult = await _authService.GetTokenAsync(request.token, request.refreshToken, cancellationToken);

        return authResult is null ? BadRequest("Invalid Token") : Ok(authResult);
    }

    [HttpPost("revoke-refresh-token")]
    public async Task<IActionResult> RevokeRefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var isRevoked = await _authService.GetRevokeRefreshTokenAsync(request.token, request.refreshToken, cancellationToken);

        return isRevoked ? Ok() : BadRequest("Opertion Falid");
    }
}