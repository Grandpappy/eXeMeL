namespace eXeMeL.ViewModel.JsonCleaners
{
  internal class JsonTrimCleaner : JsonCleanerBase
  {
    public override void Clean(JsonCleanerContext context)
    {
      context.TextToClean = context.TextToClean.Trim();
    }
  }
}
