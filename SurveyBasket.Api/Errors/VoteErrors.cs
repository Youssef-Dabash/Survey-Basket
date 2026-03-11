using SurveyBasket.Api.Abstractions;

namespace SurveyBasket.Api.Errors;

public static class VoteErrors
{
    public static readonly Error DuplicatedVote
        = new("Vote.DuplicatedVote", "This user already voted before for this poll", StatusCodes.Status409Conflict);

    public static readonly Error InvalidQuestions
        = new("Vote.InvalidQuestions", "invalid questions", StatusCodes.Status400BadRequest);
}
