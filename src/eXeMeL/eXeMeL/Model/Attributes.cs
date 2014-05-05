using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
