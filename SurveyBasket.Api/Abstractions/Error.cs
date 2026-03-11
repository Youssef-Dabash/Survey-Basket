namespace SurveyBasket.Api.Abstractions;

public record Error(string title, string detail, int? StatusCode)
{
    public static readonly Error None = new(string.Empty, string.Empty, null);
}
