using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Velopack;
using Velopack.Sources;
using Wpf.Ui.Appearance;

namespace eXeMeL
{
  public static class StartupOptions
  {
    public static string InitialFilePath { get; set; }
  }



  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    private const string GitHubRepoUrl = "https://github.com/Grandpappy/eXeMeL";

    [STAThread]
    private static void Main(string[] args)
    {
      // Velopack MUST be first — handles install/uninstall/update hooks
      VelopackApp.Build()
        .Run();

      var app = new App();
      app.InitializeComponent();
      app.Run();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);

      // Catch truly fatal exceptions that bypass Dispatcher
      AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

      ApplicationThemeManager.Apply(ApplicationTheme.Dark);

      if (e.Args.Length > 0)
      {
        StartupOptions.InitialFilePath = e.Args[0];
      }

      // Update check is triggered by MainWindow.Loaded → CheckForUpdatesOnStartup()
    }

    /// <summary>
    /// Checks for updates from GitHub Releases. If silent, only notifies when update is available.
    /// If not silent (user-initiated), shows status messages.
    /// </summary>
    public static async Task<bool> CheckForUpdatesAsync(bool silent = false)
    {
      try
      {
        var mgr = new UpdateManager(new GithubSource(GitHubRepoUrl, null, false));

        if (!mgr.IsInstalled)
        {
          // Running from dev/unpackaged — skip update check
          return false;
        }

        var updateInfo = await mgr.CheckForUpdatesAsync();
        if (updateInfo == null)
        {
          return false; // No update available
        }

        // Update available — store the info for the UI to pick up
        LatestUpdate = updateInfo;
        LatestUpdateManager = mgr;
        return true;
      }
      catch
      {
        // Network error, rate limit, etc. — fail silently
        return false;
      }
    }

    /// <summary>Downloads and applies the pending update, then restarts.</summary>
    public static async Task ApplyUpdateAsync()
    {
      if (LatestUpdate == null || LatestUpdateManager == null) return;

      try
      {
        await LatestUpdateManager.DownloadUpdatesAsync(LatestUpdate);
        LatestUpdateManager.ApplyUpdatesAndRestart(LatestUpdate);
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Update failed: {ex.Message}", "Update Error",
          MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    /// <summary>Stored update info for the UI to access.</summary>
    public static UpdateInfo LatestUpdate { get; private set; }
    public static UpdateManager LatestUpdateManager { get; private set; }


    private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
      e.Handled = true;

      try
      {
        var errorText = FormatException(e.Exception);
        var crashFile = WriteCrashLog(errorText);

        MessageBox.Show(
          errorText + Environment.NewLine + Environment.NewLine + $"Crash log saved to: {crashFile}",
          "eXeMeL - Unhandled Exception",
          MessageBoxButton.OK,
          MessageBoxImage.Error);
      }
      catch
      {
        try { WriteCrashLog(FormatException(e.Exception)); } catch { }
      }
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      try
      {
        var errorText = e.ExceptionObject is Exception ex
          ? FormatException(ex)
          : e.ExceptionObject?.ToString() ?? "Unknown fatal error";

        WriteCrashLog(errorText);
      }
      catch { }
    }

    private static string FormatException(Exception ex)
    {
      var error = string.Empty;
      var current = ex;
      while (current != null)
      {
        error += current.GetType().FullName + ": " + current.Message + Environment.NewLine
               + current.StackTrace + Environment.NewLine + Environment.NewLine;
        current = current.InnerException;
      }
      return error;
    }

    private static string WriteCrashLog(string content)
    {
      var crashDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "eXeMeL");
      Directory.CreateDirectory(crashDir);

      var crashFile = Path.Combine(crashDir, $"crash_{DateTime.Now:yyyyMMdd_HHmmss}.log");
      File.WriteAllText(crashFile, content);
      return crashFile;
    }
  }
}
