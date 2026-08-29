using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Animation;

namespace Anon.Shell.M0;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void SetStatus(string text) => StatusText.Text = text;

    private void Play_Click(object sender, RoutedEventArgs e) => SetStatus("Play surface selected — game-session integration is next.");
    private void Library_Click(object sender, RoutedEventArgs e) => SetStatus("Library selected — game discovery is next.");
    private void System_Click(object sender, RoutedEventArgs e) => SetStatus("System surface selected — live telemetry is next.");
    private void Home_Click(object sender, RoutedEventArgs e) => SetStatus("Home.");
    private void Files_Click(object sender, RoutedEventArgs e) => SetStatus("Files surface selected.");
    private void Apps_Click(object sender, RoutedEventArgs e) => SetStatus("Apps surface selected.");
    private void Games_Click(object sender, RoutedEventArgs e) => SetStatus("Games surface selected.");
    private void Ai_Click(object sender, RoutedEventArgs e) => SetStatus("ANON AI is optional and not enabled in M0.");

    private void VisualProfile_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (VisualProfile.SelectedItem is ComboBoxItem item)
            SetStatus($"Visual profile: {item.Content}");
    }

    private void ReducedMotion_Click(object sender, RoutedEventArgs e)
    {
        SetStatus(ReducedMotion.IsChecked == true ? "Reduced motion enabled." : "Reduced motion disabled.");
    }
}
