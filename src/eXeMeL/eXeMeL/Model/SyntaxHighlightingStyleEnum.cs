using System.ComponentModel;
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
    [AssociatedJsonEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.JsonBright.xshd")]
    [AssociatedYamlEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.YamlBright.xshd")]
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
    [AssociatedJsonEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.JsonEarthy.xshd")]
    [AssociatedYamlEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.YamlEarthy.xshd")]
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
    [AssociatedJsonEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.JsonDark.xshd")]
    [AssociatedYamlEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.YamlDark.xshd")]
    [AssociatedThemeBrush(ApplicationTheme.Light, ThemeBrushTarget.EditorContent, "#FF333333")]
    [AssociatedThemeBrush(ApplicationTheme.Dark, ThemeBrushTarget.EditorContent, "#FFDDDDDD")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.Element, "#73B2C4")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeName, "#C6C7AB")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeValue, "#9DCFA9")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.HoverBackground, "#66666666")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathTarget, "#AAB99400")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathStart, "#66B99400")]
    Dark_Ethereal,

    [Description("Blue (Dark Theme)")]
    [AssociatedEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.VSBlue.xshd")]
    [AssociatedJsonEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.JsonVSBlue.xshd")]
    [AssociatedYamlEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.YamlVSBlue.xshd")]
    [AssociatedThemeBrush(ApplicationTheme.Light, ThemeBrushTarget.EditorContent, "#FF333333")]
    [AssociatedThemeBrush(ApplicationTheme.Dark, ThemeBrushTarget.EditorContent, "#FFD4D4D4")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.Element, "#FF569CD6")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeName, "#FF9CDCFE")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeValue, "#FFCE9178")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.HoverBackground, "#66666666")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathTarget, "#AAB99400")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathStart, "#66B99400")]
    Dark_Blue,

    [Description("Solarized (Dark Theme)")]
    [AssociatedEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.SolarizedDark.xshd")]
    [AssociatedJsonEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.JsonSolarizedDark.xshd")]
    [AssociatedYamlEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.YamlSolarizedDark.xshd")]
    [AssociatedThemeBrush(ApplicationTheme.Light, ThemeBrushTarget.EditorContent, "#FF333333")]
    [AssociatedThemeBrush(ApplicationTheme.Dark, ThemeBrushTarget.EditorContent, "#FFCCCCCC")]
    [AssociatedThemeBrush(ApplicationTheme.SolarizedDark, ThemeBrushTarget.EditorContent, "#93a1a1")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.Element, "#268bd2")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeName, "#b58900")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeValue, "#2aa198")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.HoverBackground, "#66666666")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathTarget, "#2c4c55")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathStart, "#66B99400")]
    Dark_Solarized,

    [Description("VS Code Dark+ (Dark Theme)")]
    [AssociatedEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.VSCodeDarkPlusXml.xshd")]
    [AssociatedJsonEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.VSCodeDarkPlusJson.xshd")]
    [AssociatedYamlEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.VSCodeDarkPlusYaml.xshd")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.EditorContent, "#FFD4D4D4")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.Element, "#FF569CD6")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeName, "#FF9CDCFE")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeValue, "#FFCE9178")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.HoverBackground, "#66666666")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathTarget, "#AAB99400")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathStart, "#66B99400")]
    VSCode_DarkPlus,

    [Description("VS Code Light+ (Light Theme)")]
    [AssociatedEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.VSCodeLightPlusXml.xshd")]
    [AssociatedJsonEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.VSCodeLightPlusJson.xshd")]
    [AssociatedYamlEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.VSCodeLightPlusYaml.xshd")]
    [AssociatedThemeBrush(ApplicationTheme.Light, ThemeBrushTarget.EditorContent, "#FF000000")]
    [AssociatedThemeBrush(ApplicationTheme.Dark, ThemeBrushTarget.EditorContent, "#FFCCCCCC")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.Element, "#FF800000")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeName, "#FFFF0000")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeValue, "#FF0451A5")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.HoverBackground, "#33666666")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathTarget, "#AAB99400")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathStart, "#66B99400")]
    VSCode_LightPlus,

    [Description("Monokai (Dark Theme)")]
    [AssociatedEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.MonokaiXml.xshd")]
    [AssociatedJsonEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.MonokaiJson.xshd")]
    [AssociatedYamlEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.MonokaiYaml.xshd")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.EditorContent, "#FFF8F8F2")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.Element, "#FF66D9EF")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeName, "#FFA1EFE4")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeValue, "#FFE6DB74")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.HoverBackground, "#66666666")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathTarget, "#AAB99400")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathStart, "#66B99400")]
    Monokai,

    [Description("Dracula (Dark Theme)")]
    [AssociatedEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.DraculaXml.xshd")]
    [AssociatedJsonEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.DraculaJson.xshd")]
    [AssociatedYamlEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.DraculaYaml.xshd")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.EditorContent, "#FFF8F8F2")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.Element, "#FF8BE9FD")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeName, "#FF8BE9FD")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeValue, "#FFF1FA8C")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.HoverBackground, "#66666666")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathTarget, "#AAB99400")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathStart, "#66B99400")]
    Dracula,

    [Description("One Dark Pro (Dark Theme)")]
    [AssociatedEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.OneDarkProXml.xshd")]
    [AssociatedJsonEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.OneDarkProJson.xshd")]
    [AssociatedYamlEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.OneDarkProYaml.xshd")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.EditorContent, "#FFABB2BF")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.Element, "#FF61AFEF")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeName, "#FF56B6C2")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeValue, "#FF98C379")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.HoverBackground, "#66666666")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathTarget, "#AAB99400")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathStart, "#66B99400")]
    OneDarkPro,

    [Description("GitHub Dark (Dark Theme)")]
    [AssociatedEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.GitHubDarkXml.xshd")]
    [AssociatedJsonEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.GitHubDarkJson.xshd")]
    [AssociatedYamlEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.GitHubDarkYaml.xshd")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.EditorContent, "#FFC9D1D9")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.Element, "#FF79C0FF")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeName, "#FF79C0FF")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeValue, "#FFA5D6FF")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.HoverBackground, "#66666666")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathTarget, "#AAB99400")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathStart, "#66B99400")]
    GitHubDark,

    [Description("GitHub Light (Light Theme)")]
    [AssociatedEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.GitHubLightXml.xshd")]
    [AssociatedJsonEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.GitHubLightJson.xshd")]
    [AssociatedYamlEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.GitHubLightYaml.xshd")]
    [AssociatedThemeBrush(ApplicationTheme.Light, ThemeBrushTarget.EditorContent, "#FF24292F")]
    [AssociatedThemeBrush(ApplicationTheme.Dark, ThemeBrushTarget.EditorContent, "#FFCCCCCC")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.Element, "#FF24292F")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeName, "#FF24292F")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeValue, "#FF0550AE")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.HoverBackground, "#33666666")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathTarget, "#AAB99400")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathStart, "#66B99400")]
    GitHubLight,

    [Description("Solarized Light (Light Theme)")]
    [AssociatedEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.SolarizedLightXml.xshd")]
    [AssociatedJsonEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.SolarizedLightJson.xshd")]
    [AssociatedYamlEmbeddedResource("eXeMeL.Assets.SyntaxHighlightingSchemes.SolarizedLightYaml.xshd")]
    [AssociatedThemeBrush(ApplicationTheme.Light, ThemeBrushTarget.EditorContent, "#FF657B83")]
    [AssociatedThemeBrush(ApplicationTheme.Dark, ThemeBrushTarget.EditorContent, "#FFCCCCCC")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.Element, "#FF268BD2")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeName, "#FF268BD2")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.AttributeValue, "#FF2AA198")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.HoverBackground, "#33666666")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathTarget, "#AAB99400")]
    [AssociatedThemeBrush(ApplicationTheme.Any, ThemeBrushTarget.CurrentXPathStart, "#66B99400")]
    SolarizedLight
  }



  public static class SyntaxHighlightingStyleEnumExtensions
  {
    public static string GetResourceName(this SyntaxHighlightingStyle style)
    {
      return style.GetAttributeValue<AssociatedEmbeddedResourceAttribute, string>(x => x.HighlightResourceName);
    }

    public static string GetJsonResourceName(this SyntaxHighlightingStyle style)
    {
      return style.GetAttributeValue<AssociatedJsonEmbeddedResourceAttribute, string>(x => x.HighlightResourceName);
    }

    public static string GetYamlResourceName(this SyntaxHighlightingStyle style)
    {
      return style.GetAttributeValue<AssociatedYamlEmbeddedResourceAttribute, string>(x => x.HighlightResourceName);
    }
  }
}
