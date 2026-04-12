using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using eXeMeL.Model;
using Xunit;

namespace eXeMeL.Tests.Model
{
  #region STA Thread Helper

  /// <summary>
  /// Runs a delegate on an STA thread. Required because the Settings constructor
  /// triggers WPF brush resolution (SolidColorBrush via BrushConverter), which
  /// requires an STA apartment. xUnit runs tests on MTA threads by default.
  /// </summary>
  internal static class StaHelper
  {
    public static void Run(Action action)
    {
      Exception caught = null;
      var thread = new Thread(() =>
      {
        try
        {
          action();
        }
        catch (Exception ex)
        {
          caught = ex;
        }
      });
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
      thread.Join();

      if (caught != null)
      {
        throw new AggregateException("STA thread threw an exception.", caught);
      }
    }

    public static T Run<T>(Func<T> func)
    {
      T result = default(T);
      Run(() => { result = func(); });
      return result;
    }
  }

  #endregion

  #region Default Constructor Tests

  public class SettingsDefaultConstructorTests
  {
    [Fact]
    public void Constructor_ShowEditorLineNumbers_DefaultsToTrue()
    {
      var settings = StaHelper.Run(() => new Settings());

      Assert.True(settings.ShowEditorLineNumbers);
    }

    [Fact]
    public void Constructor_WrapEditorText_DefaultsToTrue()
    {
      var settings = StaHelper.Run(() => new Settings());

      Assert.True(settings.WrapEditorText);
    }

    [Fact]
    public void Constructor_EditorFontSize_DefaultsTo16()
    {
      var settings = StaHelper.Run(() => new Settings());

      Assert.Equal(Settings.DefaultEditorFontSize, settings.EditorFontSize);
      Assert.Equal(16.0, settings.EditorFontSize);
    }

    [Fact]
    public void Constructor_SyntaxHighlightingStyle_DefaultsToLightEarthy()
    {
      var settings = StaHelper.Run(() => new Settings());

      Assert.Equal(SyntaxHighlightingStyle.Dark_Blue, settings.SyntaxHighlightingStyle);
    }

    [Fact]
    public void Constructor_ApplicationTheme_DefaultsToDark()
    {
      var settings = StaHelper.Run(() => new Settings());

      Assert.Equal(ApplicationTheme.Dark, settings.ApplicationTheme);
    }

    [Fact]
    public void Constructor_FontFamily_DefaultsToConsolas()
    {
      var settings = StaHelper.Run(() => new Settings());

      Assert.Equal("Consolas", settings.FontFamily);
    }

    [Fact]
    public void Constructor_HighlightOtherInstancesOfSelection_DefaultsToTrue()
    {
      var settings = StaHelper.Run(() => new Settings());

      Assert.True(settings.HighlightOtherInstancesOfSelection);
    }
  }

  #endregion

  #region Round-Trip Serialization Tests

  public class SettingsSerializationTests
  {
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
      WriteIndented = false,
      NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
      Converters = { new JsonStringEnumConverter() }
    };

    private static string Serialize(Settings settings)
    {
      return JsonSerializer.Serialize(settings, JsonOptions);
    }

    private static Settings Deserialize(string json)
    {
      return JsonSerializer.Deserialize<Settings>(json, JsonOptions);
    }

    [Fact]
    public void RoundTrip_NonDefaultValues_AllPropertiesPreserved()
    {
      Settings original = StaHelper.Run(() =>
      {
        var s = new Settings();
        s.ShowEditorLineNumbers = false;
        s.WrapEditorText = false;
        s.EditorFontSize = 24.5;
        s.SyntaxHighlightingStyle = SyntaxHighlightingStyle.Dark_Blue;
        s.ApplicationTheme = ApplicationTheme.Dark;
        s.FontFamily = "Courier New";
        s.HighlightOtherInstancesOfSelection = true;
        return s;
      });

      var json = Serialize(original);
      var restored = Deserialize(json);

      Assert.Equal(original.ShowEditorLineNumbers, restored.ShowEditorLineNumbers);
      Assert.Equal(original.WrapEditorText, restored.WrapEditorText);
      Assert.Equal(original.EditorFontSize, restored.EditorFontSize);
      Assert.Equal(original.SyntaxHighlightingStyle, restored.SyntaxHighlightingStyle);
      Assert.Equal(original.ApplicationTheme, restored.ApplicationTheme);
      Assert.Equal(original.FontFamily, restored.FontFamily);
      Assert.Equal(original.HighlightOtherInstancesOfSelection, restored.HighlightOtherInstancesOfSelection);
    }

