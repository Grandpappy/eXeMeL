namespace eXeMeL.ViewModel.JsonCleaners
{
  /// <summary>
  /// Unescapes JSON that has been embedded in a string literal.
  /// Handles common escape patterns: \" → ", \\ → \, \n → newline, \t → tab, \/ → /
  /// Only applies when the JSON structure itself is escaped (e.g. pasted from a C# string
  /// literal like {\"key\":\"value\"}). Does NOT fire for normal JSON that merely contains
  /// \" inside string values (e.g. Windows paths like "C:\\Users\\").
  /// </summary>
  internal class JsonEscapeCleaner : JsonCleanerBase
  {
    public override void Clean(JsonCleanerContext context)
    {
      var text = context.TextToClean;

      // Detect double-escaped JSON: the structural braces/brackets are followed by
      // escaped quotes (\") rather than regular quotes ("). In normal JSON, { is
      // followed by " for the first key. In double-escaped JSON, { is followed by \".
      if (!LooksDoubleEscaped(text))
        return;

      text = text.Replace("\\\"", "\"");
      text = text.Replace("\\\\", "\\");
      text = text.Replace("\\n", "\n");
      text = text.Replace("\\t", "\t");
      text = text.Replace("\\r", "\r");
      text = text.Replace("\\/", "/");

      context.TextToClean = text;
    }

    private static bool LooksDoubleEscaped(string text)
    {
      var trimmed = text.TrimStart();

      // {\"  or  [\"  — structural delimiters followed by escaped quote
      if (trimmed.StartsWith("{\\\"") || trimmed.StartsWith("[\\\""))
        return true;

      // Wrapped in outer string quotes: "{...}" or "[...]"
      if (trimmed.StartsWith("\"{") || trimmed.StartsWith("\"["))
        return true;

      return false;
    }
  }
}
