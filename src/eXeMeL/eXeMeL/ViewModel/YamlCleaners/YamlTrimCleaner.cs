namespace eXeMeL.ViewModel.YamlCleaners
{
  internal class YamlTrimCleaner : YamlCleanerBase
  {
    public override void Clean(YamlCleanerContext context)
    {
      context.TextToClean = context.TextToClean.Trim();
    }
  }
}
