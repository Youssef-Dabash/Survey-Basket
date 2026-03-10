using SurveyBasket.Api.Abstractions;

namespace SurveyBasket.Api.Services.InterfaceServices;

public interface IAuthService
{
    Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
    Task<Result> GetRevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default);
}