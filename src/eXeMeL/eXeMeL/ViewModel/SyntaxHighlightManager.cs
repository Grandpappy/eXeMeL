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
      this.Observer.RegisterHandler(x => x.EditorTintIntensity, HandleEditorTintIntensityChange);
      this.Observer.RegisterHandler(x => x.ChromeOpacity, HandleChromeTintColorChange);
      this.Observer.RegisterHandler(x => x.TextColor, HandleTextOrAccentColorChange);
      this.Observer.RegisterHandler(x => x.AccentColor, HandleTextOrAccentColorChange);
      this.Observer.RegisterHandler(x => x.EditorBackgroundColor, HandleEditorBackgroundColorChange);
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

      try
      {
        var textBrush = new System.Windows.Media.SolidColorBrush(
          (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(settings.TextColor));
        textBrush.Freeze();
        existingDict["AppTextBrush"] = textBrush;

        var accentBrush = new System.Windows.Media.SolidColorBrush(
          (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(settings.AccentColor));
        accentBrush.Freeze();
        existingDict["AppAccentBrush"] = accentBrush;
      }
      catch { }
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

        // Editor tint: skip if user has a custom editor background color
        if (string.IsNullOrEmpty(settings.EditorBackgroundColor))
        {
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
      }
      catch { /* Invalid color */ }
    }



    private static void HandleEditorTintIntensityChange(Settings settings)
    {
      // Intensity affects both linked (tint-derived) and unlinked (custom color) editor backgrounds
      if (!string.IsNullOrEmpty(settings.EditorBackgroundColor))
        HandleEditorBackgroundColorChange(settings);
      else
        HandleChromeTintColorChange(settings);
    }

    private static void HandleEditorBackgroundColorChange(Settings settings)
    {
      if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
      {
        Application.Current.Dispatcher.Invoke(() => HandleEditorBackgroundColorChange(settings));
        return;
      }

      var existingDict = Application.Current?.Resources.MergedDictionaries
          .FirstOrDefault(d => d.Contains("IsEXeMeLTheme"));
      if (existingDict == null) return;

      if (!string.IsNullOrEmpty(settings.EditorBackgroundColor))
      {
        // Custom color with editor opacity applied
        try
        {
          var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(settings.EditorBackgroundColor);
          color.A = (byte)(settings.EditorTintIntensity * 255);
          var brush = new System.Windows.Media.SolidColorBrush(color);
          brush.Freeze();
          existingDict["EditorTintOverlayBrush"] = brush;
        }
        catch { }
      }
      else
      {
        // Re-linked: revert to theme-derived brush
        if (settings.ApplicationTheme.SupportsTint())
          HandleChromeTintColorChange(settings);
        else
        {
          var defaultColor = GetDefaultEditorBrushColorForTheme(settings.ApplicationTheme);
          var brush = new System.Windows.Media.SolidColorBrush(defaultColor);
          brush.Freeze();
          existingDict["EditorTintOverlayBrush"] = brush;
        }
      }
    }

    public static string GetCurrentDerivedEditorColor(Settings settings)
    {
      if (settings.ApplicationTheme.SupportsTint())
      {
        try
        {
          var baseColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(settings.ChromeTintColor);
          var r = (byte)(baseColor.R * 0.4);
          var g = (byte)(baseColor.G * 0.4);
          var b = (byte)(baseColor.B * 0.4);
          return $"#{r:X2}{g:X2}{b:X2}";
        }
        catch { return "#252525"; }
      }

      var c = GetDefaultEditorBrushColorForTheme(settings.ApplicationTheme);
      return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
    }

    private static System.Windows.Media.Color GetDefaultEditorBrushColorForTheme(Model.ApplicationTheme theme)
    {
      return theme switch
      {
        Model.ApplicationTheme.Light => (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#EDEDED"),
        Model.ApplicationTheme.SolarizedDark => (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#002B36"),
        _ => (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#252525")
      };
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

          // Editor tint: skip if user has a custom editor background color
          if (string.IsNullOrEmpty(this.Settings.EditorBackgroundColor))
          {
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
        }
        catch { /* Invalid color string — skip tint */ }
      }

      // Custom editor background overrides both tinted and non-tinted themes
      if (!string.IsNullOrEmpty(this.Settings.EditorBackgroundColor))
      {
        try
        {
          var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(this.Settings.EditorBackgroundColor);
          color.A = (byte)(this.Settings.EditorTintIntensity * 255);
          var brush = new System.Windows.Media.SolidColorBrush(color);
          brush.Freeze();
          dict["EditorTintOverlayBrush"] = brush;
        }
        catch { }
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

      // Apply user text and accent colors
      try
      {
        var textBrush = new System.Windows.Media.SolidColorBrush(
          (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(this.Settings.TextColor));
        textBrush.Freeze();
        dict["AppTextBrush"] = textBrush;

        var accentBrush = new System.Windows.Media.SolidColorBrush(
          (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(this.Settings.AccentColor));
        accentBrush.Freeze();
        dict["AppAccentBrush"] = accentBrush;
      }
      catch { }
    }



    private string GetApplicationThemeResource()
    {
      return this.Settings.ApplicationTheme.GetResourceDictionaryPath();
    }
  }
}
