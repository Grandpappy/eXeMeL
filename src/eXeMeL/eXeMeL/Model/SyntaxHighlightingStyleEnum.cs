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
    Light_Bright,

    [Description("Earthy (Light Theme)")]
    [AssociatedEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.Earthy.xshd")]
    [AssociatedThemeBrush(ApplicationTheme.Light, ThemeBrushTarget.EditorContent, "#FF333333")]
    [AssociatedThemeBrush(ApplicationTheme.Dark, ThemeBrushTarget.EditorContent, "#FFCCCCCC")]
    Light_Earthy,

    [Description("Ethereal (Dark Theme)")]
    [AssociatedEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.Dark.xshd")]
    [AssociatedThemeBrush(ApplicationTheme.Light, ThemeBrushTarget.EditorContent, "#FF333333")]
    [AssociatedThemeBrush(ApplicationTheme.Dark, ThemeBrushTarget.EditorContent, "#FFCCCCCC")]
    Dark_Ethereal,

    [Description("Blue (Dark Theme)")]
    [AssociatedEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.VSBlue.xshd")]
    [AssociatedThemeBrush(ApplicationTheme.Light, ThemeBrushTarget.EditorContent, "#FF333333")]
    [AssociatedThemeBrush(ApplicationTheme.Dark, ThemeBrushTarget.EditorContent, "#FF939393")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.Element, "#FFD6D6D6")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeName, "#FF92CAF4")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeValue, "#FF569CD6")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.HoverBackground, "#66666666")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathTarget, "#66999999")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathStart, "#66B99400")]
    Dark_Blue
  }



  public static class SyntaxHighlightingStyleEnumExtensions
  {
    public static string GetResourceName(this SyntaxHighlightingStyle style)
    {
      return style.GetAttributeValue<AssociatedEmbeddedResourceAttribute, string>(x => x.HighlightResourceName);
    }
  }
}
