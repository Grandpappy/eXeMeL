using System.Web;

namespace eXeMeL.ViewModel.XmlCleaners
{
  internal class UrlEncodingCleaner : XmlCleanerBase
  {
    public override void CleanXml(XmlCleanerContext context)
    {
      context.XmlToClean = HttpUtility.UrlDecode(context.XmlToClean);
    }
  }
}