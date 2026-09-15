using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Anon.Shell.M0;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        base.OnStartup(e);

        // M0 uses an explicit application definition, so StartupUri is not
        // provided by generated WPF startup code. Keep the main window alive
        // for the lifetime of the shell process instead of exiting immediately.
        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        mainWindow.Show();
        Log("M0 WPF main window shown");
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log($"Unhandled UI exception: {e.Exception.GetType().Name}: {e.Exception.Message}");
        MessageBox.Show("ANON OS encountered a desktop UI error. Windows remains available.", "ANON OS", MessageBoxButton.OK, MessageBoxImage.Warning);
        e.Handled = true;
    }

    private static void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            Log($"Unhandled process exception: {ex.GetType().Name}: {ex.Message}");
    }

    public static void Log(string message)
    {
        try
        {
            Directory.CreateDirectory(@"C:\ProgramData\ANON\Logs");
            File.AppendAllText(@"C:\ProgramData\ANON\Logs\m0-wpf.log", $"{DateTime.Now:O} {message}{Environment.NewLine}");
        }
        catch
        {
        }
    }
}
