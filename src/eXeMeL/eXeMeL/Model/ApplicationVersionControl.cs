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
    public static bool CurrentVersionIsDifferentFromLastRunVersion()
    {
      using (var registryKey = RegistryAccess.OpenRegistryKey())
      {
        var value = registryKey.GetValue("LastLaunchedVersion", "1.0.0.0") as string;
        var publishedVersion = GetPublishedVersion().ToString();

        if (value == publishedVersion)
        {
          return true;
        }
        else
        {
          return false;
        }
      }
    }



    public static void WriteCurrentVersionToRegistry()
    {
      using (var registryKey = RegistryAccess.OpenRegistryKey())
      {
        var publishedVersion = GetPublishedVersion().ToString();
        registryKey.SetValue("LastLaunchedVersion", publishedVersion);
      }

    }



    public static Version GetPublishedVersion()
    {
      var xmlDoc = new XmlDocument();
      var asmCurrent = Assembly.GetExecutingAssembly();
      var executePath = new Uri(asmCurrent.GetName().CodeBase).LocalPath;

      xmlDoc.Load(executePath + ".manifest");
      var retval = string.Empty;
      if (xmlDoc.HasChildNodes)
      {
        retval = xmlDoc.ChildNodes[1].ChildNodes[0].Attributes.GetNamedItem("version").Value.ToString();
      }

      return new Version(retval);
    }

  }
}
