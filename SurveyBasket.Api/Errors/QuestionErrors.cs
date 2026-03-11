
using SurveyBasket.Api.Abstractions;

namespace SurveyBasket.Api.Errors;

public class QuestionErrors
{
    public static readonly Error QuestionNotFound
        = new("Question.NotFound", "No question was found with the given ID", StatusCodes.Status404NotFound);

    public static readonly Error DuplicatedQuestionContent
        = new("Question.DuplicatedContent", "Another question with the same content with exist", StatusCodes.Status409Conflict);
}
