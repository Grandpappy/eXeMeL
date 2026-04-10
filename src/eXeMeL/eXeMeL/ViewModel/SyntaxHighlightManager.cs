using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using eXeMeL.Messages;
using eXeMeL.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using eXeMeL.Utilities;

namespace eXeMeL.ViewModel
{
  public class SettingsWatcherBase : ObservableObject
  {
    protected PropertyObserver<Settings> Observer { get; private set; }
    protected Settings Settings { get; private set; }



    public SettingsWatcherBase(Settings settings)
    {
      this.Settings = settings;
      this.Observer = new PropertyObserver<Settings>(this.Settings);
    }
  }



  public class SyntaxHighlightingManager : SettingsWatcherBase
  {

    private IHighlightingDefinition _HighlightingDefinition;



    public IHighlightingDefinition HighlightingDefinition
    {
      get { return _HighlightingDefinition; }
      set { SetProperty(ref _HighlightingDefinition, value); }
    }



    public SyntaxHighlightingManager(Settings settings)
      : base(settings)
    {
      this.HighlightingDefinition = GetSyntaxHighlighting();
      this.Observer.RegisterHandler(s => s.SyntaxHighlightingStyle, HandleSettingChange);
    }



    private void HandleSettingChange(Settings settings)
    {
      this.HighlightingDefinition = GetSyntaxHighlighting();
    }



    private IHighlightingDefinition GetSyntaxHighlighting()
    {
      var resourceName = GetSyntaxHighlightingResource();

      using (Stream stream = this.GetType().Assembly.GetManifestResourceStream(resourceName))
      {
        using (XmlTextReader reader = new XmlTextReader(stream))
        {
          return HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }
      }
    }



    private string GetSyntaxHighlightingResource()
    {
      return this.Settings.SyntaxHighlightingStyle.GetResourceName();
    }
  }



  public class ApplicationThemeManager : SettingsWatcherBase
  {

    public ApplicationThemeManager(Settings settings)
      : base(settings)
    {
      this.Observer.RegisterHandler(x => x.ApplicationTheme, HandleApplicationThemeChange);
      SetApplicationThemeBasedOnSettings();
    }



    private void HandleApplicationThemeChange(Settings settings)
    {
      SetApplicationThemeBasedOnSettings();
      WeakReferenceMessenger.Default.Send(new ApplicationThemeUpdatedMessage());
    }



    private void SetApplicationThemeBasedOnSettings()
    {
      // Remove the old theme dictionary (look for one with our marker key)
      var existingTheme = Application.Current.Resources.MergedDictionaries
          .FirstOrDefault(d => d.Contains("IsEXeMeLTheme"));
      if (existingTheme != null)
        Application.Current.Resources.MergedDictionaries.Remove(existingTheme);

      // Add the new theme dictionary (editor-specific brushes)
      var dict = new ResourceDictionary() { Source = new Uri(GetApplicationThemeResource(), UriKind.RelativeOrAbsolute) };
      dict["IsEXeMeLTheme"] = true; // marker
      Application.Current.Resources.MergedDictionaries.Add(dict);

      // Sync WPF-UI theme system for Fluent control styling
      var wpfUiTheme = this.Settings.ApplicationTheme switch
      {
        Model.ApplicationTheme.Light => Wpf.Ui.Appearance.ApplicationTheme.Light,
        Model.ApplicationTheme.Dark => Wpf.Ui.Appearance.ApplicationTheme.Dark,
        Model.ApplicationTheme.SolarizedDark => Wpf.Ui.Appearance.ApplicationTheme.Dark,
        _ => Wpf.Ui.Appearance.ApplicationTheme.Light
      };
      Wpf.Ui.Appearance.ApplicationThemeManager.Apply(wpfUiTheme);
    }



    private string GetApplicationThemeResource()
    {
      return this.Settings.ApplicationTheme.GetResourceDictionaryPath();
    }
  }
}
