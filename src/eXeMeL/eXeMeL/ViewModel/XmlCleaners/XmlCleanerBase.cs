using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eXeMeL.ViewModel.XmlCleaners
{
  internal class XmlCleanerContext
  {
    public string XmlToClean { get; set; }
  }


  internal abstract class XmlCleanerBase
  {
    public abstract void CleanXml(XmlCleanerContext context);
  }
}
