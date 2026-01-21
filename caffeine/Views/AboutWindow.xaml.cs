using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using caffeine.ViewModels;

namespace caffeine.Views;

/// <summary>
/// Interaction logic for AboutWindow.xaml
/// </summary>
public partial class AboutWindow : Window
{
    private readonly AboutViewModel _viewModel;

    /// <summary>
    /// Initializes a new instance of the AboutWindow class.
    /// </summary>
    public AboutWindow()
    {
        InitializeComponent();

        _viewModel = new AboutViewModel();
        DataContext = _viewModel;
    }

    /// <summary>
    /// Handles the Buy Me a Coffee button click.
    /// </summary>
    private void BuyMeCoffee_Click(object sender, RoutedEventArgs e)
    {
        OpenUrl(_viewModel.BuyMeCoffeeUrl);
    }

    /// <summary>
    /// Handles the Patreon button click.
    /// </summary>
    private void Patreon_Click(object sender, RoutedEventArgs e)
    {
        OpenUrl(_viewModel.PatreonUrl);
    }

    /// <summary>
    /// Handles hyperlink navigation.
    /// </summary>
    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        OpenUrl(e.Uri.AbsoluteUri);
        e.Handled = true;
    }

    /// <summary>
    /// Opens a URL in the default browser.
    /// </summary>
    /// <param name="url">The URL to open.</param>
    private void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to open URL: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
