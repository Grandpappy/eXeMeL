using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace eXeMeL.Model
{
  public class AssociatedEmbeddedResourceAttribute : Attribute
  {
    public string HighlightResourceName { get; private set; }


    public AssociatedEmbeddedResourceAttribute(string highlightResourceName)
    {
      this.HighlightResourceName = highlightResourceName;
    }
  }



  public class AssociatedResourceDictionaryAttribute : Attribute
  {
    public string ResourceDictionaryPath { get; private set; }


    public AssociatedResourceDictionaryAttribute(string resourceDictionaryPath)
    {
      this.ResourceDictionaryPath = resourceDictionaryPath;
    }
  }



  [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
  public class AssociatedThemeBrushAttribute : Attribute
  {
    public Brush AssociatedBrush { get; private set; }
    public ApplicationTheme AssociatedTheme { get; private set; }



    public AssociatedThemeBrushAttribute(ApplicationTheme associatedTheme, Brush associatedBrush)
    {
      this.AssociatedBrush = associatedBrush;
      this.AssociatedTheme = associatedTheme;
    }



    public AssociatedThemeBrushAttribute(ApplicationTheme associatedTheme, string associatedBrush)
    {
      this.AssociatedBrush = (SolidColorBrush)new BrushConverter().ConvertFromString(associatedBrush);
      this.AssociatedTheme = associatedTheme;
    }
  }
}
