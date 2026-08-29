using Microsoft.UI.Xaml;
using Anon.Shell.M0.Runtime;

namespace Anon.Shell.M0;

public partial class App : Application
{
    private Window? _window;
    public static DesktopController Desktop { get; private set; } = null!;

    public App()
    {
        InitializeComponent();
        Desktop = RuntimeFactory.CreateDefault();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        _window.Activate();
    }
}
