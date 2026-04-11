using System;
using System.Text.Json;

namespace eXeMeL.ViewModel.JsonCleaners
{
  /// <summary>
  /// Parses JSON and re-serializes with indentation for pretty-printing.
  /// If parsing fails, sets ErrorMessage on the context.
  /// </summary>
  internal class JsonFormatCleaner : JsonCleanerBase
  {
    private static readonly JsonSerializerOptions WriteOptions = new()
    {
      WriteIndented = true
    };

    public override void Clean(JsonCleanerContext context)
    {
      try
      {
        using var doc = JsonDocument.Parse(context.TextToClean);
        context.TextToClean = JsonSerializer.Serialize(doc.RootElement, WriteOptions);
        context.IsParsedSuccessfully = true;
      }
      catch (JsonException ex)
      {
        context.ErrorMessage = $"JSON parse error: {ex.Message}";
      }
    }
  }
}
