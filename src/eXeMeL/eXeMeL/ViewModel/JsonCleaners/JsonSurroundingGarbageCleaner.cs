using System;

namespace eXeMeL.ViewModel.JsonCleaners
{
  /// <summary>
  /// Extracts JSON from surrounding garbage text by finding the first { or [
  /// and the matching last } or ].
  /// </summary>
  internal class JsonSurroundingGarbageCleaner : JsonCleanerBase
  {
    public override void Clean(JsonCleanerContext context)
    {
      var text = context.TextToClean;

      int firstBrace = text.IndexOf('{');
      int firstBracket = text.IndexOf('[');

      int start;
      char closeChar;

      if (firstBrace < 0 && firstBracket < 0)
        return; // No JSON-like content found

      if (firstBrace < 0)
      {
        start = firstBracket;
        closeChar = ']';
      }
      else if (firstBracket < 0)
      {
        start = firstBrace;
        closeChar = '}';
      }
      else if (firstBrace < firstBracket)
      {
        start = firstBrace;
        closeChar = '}';
      }
      else
      {
        start = firstBracket;
        closeChar = ']';
      }

      int end = text.LastIndexOf(closeChar);
      if (end < start)
        return;

      context.TextToClean = text.Substring(start, end - start + 1);
    }
  }
}
