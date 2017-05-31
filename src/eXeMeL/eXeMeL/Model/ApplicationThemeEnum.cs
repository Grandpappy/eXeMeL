using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    [DoNotDisplayInSettings]
    Any
  }



  public static class ApplicationThemeExtensions
  {
    public static string GetResourceDictionaryPath(this ApplicationTheme theme)
    {
      return theme.GetAttributeValue<AssociatedResourceDictionaryAttribute, string>(x => x.ResourceDictionaryPath);
    }
  }

}
