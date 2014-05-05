using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace eXeMeL.Model
{
  internal static class SettingsIO
  {
    public static void SaveSettings(Settings settings)
    {
      using (var s = new MemoryStream())
      {
        using (var registryKey = RegistryAccess.OpenRegistryKey())
        {
          var serializer = new DataContractJsonSerializer(settings.GetType());
          serializer.WriteObject(s, settings);

          var value = Encoding.UTF8.GetString(s.ToArray());

          registryKey.SetValue("Settings", value);
        }
      }
    }



    public static T LoadSettings<T>()
      where T : Settings, new()
    {
      try
      {
        using (var registryKey = RegistryAccess.OpenRegistryKey())
        {
          var value = registryKey.GetValue("Settings") as string;
          using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(value)))
          {
            var serializer = new DataContractJsonSerializer(typeof(T));
            return serializer.ReadObject(memoryStream) as T;
          }
        }
      }
      catch
      {
        return new T();
      }
    }
  }
}
