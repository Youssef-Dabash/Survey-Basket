namespace SurveyBasket.Api.Abstractions;

public static class ResultExtensions
{
    public static ObjectResult ToProblem(this Result result, int statusCode)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException();

        var problem = Results.Problem(statusCode: statusCode);
        var problemDetails = problem.GetType().GetProperty(nameof(ProblemDetails))!.GetValue(problem) as ProblemDetails;

        problemDetails!.Extensions = new Dictionary<string, object?>()
        {
            {
                "error", new[]{ result.Error }
            }
        };

        //var problemDetails = new ProblemDetails
        //{
        //    Status = statusCode,
        //    Title = title,
        //    Extensions = new Dictionary<string, object?>()
        //    {
        //        {
        //            "error", new[]{ result.Error }
        //        }
        //    }
        //};

        return new ObjectResult(problemDetails);
    }
}
