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
using GalaSoft.MvvmLight;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using MvvmFoundation.Wpf;

namespace eXeMeL.ViewModel
{
  public class SettingsWatcherBase : ViewModelBase
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
      set { Set(() => this.HighlightingDefinition, ref _HighlightingDefinition, value); }
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
      this.MessengerInstance.Send(new ApplicationThemeUpdatedMessage());
    }



    private void SetApplicationThemeBasedOnSettings()
    {
      //Debug.Assert(Application.Current.Resources.MergedDictionaries.Count <= 6, "There are more resource dictionaries than expected.");

      if (Application.Current.Resources.MergedDictionaries.Count == 6)
      {
        Application.Current.Resources.MergedDictionaries.RemoveAt(5);
      }

      var dict = new ResourceDictionary() { Source = new Uri(GetApplicationThemeResource(), UriKind.RelativeOrAbsolute) };
      Application.Current.Resources.MergedDictionaries.Add(dict);
    }



    private string GetApplicationThemeResource()
    {
      return this.Settings.ApplicationTheme.GetResourceDictionaryPath();
    }
  }
}
