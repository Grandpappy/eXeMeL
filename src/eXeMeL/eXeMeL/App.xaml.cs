using System;
using System.IO;
using System.Windows;
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
    }

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
        // If even the error dialog fails, write to file as last resort
        try
        {
          WriteCrashLog(FormatException(e.Exception));
        }
        catch { }
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
