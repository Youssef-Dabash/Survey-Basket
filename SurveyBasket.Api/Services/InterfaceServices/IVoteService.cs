using SurveyBasket.Api.Abstractions;
using SurveyBasket.Api.Contracts.Votes;

namespace SurveyBasket.Api.Services.InterfaceServices;

public interface IVoteService
{
    Task<Result> AddAsync(int pollId, string userId, VoteRequest request, CancellationToken cancellationToken = default);
}
