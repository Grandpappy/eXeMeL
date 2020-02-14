namespace eXeMeL.ViewModel.XmlCleaners
{
  internal class VisualStudioCleaner : XmlCleanerBase
  {

    public override void CleanXml(XmlCleanerContext context)
    {
      context.XmlToClean = context.XmlToClean.Replace("\\\"", "\"");
    }
  }
}