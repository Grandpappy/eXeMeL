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
        var fileName = arguments.ActivationData[0];
        var uri = new Uri(fileName);
        StartupOptions.InitialFilePath = uri.LocalPath;
      }
      else
      if (e.Args.Length > 0)
      {
        StartupOptions.InitialFilePath = e.Args[0];
      }
    }
  }
}
