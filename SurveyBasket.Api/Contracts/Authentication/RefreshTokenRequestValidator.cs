namespace SurveyBasket.Api.Contracts.Authentication;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(s => s.token).NotEmpty();
        RuleFor(s => s.refreshToken).NotEmpty();
    }
}
