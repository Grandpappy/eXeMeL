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
      // Text mode: no syntax highlighting
      if (_contentType == DocumentContentType.Text)
        return null;

      try
      {
        var resourceName = _contentType switch
        {
          DocumentContentType.Json => GetJsonSyntaxHighlightingResource(),
          DocumentContentType.Yaml => GetYamlSyntaxHighlightingResource(),
          DocumentContentType.Markdown => GetMarkdownSyntaxHighlightingResource(),
          _ => GetXmlSyntaxHighlightingResource()
        };

        using var stream = this.GetType().Assembly.GetManifestResourceStream(resourceName);
        if (stream == null) return null;
        using var reader = XmlReader.Create(stream);
        return HighlightingLoader.Load(reader, HighlightingManager.Instance);
      }
      catch
      {
        // If highlighting fails to load, fall back to no highlighting
        return null;
      }
    }

    private string GetMarkdownSyntaxHighlightingResource()
    {
      // Pick dark or light based on application theme
      var isLight = this.Settings.ApplicationTheme == Model.ApplicationTheme.Light
                    || (this.Settings.ApplicationTheme.SupportsTint()
                        && ApplicationThemeExtensions.IsLightColor(this.Settings.ChromeTintColor));
      return isLight
        ? "eXeMeL.Assets.SyntaxHighlightingSchemes.MarkdownLight.xshd"
        : "eXeMeL.Assets.SyntaxHighlightingSchemes.MarkdownDark.xshd";
    }



    private string GetXmlSyntaxHighlightingResource()
    {
      return this.Settings.SyntaxHighlightingStyle.GetResourceName();
    }



    private string GetYamlSyntaxHighlightingResource()
    {
      return this.Settings.SyntaxHighlightingStyle.GetYamlResourceName();
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
      this.Observer.RegisterHandler(x => x.ChromeTintColor, HandleChromeTintColorChange);
      this.Observer.RegisterHandler(x => x.EditorTintIntensity, HandleChromeTintColorChange);
      this.Observer.RegisterHandler(x => x.ChromeOpacity, HandleChromeTintColorChange);
      this.Observer.RegisterHandler(x => x.TextColor, HandleTextOrAccentColorChange);
      this.Observer.RegisterHandler(x => x.AccentColor, HandleTextOrAccentColorChange);
      SetApplicationThemeBasedOnSettings();
    }



    private void HandleApplicationThemeChange(Settings settings)
    {
      SetApplicationThemeBasedOnSettings();
      WeakReferenceMessenger.Default.Send(new ApplicationThemeUpdatedMessage());
    }

    private static void HandleTextOrAccentColorChange(Settings settings)
    {
      if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
      {
        Application.Current.Dispatcher.Invoke(() => HandleTextOrAccentColorChange(settings));
        return;
      }

      var existingDict = Application.Current?.Resources.MergedDictionaries
          .FirstOrDefault(d => d.Contains("IsEXeMeLTheme"));
      if (existingDict == null) return;

      ApplyUserColors(existingDict, settings);
    }

    private static void ApplyUserColors(ResourceDictionary dict, Settings settings)
    {
      // Only override theme defaults when the user has explicitly chosen a color.
      // Null means "not set" (e.g. migrated from an older version without this setting),
      // so we let the theme's built-in AppTextBrush provide a readable default.
      try
      {
        if (!string.IsNullOrEmpty(settings.TextColor))
        {
          var textBrush = new System.Windows.Media.SolidColorBrush(
            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(settings.TextColor));
          textBrush.Freeze();
          dict["AppTextBrush"] = textBrush;
        }

        if (!string.IsNullOrEmpty(settings.AccentColor))
        {
          var accentBrush = new System.Windows.Media.SolidColorBrush(
            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(settings.AccentColor));
          accentBrush.Freeze();
          dict["AppAccentBrush"] = accentBrush;
        }
      }
      catch { /* Invalid color string — leave theme defaults */ }
    }

    private static void HandleChromeTintColorChange(Settings settings)
    {
      if (!settings.ApplicationTheme.SupportsTint())
        return;

      if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
      {
        Application.Current.Dispatcher.Invoke(() => HandleChromeTintColorChange(settings));
        return;
      }

      // Update tint brushes in-place without reloading the entire theme dictionary
      var existingDict = Application.Current?.Resources.MergedDictionaries
          .FirstOrDefault(d => d.Contains("IsEXeMeLTheme"));
      if (existingDict == null) return;

      try
      {
        var tintColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(settings.ChromeTintColor);
        var chromeAlpha = (byte)(settings.ChromeOpacity * 255);
        tintColor.A = chromeAlpha;
        var chromeBrush = new System.Windows.Media.SolidColorBrush(tintColor);
        chromeBrush.Freeze();
        existingDict["ChromeTintOverlayBrush"] = chromeBrush;

        // Editor tint: darker than chrome, controlled by intensity
        var editorAlpha = (byte)(settings.EditorTintIntensity * 255);
        var editorTint = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(settings.ChromeTintColor);
        editorTint.R = (byte)(editorTint.R * 0.4);
        editorTint.G = (byte)(editorTint.G * 0.4);
        editorTint.B = (byte)(editorTint.B * 0.4);
        editorTint.A = editorAlpha;
        var editorBrush = new System.Windows.Media.SolidColorBrush(editorTint);
        editorBrush.Freeze();
        existingDict["EditorTintOverlayBrush"] = editorBrush;
      }
      catch { /* Invalid color */ }
    }



    private void SetApplicationThemeBasedOnSettings()
    {
      // Ensure we're on the UI thread for ResourceDictionary operations
      if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
      {
        Application.Current.Dispatcher.Invoke(SetApplicationThemeBasedOnSettings);
        return;
      }

      if (Application.Current == null) return;

      var existingTheme = Application.Current.Resources.MergedDictionaries
          .FirstOrDefault(d => d.Contains("IsEXeMeLTheme"));
      if (existingTheme != null)
        Application.Current.Resources.MergedDictionaries.Remove(existingTheme);

      var dict = new ResourceDictionary() { Source = new System.Uri(GetApplicationThemeResource(), System.UriKind.RelativeOrAbsolute) };
      dict["IsEXeMeLTheme"] = true;
      Application.Current.Resources.MergedDictionaries.Add(dict);

      // Apply tint color for glass/tinted themes
      if (this.Settings.ApplicationTheme.SupportsTint())
      {
        try
        {
          var baseColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(this.Settings.ChromeTintColor);

          // Chrome tint — alpha controlled by ChromeOpacity slider
          var chromeAlpha = (byte)(this.Settings.ChromeOpacity * 255);
          var chromeTint = baseColor;
          chromeTint.A = chromeAlpha;
          var chromeBrush = new System.Windows.Media.SolidColorBrush(chromeTint);
          chromeBrush.Freeze();
          dict["ChromeTintOverlayBrush"] = chromeBrush;

          // Editor tint: darker than chrome, controlled by intensity slider
          var editorAlpha = (byte)(this.Settings.EditorTintIntensity * 255);
          var editorTint = baseColor;
          editorTint.R = (byte)(editorTint.R * 0.4);
          editorTint.G = (byte)(editorTint.G * 0.4);
          editorTint.B = (byte)(editorTint.B * 0.4);
          editorTint.A = editorAlpha;
          var editorBrush = new System.Windows.Media.SolidColorBrush(editorTint);
          editorBrush.Freeze();
          dict["EditorTintOverlayBrush"] = editorBrush;
        }
        catch { /* Invalid color string — skip tint */ }
      }

      Wpf.Ui.Appearance.ApplicationTheme wpfUiTheme;
      if (this.Settings.ApplicationTheme.SupportsTint())
      {
        // For Glass/Tinted: determine light/dark from the tint color luminance
        wpfUiTheme = ApplicationThemeExtensions.IsLightColor(this.Settings.ChromeTintColor)
          ? Wpf.Ui.Appearance.ApplicationTheme.Light
          : Wpf.Ui.Appearance.ApplicationTheme.Dark;
      }
      else
      {
        wpfUiTheme = this.Settings.ApplicationTheme switch
        {
          Model.ApplicationTheme.Light => Wpf.Ui.Appearance.ApplicationTheme.Light,
          _ => Wpf.Ui.Appearance.ApplicationTheme.Dark
        };
      }
      Wpf.Ui.Appearance.ApplicationThemeManager.Apply(wpfUiTheme);

      ApplyUserColors(dict, this.Settings);
    }



    private string GetApplicationThemeResource()
    {
      return this.Settings.ApplicationTheme.GetResourceDictionaryPath();
    }
  }
}
