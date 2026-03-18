namespace SurveyBasket.Api.Services.InterfaceServices;

public interface INotificationService
{
    Task SendNewPollsNotificaiton(int? pollId = null);
}
