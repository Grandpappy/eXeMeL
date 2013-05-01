using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace eXeMeL.ViewModel.XmlCleaners
{
  internal class VisualStudioCleaner : XmlCleanerBase
  {

    public override void CleanXml(XmlCleanerContext context)
    {
      context.XmlToClean = context.XmlToClean.Replace("\\\"", "\"");
    }
  }


  internal class AddedRootCleaner : XmlCleanerBase
  {
    public override void CleanXml(XmlCleanerContext context)
    {
      if (context.ParsedXml != null)
        return;


      try
      {
        context.ParsedXml = XElement.Parse(context.XmlToClean);
        context.XmlToClean = context.ParsedXml.ToString(SaveOptions.None);
      }
      catch
      {
        try
        {
          context.ParsedXml = XElement.Parse(String.Format("<AddedRoot>{0}</AddedRoot>", context.XmlToClean));
          context.XmlToClean = context.ParsedXml.ToString(SaveOptions.None);
        }
        catch (Exception e)
        {
          context.ErrorMessage = "Unable to parse XML, even when surrounded with a root element.  " + e.Message;
        }
      }
    }
  }


  internal class NewLineCleaner : XmlCleanerBase
  {
    public override void CleanXml(XmlCleanerContext context)
    {
      context.XmlToClean = context.XmlToClean.Replace(Environment.NewLine, string.Empty);
    }
  }
}
