using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eXeMeL.Utilities;


namespace eXeMeL.Model
{
  public enum SyntaxHighlightingStyle
  {
    [Description("Bright (Light Theme)")]
    [AssociatedEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.Bright.xshd")]
    [AssociatedThemeBrush(ApplicationTheme.Light, "#FF333333")]
    [AssociatedThemeBrush(ApplicationTheme.Dark, "#FFCCCCCC")]
    Light_Bright,

    [Description("Earthy (Light Theme)")]
    [AssociatedEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.Earthy.xshd")]
    [AssociatedThemeBrush(ApplicationTheme.Light, "#FF333333")]
    [AssociatedThemeBrush(ApplicationTheme.Dark, "#FFCCCCCC")]
    Light_Earthy,

    [Description("Ethereal (Dark Theme)")]
    [AssociatedEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.Dark.xshd")]
    [AssociatedThemeBrush(ApplicationTheme.Light, "#FF333333")]
    [AssociatedThemeBrush(ApplicationTheme.Dark, "#FFCCCCCC")]
    Dark_Ethereal,

    [Description("Blue (Dark Theme)")]
    [AssociatedEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.VSBlue.xshd")]
    [AssociatedThemeBrush(ApplicationTheme.Light, "#FF333333")]
    [AssociatedThemeBrush(ApplicationTheme.Dark, "#FF939393")]
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
