using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace eXeMeL.ViewModel.XmlCleaners
{
  internal class SurroundingGarbageCleaner : XmlCleanerBase
  {
    public override void CleanXml(XmlCleanerContext context)
    {
      var firstLessThanIndex = context.XmlToClean.IndexOf('<');
      if (firstLessThanIndex < 0)
        return;

      var lastGreaterThanIndex = context.XmlToClean.LastIndexOf('>');
      if (lastGreaterThanIndex < 0)
        return;

      context.XmlToClean = context.XmlToClean.Substring(firstLessThanIndex, lastGreaterThanIndex - firstLessThanIndex + 1);
    }
  }
}
