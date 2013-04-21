using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eXeMeL.ViewModel.XmlCleaners
{
  internal class TrimCleaner : XmlCleanerBase
  {
    public override void CleanXml(XmlCleanerContext context)
    {
      context.XmlToClean = context.XmlToClean.Trim();
    }
  }
}
