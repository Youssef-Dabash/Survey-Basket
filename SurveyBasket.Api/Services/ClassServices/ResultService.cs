using SurveyBasket.Api.Abstractions;
using SurveyBasket.Api.Contracts.Questions;
using SurveyBasket.Api.Contracts.Results;
using SurveyBasket.Api.Errors;
using SurveyBasket.Api.Services.InterfaceServices;

namespace SurveyBasket.Api.Services.ClassServices;

public class ResultService(ApplicationDbContext context) : IResultService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Result<PollVotesResponse>> GetPollVotesAsync(int pollId, CancellationToken cancellationToken = default)
    {
        var pollVotes = await _context.Polls
            .Where(x => x.Id == pollId)
            .Select(s => new PollVotesResponse(
                    s.Title,
                    s.Votes.Select(v => new VoteResponse(
                        $"{v.user.FirstName} {v.user.LastName}",
                        v.SubmittedOn,
                        v.VoteAnswer.Select(a => new QuestionAnswerResponse(
                            a.Question.Content,
                            a.Answer.Content
                        ))
                    ))
            ))
            .SingleOrDefaultAsync(cancellationToken);

        return pollVotes is null
            ? Result.Failure<PollVotesResponse>(PollErrors.PollNotFound)
            : Result.Success(pollVotes);
    }

    public async Task<Result<IEnumerable<VotePerDayResponse>>> GetVotesPerDayAsync(int pollId, CancellationToken cancellationToken = default)
    {
        var pollIsExists = await _context.Polls.AnyAsync(x => x.Id == pollId, cancellationToken);

        if (!pollIsExists)
            return Result.Failure<IEnumerable<VotePerDayResponse>>(PollErrors.PollNotFound);

        var votesPerDay = await _context.Votes
            .Where(x => x.PollId == pollId)
            .GroupBy(g => new { VoteDate = DateOnly.FromDateTime(g.SubmittedOn) })
            .Select(v => new VotePerDayResponse(
                v.Key.VoteDate,
                v.Count()
            ))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<VotePerDayResponse>>(votesPerDay);
    }

    public async Task<Result<IEnumerable<VotePerQuestionResponse>>> GetVotesPerQuestionAsync(int pollId, CancellationToken cancellationToken = default)
    {
        var pollIsExists = await _context.Polls.AnyAsync(x => x.Id == pollId, cancellationToken);

        if (!pollIsExists)
            return Result.Failure<IEnumerable<VotePerQuestionResponse>>(PollErrors.PollNotFound);

        var votesPerQuestion = await _context.VoteAnswers
            .Where(x => x.Vote.PollId == pollId)
            .Select(s => new VotePerQuestionResponse(
                s.Question.Content,
                s.Question.Votes
                    .GroupBy(g => new { AnswerId = g.Answer.Id, AnswerContent = g.Answer.Content })
                    .Select(s => new VotesPerAnswerResponse(
                        s.Key.AnswerContent,
                        s.Count()
                    ))

            ))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<VotePerQuestionResponse>>(votesPerQuestion);
    }
}
