using System.Reflection;

namespace caffeine.ViewModels;

/// <summary>
/// ViewModel for the About dialog containing application information and support links.
/// </summary>
public class AboutViewModel
{
    /// <summary>
    /// Gets the application version number.
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Gets the URL for Buy Me a Coffee support.
    /// </summary>
    public string BuyMeCoffeeUrl { get; } = "https://www.buymeacoffee.com/teocci";

    /// <summary>
    /// Gets the URL for Patreon support.
    /// </summary>
    public string PatreonUrl { get; } = "https://www.patreon.com/teocci";

    /// <summary>
    /// Gets the URL for the GitHub repository.
    /// </summary>
    public string GitHubUrl { get; } = "https://github.com/teocci/csh-caffeine";

    /// <summary>
    /// Gets the copyright text.
    /// </summary>
    public string Copyright { get; } = $"© {DateTime.Now.Year} Teocci";

    /// <summary>
    /// Initializes a new instance of the AboutViewModel class.
    /// </summary>
    public AboutViewModel()
    {
        // Get version from assembly
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        Version = version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "1.0.0";
    }
}
