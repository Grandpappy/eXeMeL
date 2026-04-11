using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using eXeMeL.Utilities;

namespace eXeMeL.Model
{
  public class Settings : ObservableObject
  {
    public const double DefaultEditorFontSize = 16;

    private SyntaxHighlightingStyle _syntaxHighlightingStyle;
    private ApplicationTheme _applicationTheme;
    private bool _wrapEditorText;
    private bool _showEditorLineNumbers;
    private double _editorFontSize;
    private string _fontFamily;
    private bool _highlightOtherInstancesOfSelection;
    private string _chromeTintColor = "#3366CC";
    private double _editorTintIntensity = 0.3;
    private double _chromeOpacity = 0.5;
    private string _lastLaunchedVersion;
    private double _windowLeft = double.NaN;
    private double _windowTop = double.NaN;
    private double _windowWidth = 800;
    private double _windowHeight = 600;
    private int _windowState;

    private Brush _editorBrush;
    private Brush _elementBrush;
    private Brush _attributeNameBrush;
    private Brush _attributeValueBrush;
    private Brush _hoverBackgroundBrush;
    private Brush _currentXPathTargetBrush;
    private Brush _currentXPathStartBrush;


    public string LastLaunchedVersion
    {
      get => _lastLaunchedVersion;
      set => SetProperty(ref _lastLaunchedVersion, value);
    }

    public bool ShowEditorLineNumbers
    {
      get => _showEditorLineNumbers;
      set => SetProperty(ref _showEditorLineNumbers, value);
    }

    public bool WrapEditorText
    {
      get => _wrapEditorText;
      set => SetProperty(ref _wrapEditorText, value);
    }

    public double EditorFontSize
    {
      get => _editorFontSize;
      set => SetProperty(ref _editorFontSize, value);
    }

    public SyntaxHighlightingStyle SyntaxHighlightingStyle
    {
      get => _syntaxHighlightingStyle;
      set
      {
        if (SetProperty(ref _syntaxHighlightingStyle, value))
          UpdateBrushes();
      }
    }

    public ApplicationTheme ApplicationTheme
    {
      get => _applicationTheme;
      set
      {
        if (SetProperty(ref _applicationTheme, value))
          UpdateBrushes();
      }
    }

    public string FontFamily
    {
      get => _fontFamily;
      set => SetProperty(ref _fontFamily, value);
    }

    public bool HighlightOtherInstancesOfSelection
    {
      get => _highlightOtherInstancesOfSelection;
      set => SetProperty(ref _highlightOtherInstancesOfSelection, value);
    }

    public string ChromeTintColor
    {
      get => _chromeTintColor;
      set => SetProperty(ref _chromeTintColor, value);
    }

    public double EditorTintIntensity
    {
      get => _editorTintIntensity;
      set => SetProperty(ref _editorTintIntensity, value);
    }

    public double ChromeOpacity
    {
      get => _chromeOpacity;
      set => SetProperty(ref _chromeOpacity, value);
    }

    public double WindowLeft
    {
      get => _windowLeft;
      set => SetProperty(ref _windowLeft, value);
    }

    public double WindowTop
    {
      get => _windowTop;
      set => SetProperty(ref _windowTop, value);
    }

    public double WindowWidth
    {
      get => _windowWidth;
      set => SetProperty(ref _windowWidth, value);
    }

    public double WindowHeight
    {
      get => _windowHeight;
      set => SetProperty(ref _windowHeight, value);
    }

    public int WindowState
    {
      get => _windowState;
      set => SetProperty(ref _windowState, value);
    }


    [JsonIgnore]
    public Brush EditorBrush
    {
      get => _editorBrush;
      set => SetProperty(ref _editorBrush, value);
    }

    [JsonIgnore]
    public Brush ElementBrush
    {
      get => _elementBrush;
      set => SetProperty(ref _elementBrush, value);
    }

    [JsonIgnore]
    public Brush AttributeNameBrush
    {
      get => _attributeNameBrush;
      set => SetProperty(ref _attributeNameBrush, value);
    }

    [JsonIgnore]
    public Brush AttributeValueBrush
    {
      get => _attributeValueBrush;
      set => SetProperty(ref _attributeValueBrush, value);
    }

    [JsonIgnore]
    public Brush HoverBackgroundBrush
    {
      get => _hoverBackgroundBrush;
      set => SetProperty(ref _hoverBackgroundBrush, value);
    }

    [JsonIgnore]
    public Brush CurrentXPathTargetBrush
    {
      get => _currentXPathTargetBrush;
      set => SetProperty(ref _currentXPathTargetBrush, value);
    }

    [JsonIgnore]
    public Brush CurrentXPathStartBrush
    {
      get => _currentXPathStartBrush;
      set => SetProperty(ref _currentXPathStartBrush, value);
    }


    public Settings()
    {
      ShowEditorLineNumbers = true;
      WrapEditorText = true;
      EditorFontSize = DefaultEditorFontSize;
      SyntaxHighlightingStyle = SyntaxHighlightingStyle.Light_Earthy;
      ApplicationTheme = ApplicationTheme.Light;
      FontFamily = "Consolas";
    }


    private void UpdateBrushes()
    {
      EditorBrush = GetBrushForCurrentTheme(ThemeBrushTarget.EditorContent);
      ElementBrush = GetBrushForCurrentTheme(ThemeBrushTarget.Element);
      AttributeNameBrush = GetBrushForCurrentTheme(ThemeBrushTarget.AttributeName);
      AttributeValueBrush = GetBrushForCurrentTheme(ThemeBrushTarget.AttributeValue);
      HoverBackgroundBrush = GetBrushForCurrentTheme(ThemeBrushTarget.HoverBackground);
      CurrentXPathTargetBrush = GetBrushForCurrentTheme(ThemeBrushTarget.CurrentXPathTarget);
      CurrentXPathStartBrush = GetBrushForCurrentTheme(ThemeBrushTarget.CurrentXPathStart);
    }


    private Brush GetBrushForCurrentTheme(ThemeBrushTarget target)
    {
      var attribute = SyntaxHighlightingStyle.GetAttributes<AssociatedThemeBrushAttribute>()
         .FirstOrDefault(x => (x.AssociatedTheme == ApplicationTheme || x.AssociatedTheme == ApplicationTheme.Any)
                              && x.Target == target);

      if (attribute?.AssociatedBrush != null)
        return attribute.AssociatedBrush;

      var fallback = new SolidColorBrush(Colors.Red);
      fallback.Freeze();
      return fallback;
    }
  }
}
