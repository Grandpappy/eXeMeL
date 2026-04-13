using System.Text.RegularExpressions;

namespace eXeMeL.Model
{
  public static class ContentTypeDetector
  {
    private static readonly Regex YamlKeyValuePattern = new(@"^\s*[\w][\w.\-]*\s*:", RegexOptions.Multiline);

    public static DocumentContentType Detect(string content)
    {
      if (string.IsNullOrWhiteSpace(content))
        return DocumentContentType.Text;

      var trimmed = content.TrimStart();

      // JSON: starts with { or [
      if (trimmed.Length > 0 && (trimmed[0] == '{' || trimmed[0] == '['))
        return DocumentContentType.Json;

      // XML: starts with <
      if (trimmed.Length > 0 && trimmed[0] == '<')
        return DocumentContentType.Xml;

      // URL-encoded XML (%3C = <) or JSON (%7B = {, %5B = [)
      if (trimmed.StartsWith("%3C", System.StringComparison.OrdinalIgnoreCase))
        return DocumentContentType.Xml;
      if (trimmed.StartsWith("%7B", System.StringComparison.OrdinalIgnoreCase) ||
          trimmed.StartsWith("%5B", System.StringComparison.OrdinalIgnoreCase))
        return DocumentContentType.Json;

      // Markdown: starts with # heading
      if (trimmed.StartsWith("# ") || trimmed.StartsWith("## "))
        return DocumentContentType.Markdown;

      // YAML: starts with --- document marker
      if (trimmed.StartsWith("---"))
        return DocumentContentType.Yaml;

      // YAML: first few lines match key: value pattern
      // Only exclude if the content looks like it starts as JSON/XML (already checked above)
      // YAML can legitimately contain { } [ ] in flow sequences/mappings
      var firstChunk = trimmed.Length > 500 ? trimmed[..500] : trimmed;
      if (YamlKeyValuePattern.IsMatch(firstChunk))
      {
        return DocumentContentType.Yaml;
      }

      // Fallback
      return DocumentContentType.Text;
    }

    public static DocumentContentType? DetectFromFileExtension(string filePath)
    {
      if (string.IsNullOrEmpty(filePath))
        return null;

      var ext = System.IO.Path.GetExtension(filePath)?.ToLowerInvariant();
      return ext switch
      {
        ".json" => DocumentContentType.Json,
        ".xml" or ".xsl" or ".xslt" or ".xsd" or ".config" or ".csproj"
          or ".vbproj" or ".xaml" or ".xshd" or ".nuspec" or ".targets"
          or ".props" or ".proj" => DocumentContentType.Xml,
        ".yaml" or ".yml" => DocumentContentType.Yaml,
        ".md" or ".markdown" => DocumentContentType.Markdown,
        ".txt" or ".log" => DocumentContentType.Text,
        _ => null
      };
    }
  }
}
