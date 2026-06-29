namespace AboardKleerIntegration.Services;

public interface ISlackService
{
    Task SendMessageAsync(string message);
}