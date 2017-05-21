using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eXeMeL.Utilities;


namespace eXeMeL.Model
{
  public enum ThemeBrushTarget
  {
    EditorContent,
    Element,
    AttributeName,
    AttributeValue,
    HoverBackground,
    CurrentXPathTarget,
    CurrentXPathStart
  }


  public enum SyntaxHighlightingStyle
  {
    [Description("Bright (Light Theme)")]
    [AssociatedEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.Bright.xshd")]
    [AssociatedThemeBrush(ApplicationTheme.Light, ThemeBrushTarget.EditorContent, "#FF333333")]
    [AssociatedThemeBrush(ApplicationTheme.Dark, ThemeBrushTarget.EditorContent, "#FFCCCCCC")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.Element, "DarkMagenta")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeName, "Red")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeValue, "Blue")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.HoverBackground, "#33666666")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathTarget, "#AAB99400")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathStart, "#66B99400")]
    Light_Bright,

    [Description("Earthy (Light Theme)")]
    [AssociatedEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.Earthy.xshd")]
    [AssociatedThemeBrush(ApplicationTheme.Light, ThemeBrushTarget.EditorContent, "#FF333333")]
    [AssociatedThemeBrush(ApplicationTheme.Dark, ThemeBrushTarget.EditorContent, "#FFCCCCCC")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.Element, "#BA2F2F")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeName, "#DE7800")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeValue, "Teal")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.HoverBackground, "#33666666")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathTarget, "#AAB99400")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathStart, "#66B99400")]
    Light_Earthy,

    [Description("Ethereal (Dark Theme)")]
    [AssociatedEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.Dark.xshd")]
    [AssociatedThemeBrush(ApplicationTheme.Light, ThemeBrushTarget.EditorContent, "#FF333333")]
    [AssociatedThemeBrush(ApplicationTheme.Dark, ThemeBrushTarget.EditorContent, "#FFCCCCCC")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.Element, "#73B2C4")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeName, "#C6C7AB")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeValue, "#9DCFA9")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.HoverBackground, "#66666666")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathTarget, "#AAB99400")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathStart, "#66B99400")]
    Dark_Ethereal,

    [Description("Blue (Dark Theme)")]
    [AssociatedEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.VSBlue.xshd")]
    [AssociatedThemeBrush(ApplicationTheme.Light, ThemeBrushTarget.EditorContent, "#FF333333")]
    [AssociatedThemeBrush(ApplicationTheme.Dark, ThemeBrushTarget.EditorContent, "#FF939393")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.Element, "#FFD6D6D6")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeName, "#FF92CAF4")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeValue, "#FF569CD6")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.HoverBackground, "#66666666")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathTarget, "#AAB99400")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathStart, "#66B99400")]
    Dark_Blue,

    [Description("Solarized (Dark Theme)")]
    [AssociatedEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.SolarizedDark.xshd")]
    [AssociatedThemeBrush(ApplicationTheme.Light, ThemeBrushTarget.EditorContent, "#FF333333")]
    [AssociatedThemeBrush(ApplicationTheme.Dark, ThemeBrushTarget.EditorContent, "#FFCCCCCC")]
    [AssociatedThemeBrush(ApplicationTheme.SolarizedDark, ThemeBrushTarget.EditorContent, "#93a1a1")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.Element, "#268bd2")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeName, "#b58900")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeValue, "#2aa198")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.HoverBackground, "#66666666")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathTarget, "#2c4c55")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathStart, "#66B99400")]
    Dark_Solarized
  }



  public static class SyntaxHighlightingStyleEnumExtensions
  {
    public static string GetResourceName(this SyntaxHighlightingStyle style)
    {
      return style.GetAttributeValue<AssociatedEmbeddedResourceAttribute, string>(x => x.HighlightResourceName);
    }
  }
}
