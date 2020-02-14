using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace eXeMeL.ViewModel.XmlCleaners
{
  internal class NewLineCleaner : XmlCleanerBase
  {
    public override void CleanXml(XmlCleanerContext context)
    {
      context.XmlToClean = context.XmlToClean.Replace(Environment.NewLine, string.Empty);
    }
  }
}
