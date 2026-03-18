using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using SurveyBasket.Api.Helpers;
using SurveyBasket.Api.Services.InterfaceServices;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace SurveyBasket.Api.Services.ClassServices;

public class NotificationService(ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor,
    IEmailSender emailSender
    ) : INotificationService
{
    private readonly ApplicationDbContext _context = context;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IEmailSender _emailSender = emailSender;

    public async Task SendNewPollsNotificaiton(int? pollId = null)
    {
        IEnumerable<Poll> polls = [];
        if (pollId.HasValue)
        {
            var poll = await _context.Polls.SingleOrDefaultAsync(s => s.Id == pollId && s.IsPublished);
            polls = [poll!];
        }
        else
        {
            polls = await _context.Polls
                .Where(s => s.IsPublished && s.StartsAt == DateOnly.FromDateTime(DateTime.UtcNow))
                .AsNoTracking()
                .ToListAsync();
        }
        var users = await _userManager.Users.ToListAsync();

        var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;

        foreach (var poll in polls)
        {
            foreach (var user in users)
            {
                var placeHolder = new Dictionary<string, string>
                {
                    {
                        "{{name}}", user.FirstName
                    },
                    {
                        "{{pollTill}}", poll.Title
                    },
                    {
                        "{{endDate}}", poll.EndsAt.ToString()
                    },
                    {
                        "{{url}}", $"{origin}/poll/start/{poll.Id}"
                    }
                };

                var body = EmailBodyBuilder.GenerateEmailBody("PollNotification", placeHolder);

                await _emailSender.SendEmailAsync(user.Email!, $"Survay Basket: New Poll - {poll.Title}", body);
            }
        }
    }
}
