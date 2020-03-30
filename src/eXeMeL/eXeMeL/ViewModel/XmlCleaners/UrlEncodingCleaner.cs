using System.Net;

namespace eXeMeL.ViewModel.XmlCleaners
{
  internal class UrlEncodingCleaner : XmlCleanerBase
  {
    public override void CleanXml(XmlCleanerContext context)
    {
      context.XmlToClean = WebUtility.UrlDecode(context.XmlToClean);
    }
  }
}