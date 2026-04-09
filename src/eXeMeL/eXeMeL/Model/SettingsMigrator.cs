using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.Win32;

namespace eXeMeL.Model
{
  internal static class SettingsMigrator
  {
    /// <summary>
    /// Migrates settings from the Windows Registry (old format) to the file-based JSON format
    /// if the new settings file does not already exist. The registry values are left intact as a backup.
    /// </summary>
    public static void MigrateIfNeeded()
    {
      var settingsFilePath = SettingsIO.GetSettingsFilePath();

      if (File.Exists(settingsFilePath))
      {
        return;
      }

      try
      {
        Settings migratedSettings = TryReadFromRegistry();

        if (migratedSettings != null)
        {
          // Also migrate LastLaunchedVersion from the registry
          migratedSettings.LastLaunchedVersion = TryReadLastLaunchedVersionFromRegistry();

          SettingsIO.SaveSettings(migratedSettings);
        }
      }
      catch
      {
        // Migration failed — the app will fall back to defaults via SettingsIO.LoadSettings
      }
    }



    private static Settings TryReadFromRegistry()
    {
#pragma warning disable CS0612 // Type or member is obsolete
      try
      {
        using (var registryKey = RegistryAccess.OpenRegistryKey())
        {
          var value = registryKey.GetValue("Settings") as string;

          if (string.IsNullOrEmpty(value))
          {
            return null;
          }

          using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(value)))
          {
            var serializer = new DataContractJsonSerializer(typeof(Settings));
            return serializer.ReadObject(memoryStream) as Settings;
          }
        }
      }
      catch
      {
        return null;
      }
#pragma warning restore CS0612
    }



    private static string TryReadLastLaunchedVersionFromRegistry()
    {
#pragma warning disable CS0612 // Type or member is obsolete
      try
      {
        using (var registryKey = RegistryAccess.OpenRegistryKey())
        {
          return registryKey.GetValue("LastLaunchedVersion", null) as string;
        }
      }
      catch
      {
        return null;
      }
#pragma warning restore CS0612
    }
  }
}
