using System.ComponentModel;
using System.Windows.Media;
using eXeMeL.Utilities;

namespace eXeMeL.Model
{
  public enum ApplicationTheme
  {
    [Description("Light")]
    [AssociatedResourceDictionary(@"pack://application:,,,/Resources/ThemeColors.xaml")]
    Light,

    [Description("Dark")]
    [AssociatedResourceDictionary(@"pack://application:,,,/Resources/DarkThemeColors.xaml")]
    Dark,

    [Description("Solarized (Dark)")]
    [AssociatedResourceDictionary(@"pack://application:,,,/Resources/SolarizedDarkThemeColors.xaml")]
    SolarizedDark,

    [Description("Glass")]
    [AssociatedResourceDictionary(@"pack://application:,,,/Resources/GlassDarkThemeColors.xaml")]
    Glass,

    [Description("Tinted")]
    [AssociatedResourceDictionary(@"pack://application:,,,/Resources/TintedDarkThemeColors.xaml")]
    Tinted,

    [DoNotDisplayInSettings]
    Any
  }



  public static class ApplicationThemeExtensions
  {
    public static string GetResourceDictionaryPath(this ApplicationTheme theme)
    {
      return theme.GetAttributeValue<AssociatedResourceDictionaryAttribute, string>(x => x.ResourceDictionaryPath);
    }

    public static bool IsGlassTheme(this ApplicationTheme theme)
    {
      return theme == ApplicationTheme.Glass;
    }

    public static bool IsTintedTheme(this ApplicationTheme theme)
    {
      return theme == ApplicationTheme.Tinted;
    }

    public static bool SupportsTint(this ApplicationTheme theme)
    {
      return theme == ApplicationTheme.Glass || theme == ApplicationTheme.Tinted;
    }

    /// <summary>
    /// Determines whether to use WPF-UI Light or Dark theme based on the tint color luminance.
    /// </summary>
    public static bool IsLightColor(string hexColor)
    {
      try
      {
        var color = (Color)ColorConverter.ConvertFromString(hexColor);
        var luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
        return luminance > 0.5;
      }
      catch
      {
        return false;
      }
    }
  }
}
