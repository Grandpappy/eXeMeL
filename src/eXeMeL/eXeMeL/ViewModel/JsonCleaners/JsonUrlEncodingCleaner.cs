using System.Net;

namespace eXeMeL.ViewModel.JsonCleaners
{
  internal class JsonUrlEncodingCleaner : JsonCleanerBase
  {
    public override void Clean(JsonCleanerContext context)
    {
      context.TextToClean = WebUtility.UrlDecode(context.TextToClean);
    }
  }
}
