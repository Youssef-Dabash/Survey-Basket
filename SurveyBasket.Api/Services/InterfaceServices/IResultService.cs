using SurveyBasket.Api.Abstractions;
using SurveyBasket.Api.Contracts.Results;

namespace SurveyBasket.Api.Services.InterfaceServices;

public interface IResultService
{
    public Task<Result<PollVotesResponse>> GetPollVotesAsync(int pollId, CancellationToken cancellationToken = default);
    public Task<Result<IEnumerable<VotePerDayResponse>>> GetVotesPerDayAsync(int pollId, CancellationToken cancellationToken = default);
    public Task<Result<IEnumerable<VotePerQuestionResponse>>> GetVotesPerQuestionAsync(int pollId, CancellationToken cancellationToken = default);
}
