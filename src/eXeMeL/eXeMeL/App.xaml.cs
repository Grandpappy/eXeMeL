using System;
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

      ApplicationThemeManager.Apply(ApplicationTheme.Dark);

      if (e.Args.Length > 0)
      {
        StartupOptions.InitialFilePath = e.Args[0];
      }
    }

    private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
      // Avoid calling ourselves recursively. Visible when a "staircase" of message boxes are displayed.
      this.DispatcherUnhandledException -= Application_DispatcherUnhandledException;


      var error = string.Empty;
      var currentException = e.Exception;
      while (currentException != null)
      {
        error += currentException.Message + Environment.NewLine + Environment.NewLine;
        currentException = currentException.InnerException;
      }

      MessageBox.Show(error);
    }
  }
}
