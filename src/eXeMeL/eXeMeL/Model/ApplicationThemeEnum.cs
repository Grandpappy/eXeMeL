using System.ComponentModel;
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

    [Description("Glass Light")]
    [AssociatedResourceDictionary(@"pack://application:,,,/Resources/GlassLightThemeColors.xaml")]
    GlassLight,

    [Description("Glass Dark")]
    [AssociatedResourceDictionary(@"pack://application:,,,/Resources/GlassDarkThemeColors.xaml")]
    GlassDark,

    [Description("Tinted Light")]
    [AssociatedResourceDictionary(@"pack://application:,,,/Resources/TintedLightThemeColors.xaml")]
    TintedLight,

    [Description("Tinted Dark")]
    [AssociatedResourceDictionary(@"pack://application:,,,/Resources/TintedDarkThemeColors.xaml")]
    TintedDark,

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
      return theme == ApplicationTheme.GlassLight || theme == ApplicationTheme.GlassDark;
    }

    public static bool IsTintedTheme(this ApplicationTheme theme)
    {
      return theme == ApplicationTheme.TintedLight || theme == ApplicationTheme.TintedDark;
    }

    public static bool SupportsTint(this ApplicationTheme theme)
    {
      return theme.IsGlassTheme() || theme.IsTintedTheme();
    }
  }
}
