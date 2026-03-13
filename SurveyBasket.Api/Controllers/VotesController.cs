using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using SurveyBasket.Api.Abstractions;
using SurveyBasket.Api.Contracts.Votes;
using SurveyBasket.Api.Errors;
using SurveyBasket.Api.Extensions;
using SurveyBasket.Api.Services.InterfaceServices;
using System.Security.Claims;

namespace SurveyBasket.Api.Controllers;

[Route("api/polls/{pollId}/vote")]
[ApiController]
[Authorize]
public class VotesController(IQuestionService questionService, IVoteService voteService) : ControllerBase
{
    private readonly IQuestionService _questionService = questionService;
    private readonly IVoteService _voteService = voteService;

    [HttpGet("")]
    [OutputCache(PolicyName = "CachePolicy")]
    public async Task<IActionResult> Start([FromRoute] int pollId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId(); // 1b29b3c7-39c0-404a-b56a-313b754386b7

        var result = await _questionService.GetAvailableAsync(pollId, userId!, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }


    [HttpPost("")]
    public async Task<IActionResult> Vote([FromRoute] int pollId,[FromBody] VoteRequest request, CancellationToken cancellationToken)
    {
        var result = await _voteService.AddAsync(pollId, User.GetUserId()!, request, cancellationToken);

        return result.IsSuccess
            ? Created()
            : result.ToProblem();
    }
}