    [Fact]
    public void RoundTrip_DefaultValues_AllPropertiesPreserved()
    {
      Settings original = StaHelper.Run(() => new Settings());

      var json = Serialize(original);
      var restored = Deserialize(json);

      Assert.Equal(original.ShowEditorLineNumbers, restored.ShowEditorLineNumbers);
      Assert.Equal(original.WrapEditorText, restored.WrapEditorText);
      Assert.Equal(original.EditorFontSize, restored.EditorFontSize);
      Assert.Equal(original.SyntaxHighlightingStyle, restored.SyntaxHighlightingStyle);
      Assert.Equal(original.ApplicationTheme, restored.ApplicationTheme);
      Assert.Equal(original.FontFamily, restored.FontFamily);
      Assert.Equal(original.HighlightOtherInstancesOfSelection, restored.HighlightOtherInstancesOfSelection);
    }

    [Fact]
    public void RoundTrip_EachSyntaxHighlightingStyle_Survives()
    {
      foreach (SyntaxHighlightingStyle style in Enum.GetValues(typeof(SyntaxHighlightingStyle)))
      {
        Settings original = StaHelper.Run(() =>
        {
          var s = new Settings();
          s.SyntaxHighlightingStyle = style;
          return s;
        });

        var json = Serialize(original);
        var restored = Deserialize(json);

        Assert.Equal(style, restored.SyntaxHighlightingStyle);
      }
    }

    [Fact]
    public void RoundTrip_EachApplicationTheme_Survives()
    {
      foreach (ApplicationTheme theme in Enum.GetValues(typeof(ApplicationTheme)))
      {
        Settings original = StaHelper.Run(() =>
        {
          var s = new Settings();
          s.ApplicationTheme = theme;
          return s;
        });

        var json = Serialize(original);
        var restored = Deserialize(json);

        Assert.Equal(theme, restored.ApplicationTheme);
      }
    }

