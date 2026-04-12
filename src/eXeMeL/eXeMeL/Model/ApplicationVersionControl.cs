using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace eXeMeL.Model
{
  public static class ApplicationVersionControl
  {
    public static bool CurrentVersionIsDifferentFromLastRunVersion(Settings settings)
    {
      var lastVersion = settings.LastLaunchedVersion ?? "1.0.0.0";
      var publishedVersion = GetPublishedVersion().ToString();

      if (lastVersion == publishedVersion)
      {
        return true;
      }
      else
      {
        return false;
      }
    }



    public static void WriteCurrentVersion(Settings settings)
    {
      var publishedVersion = GetPublishedVersion().ToString();
      settings.LastLaunchedVersion = publishedVersion;
    }



    public static Version GetPublishedVersion()
    {
      return Assembly.GetExecutingAssembly().GetName().Version;
    }

  }
}
