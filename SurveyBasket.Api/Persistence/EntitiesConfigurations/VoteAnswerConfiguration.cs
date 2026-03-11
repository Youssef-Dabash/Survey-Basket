using SurveyBasket.Api.Entities;

namespace SurveyBasket.Persistence.EntitiesConfigurations;

public class VoteAnswerConfiguration : IEntityTypeConfiguration<VoteAnswer>
{
    public void Configure(EntityTypeBuilder<VoteAnswer> builder)
    {
        builder.HasIndex(x => new { x.VoteId , x.QuestionId }).IsUnique();
    }
}