namespace SurveyBasket.Api.Entities;

public class Vote
{
    public int Id { get; set; }
    public int PollId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime SubmittedOn { get; set; } = DateTime.UtcNow;

    public Poll poll { get; set; } = default!;
    public ApplicationUser user { get; set; } = default!;
    public ICollection<VoteAnswer> VoteAnswer { get; set; } = [];
}
