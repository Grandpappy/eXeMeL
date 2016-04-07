using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace eXeMeL.ViewModel.XmlCleaners
{
  internal class XmlCleanerContext
  {
    public string XmlToClean { get; set; }
    public XElement ParsedXml { get; set; }
    public string ErrorMessage { get; set; }
    public string OriginalXml { get; set; }
  }


  internal abstract class XmlCleanerBase
  {
    public abstract void CleanXml(XmlCleanerContext context);
  }
}
