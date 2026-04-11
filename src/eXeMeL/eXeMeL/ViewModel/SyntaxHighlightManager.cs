using System.IO;
using System.Linq;
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
    private DocumentContentType _contentType = DocumentContentType.Xml;


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
      WeakReferenceMessenger.Default.Register<ContentTypeChangedMessage>(this, (r, m) => HandleContentTypeChanged(m));
    }



    private void HandleSettingChange(Settings settings)
    {
      this.HighlightingDefinition = GetSyntaxHighlighting();
    }



    private void HandleContentTypeChanged(ContentTypeChangedMessage message)
    {
      _contentType = message.ContentType;
      this.HighlightingDefinition = GetSyntaxHighlighting();
    }



    private IHighlightingDefinition GetSyntaxHighlighting()
    {
      var resourceName = _contentType == DocumentContentType.Json
        ? GetJsonSyntaxHighlightingResource()
        : GetXmlSyntaxHighlightingResource();

      using var stream = this.GetType().Assembly.GetManifestResourceStream(resourceName);
      using var reader = XmlReader.Create(stream);
      return HighlightingLoader.Load(reader, HighlightingManager.Instance);
    }



    private string GetXmlSyntaxHighlightingResource()
    {
      return this.Settings.SyntaxHighlightingStyle.GetResourceName();
    }



    private string GetJsonSyntaxHighlightingResource()
    {
      return this.Settings.SyntaxHighlightingStyle.GetJsonResourceName();
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
      var existingTheme = Application.Current.Resources.MergedDictionaries
          .FirstOrDefault(d => d.Contains("IsEXeMeLTheme"));
      if (existingTheme != null)
        Application.Current.Resources.MergedDictionaries.Remove(existingTheme);

      var dict = new ResourceDictionary() { Source = new System.Uri(GetApplicationThemeResource(), System.UriKind.RelativeOrAbsolute) };
      dict["IsEXeMeLTheme"] = true;
      Application.Current.Resources.MergedDictionaries.Add(dict);

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
