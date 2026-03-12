namespace SurveyBasket.Api.Contracts.Results;

public record VotePerDayResponse(
    DateOnly Date,
    int NumberOfVotes
);
