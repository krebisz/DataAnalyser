namespace DataVisualiser.Core.Services.Abstractions;

public interface IUserNotificationService
{
    void ShowError(string title, string message);
}
