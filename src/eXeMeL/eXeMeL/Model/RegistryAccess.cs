using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace eXeMeL.Model
{
  public class RegistryAccess
  {
    public static RegistryKey OpenRegistryKey()
    {
      var registryKey = Registry.CurrentUser.OpenSubKey("Software\\eXeMeL", true);
      if (registryKey == null)
      {
        registryKey = Registry.CurrentUser.CreateSubKey("Software\\eXeMeL");
      }

      return registryKey;
    }

  }
}
