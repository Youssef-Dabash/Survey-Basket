namespace SurveyBasket.Api.Contracts.Authentication;


public class ResendConfirmationEmailRequestValidator : AbstractValidator<ResendConfirmationEmailRequest>
{
    public ResendConfirmationEmailRequestValidator()
    {
        RuleFor(s => s.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
