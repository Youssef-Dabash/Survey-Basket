using SurveyBasket.Api.Abstractions;
using SurveyBasket.Api.Contracts.Users;

namespace SurveyBasket.Api.Services.InterfaceServices;

public interface IUserService
{
    Task<Result<UserProfileResponse>> GetProfileAsync(string userId);
    Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request);
    Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request);
}
