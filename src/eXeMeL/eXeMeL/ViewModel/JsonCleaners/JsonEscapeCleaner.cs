namespace eXeMeL.ViewModel.JsonCleaners
{
  /// <summary>
  /// Unescapes JSON that has been embedded in a string literal.
  /// Handles common escape patterns: \" → ", \\ → \, \n → newline, \t → tab, \/ → /
  /// Only applies if the content looks like it was escaped (contains \").
  /// </summary>
  internal class JsonEscapeCleaner : JsonCleanerBase
  {
    public override void Clean(JsonCleanerContext context)
    {
      var text = context.TextToClean;

      // Only unescape if it looks like the JSON was embedded in a string literal
      if (!text.Contains("\\\""))
        return;

      text = text.Replace("\\\"", "\"");
      text = text.Replace("\\\\", "\\");
      text = text.Replace("\\n", "\n");
      text = text.Replace("\\t", "\t");
      text = text.Replace("\\r", "\r");
      text = text.Replace("\\/", "/");

      context.TextToClean = text;
    }
  }
}
