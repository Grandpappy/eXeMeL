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
    Light_Bright,

    [Description("Earthy (Light Theme)")]
    [AssociatedEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.Earthy.xshd")]
    Light_Earthy,

    [Description("Ethereal (Dark Theme)")]
    [AssociatedEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.Dark.xshd")]
    Dark_Ethereal,

    [Description("Blue (Dark Theme)")]
    [AssociatedEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.VSBlue.xshd")]
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
