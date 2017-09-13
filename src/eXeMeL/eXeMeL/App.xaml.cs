using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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

      var arguments = AppDomain.CurrentDomain.SetupInformation.ActivationArguments;
      if (arguments != null && arguments.ActivationData != null && arguments.ActivationData.Length > 0)
      {
        // This is causing the application manifest to load!
        //var fileName = arguments.ActivationData[0];
        //var uri = new Uri(fileName);
        //StartupOptions.InitialFilePath = uri.LocalPath;
        
        StartupOptions.InitialFilePath = arguments.ActivationData[0];
      }
      // This is causing the application manifest to load!
      //else
      //if (e.Args.Length > 0)
      //{
      //  StartupOptions.InitialFilePath = e.Args[0];
      //}
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
