using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace eXeMeL.Model
{
  internal static class SettingsIO
  {
    private static readonly string SettingsDirectory =
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "eXeMeL");

    private static readonly string SettingsFilePath =
      Path.Combine(SettingsDirectory, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
      WriteIndented = true,
      NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
      Converters = { new JsonStringEnumConverter() }
    };


    public static string GetSettingsFilePath() => SettingsFilePath;


    public static void SaveSettings(Settings settings)
    {
      Directory.CreateDirectory(SettingsDirectory);

      var json = JsonSerializer.Serialize(settings, JsonOptions);
      File.WriteAllText(SettingsFilePath, json);
    }



    public static T LoadSettings<T>()
      where T : Settings, new()
    {
      try
      {
        if (!File.Exists(SettingsFilePath))
        {
          return new T();
        }

        var json = File.ReadAllText(SettingsFilePath);
        return JsonSerializer.Deserialize<T>(json, JsonOptions) ?? new T();
      }
      catch
      {
        return new T();
      }
    }
  }
}
