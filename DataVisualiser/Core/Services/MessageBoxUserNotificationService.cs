using System.Windows;
using DataVisualiser.Core.Services.Abstractions;

namespace DataVisualiser.Core.Services;

public sealed class MessageBoxUserNotificationService : IUserNotificationService
{
    public static MessageBoxUserNotificationService Instance { get; } = new();

    private MessageBoxUserNotificationService()
    {
    }

    public void ShowError(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