    [Fact]
    public void Deserialize_FromJsonPayload_OverridesDefaults()
    {
      // System.Text.Json calls the constructor then sets properties from JSON.
      // Verify final values come from the payload, not constructor defaults.
      var json = "{" +
        "\"ApplicationTheme\":\"Dark\"," +
        "\"EditorFontSize\":20," +
        "\"FontFamily\":\"Arial\"," +
        "\"HighlightOtherInstancesOfSelection\":true," +
        "\"ShowEditorLineNumbers\":false," +
        "\"SyntaxHighlightingStyle\":\"Dark_Ethereal\"," +
        "\"WrapEditorText\":false" +
        "}";

      var restored = Deserialize(json);

      Assert.Equal(ApplicationTheme.Dark, restored.ApplicationTheme);
      Assert.Equal(20.0, restored.EditorFontSize);
      Assert.Equal("Arial", restored.FontFamily);
      Assert.True(restored.HighlightOtherInstancesOfSelection);
      Assert.False(restored.ShowEditorLineNumbers);
      Assert.Equal(SyntaxHighlightingStyle.Dark_Ethereal, restored.SyntaxHighlightingStyle);
      Assert.False(restored.WrapEditorText);
    }
  }

  #endregion

  #region JSON Snapshot Tests

  public class SettingsJsonSnapshotTests
  {
    [Fact]
    public void Snapshot_DefaultJson_DeserializesToExpectedValues()
    {
      var json = "{" +
        "\"ApplicationTheme\":\"Light\"," +
        "\"EditorFontSize\":16," +
        "\"FontFamily\":\"Consolas\"," +
        "\"HighlightOtherInstancesOfSelection\":false," +
        "\"ShowEditorLineNumbers\":true," +
        "\"SyntaxHighlightingStyle\":\"Light_Earthy\"," +
        "\"WrapEditorText\":true" +
        "}";

      var restored = JsonSerializer.Deserialize<Settings>(json, new JsonSerializerOptions
      {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        Converters = { new JsonStringEnumConverter() }
      });

      Assert.True(restored.ShowEditorLineNumbers);
      Assert.True(restored.WrapEditorText);
      Assert.Equal(16.0, restored.EditorFontSize);
      Assert.Equal(SyntaxHighlightingStyle.Light_Earthy, restored.SyntaxHighlightingStyle);
      Assert.Equal(ApplicationTheme.Light, restored.ApplicationTheme);
      Assert.Equal("Consolas", restored.FontFamily);
      Assert.False(restored.HighlightOtherInstancesOfSelection);
    }

    [Fact]
    public void Snapshot_NonDefaultJson_DeserializesToExpectedValues()
    {
      var json = "{" +
        "\"ApplicationTheme\":\"Dark\"," +
        "\"EditorFontSize\":24.5," +
        "\"FontFamily\":\"Courier New\"," +
        "\"HighlightOtherInstancesOfSelection\":true," +
        "\"ShowEditorLineNumbers\":false," +
        "\"SyntaxHighlightingStyle\":\"Dark_Blue\"," +
        "\"WrapEditorText\":false" +
        "}";

      var restored = JsonSerializer.Deserialize<Settings>(json, new JsonSerializerOptions
      {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        Converters = { new JsonStringEnumConverter() }
      });

      Assert.False(restored.ShowEditorLineNumbers);
      Assert.False(restored.WrapEditorText);
      Assert.Equal(24.5, restored.EditorFontSize);
      Assert.Equal(SyntaxHighlightingStyle.Dark_Blue, restored.SyntaxHighlightingStyle);
      Assert.Equal(ApplicationTheme.Dark, restored.ApplicationTheme);
      Assert.Equal("Courier New", restored.FontFamily);
      Assert.True(restored.HighlightOtherInstancesOfSelection);
    }
  }

  #endregion

  #region PropertyChanged Notification Tests

  public class SettingsPropertyChangedTests
  {
    private static List<string> CollectPropertyChangedEvents(Settings settings, Action<Settings> action)
    {
      var fired = new List<string>();
      settings.PropertyChanged += (sender, e) => fired.Add(e.PropertyName);
      action(settings);
      return fired;
    }

    [Fact]
    public void ShowEditorLineNumbers_Set_FiresPropertyChanged()
    {
      var settings = StaHelper.Run(() => new Settings());

      var fired = CollectPropertyChangedEvents(settings, s => s.ShowEditorLineNumbers = false);

      Assert.Contains("ShowEditorLineNumbers", fired);
    }

    [Fact]
    public void WrapEditorText_Set_FiresPropertyChanged()
    {
      var settings = StaHelper.Run(() => new Settings());

      var fired = CollectPropertyChangedEvents(settings, s => s.WrapEditorText = false);

      Assert.Contains("WrapEditorText", fired);
    }

    [Fact]
    public void EditorFontSize_Set_FiresPropertyChanged()
    {
      var settings = StaHelper.Run(() => new Settings());

      var fired = CollectPropertyChangedEvents(settings, s => s.EditorFontSize = 20);

      Assert.Contains("EditorFontSize", fired);
    }

    [Fact]
    public void SyntaxHighlightingStyle_Set_FiresPropertyChanged()
    {
      var settings = StaHelper.Run(() => new Settings());

      var fired = StaHelper.Run(() =>
      {
        return CollectPropertyChangedEvents(settings, s => s.SyntaxHighlightingStyle = SyntaxHighlightingStyle.Dark_Ethereal);
      });

      Assert.Contains("SyntaxHighlightingStyle", fired);
    }

    [Fact]
    public void ApplicationTheme_Set_FiresPropertyChanged()
    {
      var settings = StaHelper.Run(() => new Settings());

      var fired = CollectPropertyChangedEvents(settings, s => s.ApplicationTheme = ApplicationTheme.Light);

      Assert.Contains("ApplicationTheme", fired);
    }

    [Fact]
    public void ApplicationTheme_Set_AlsoFiresEditorBrushChanged()
    {
      var settings = StaHelper.Run(() => new Settings());

      var fired = CollectPropertyChangedEvents(settings, s => s.ApplicationTheme = ApplicationTheme.Light);

      Assert.Contains("ApplicationTheme", fired);
      Assert.Contains("EditorBrush", fired);
    }

    [Fact]
    public void FontFamily_Set_FiresPropertyChanged()
    {
      var settings = StaHelper.Run(() => new Settings());

      var fired = CollectPropertyChangedEvents(settings, s => s.FontFamily = "Arial");

      Assert.Contains("FontFamily", fired);
    }

    [Fact]
    public void HighlightOtherInstancesOfSelection_Set_FiresPropertyChanged()
    {
      var settings = StaHelper.Run(() => new Settings());

      var fired = CollectPropertyChangedEvents(settings, s => s.HighlightOtherInstancesOfSelection = false);

      Assert.Contains("HighlightOtherInstancesOfSelection", fired);
    }
  }

  #endregion
}
