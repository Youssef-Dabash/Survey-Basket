namespace SurveyBasket.Api.Contracts.Authentication;

public class ConfirmEmailRequestValidator : AbstractValidator<ConfirmEmailRequest>
{
    public ConfirmEmailRequestValidator()
    {
        RuleFor(s => s.UserId)
            .NotEmpty();

        RuleFor(s => s.Code)
            .NotEmpty();
    }
}