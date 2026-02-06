using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Windows;
using Syncfusion.Licensing;

namespace DataVisualiser;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        AttachCrashLogging();

        var licenseKey = ConfigurationManager.AppSettings["Syncfusion:LicenseKey"];
        if (!string.IsNullOrWhiteSpace(licenseKey) && !string.Equals(licenseKey, "PUT_YOUR_SYNCFUSION_LICENSE_KEY_HERE", StringComparison.OrdinalIgnoreCase))
            SyncfusionLicenseProvider.RegisterLicense(licenseKey);

        base.OnStartup(e);
    }

    private void AttachCrashLogging()
    {
        // Keep this extremely defensive: we only want to log, never throw.
        try
        {
            DispatcherUnhandledException += (_, ex) =>
            {
                try
                {
                    var path = WriteCrashLog("DispatcherUnhandledException", ex.Exception);
                    MessageBox.Show(
                        $"An unhandled UI exception occurred.\n\nLog written to:\n{path}\n\n{ex.Exception.GetType().Name}: {ex.Exception.Message}",
                        "DataVisualiser Crash",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                catch
                {
                    // ignored
                }

                // Prevent the app from terminating so we can at least show the log path.
                ex.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (_, ex) =>
            {
                try
                {
                    WriteCrashLog("AppDomain.UnhandledException", ex.ExceptionObject as Exception);
                }
                catch
                {
                    // ignored
                }
            };

            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (_, ex) =>
            {
                try
                {
                    WriteCrashLog("TaskScheduler.UnobservedTaskException", ex.Exception);
                }
                catch
                {
                    // ignored
                }

                ex.SetObserved();
            };
        }
        catch
        {
            // ignored
        }
    }

    private static string WriteCrashLog(string category, Exception? exception)
    {
        var dir = Path.Combine(Path.GetTempPath(), "DataAnalyser", "crash-logs");
        Directory.CreateDirectory(dir);

        var file = Path.Combine(dir, $"DataVisualiser-{DateTime.UtcNow:yyyyMMdd-HHmmssfff}-utc.log");

        var sb = new StringBuilder();
        sb.AppendLine($"Category: {category}");
        sb.AppendLine($"UTC: {DateTime.UtcNow:O}");
        sb.AppendLine($"Machine: {Environment.MachineName}");
        sb.AppendLine($"User: {Environment.UserName}");
        sb.AppendLine($"Process: {Environment.ProcessId}");
        sb.AppendLine();

        if (exception == null)
        {
            sb.AppendLine("(No exception object provided)");
        }
        else
        {
            sb.AppendLine(exception.ToString());
        }

        File.WriteAllText(file, sb.ToString());
        return file;
    }
}
