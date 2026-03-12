namespace SurveyBasket.Api.Contracts.Results;

public record VotePerQuestionResponse(
    string Question,
    IEnumerable<VotesPerAnswerResponse> SelectedAnswers
);
