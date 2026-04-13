using System;
using System.Text.RegularExpressions;

namespace eXeMeL.Model
{
  public static class ContentTypeDetector
  {
    // Markdown strong signals — patterns unique to Markdown, unlikely in YAML
    private static readonly Regex MarkdownBoldPattern = new(@"\*\*[^*]+\*\*|__[^_]+__", RegexOptions.Compiled);
    private static readonly Regex MarkdownLinkPattern = new(@"\[[^\]]+\]\([^\)]+\)", RegexOptions.Compiled);
    private static readonly Regex MarkdownImagePattern = new(@"!\[[^\]]+\]\([^\)]+\)", RegexOptions.Compiled);
    private static readonly Regex MarkdownStrikePattern = new(@"~~[^~]+~~", RegexOptions.Compiled);
    private static readonly Regex MarkdownHeadingPattern = new(@"(?m)^#{1,6}\s", RegexOptions.Compiled);

    // YAML structural signals — indented key:value nesting is the strongest indicator
    private static readonly Regex YamlRootKeyValuePattern = new(@"(?m)^[\w][\w.\-]*\s*:", RegexOptions.Compiled);
    private static readonly Regex YamlIndentedKeyValuePattern = new(@"(?m)^[ \t]+[\w][\w.\-]*\s*:", RegexOptions.Compiled);
    private static readonly Regex YamlIndentedSequencePattern = new(@"(?m)^[ \t]+- ", RegexOptions.Compiled);

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
      if (trimmed.StartsWith("%3C", StringComparison.OrdinalIgnoreCase))
        return DocumentContentType.Xml;
      if (trimmed.StartsWith("%7B", StringComparison.OrdinalIgnoreCase) ||
          trimmed.StartsWith("%5B", StringComparison.OrdinalIgnoreCase))
        return DocumentContentType.Json;

      // Score-based detection for Markdown vs YAML vs Text.
      // Scans first 2000 chars for format-specific patterns and compares weighted scores.
      var sample = trimmed.Length > 2000 ? trimmed[..2000] : trimmed;
      var markdownScore = ScoreMarkdown(sample);
      var yamlScore = ScoreYaml(sample);

      if (markdownScore >= 2 && markdownScore > yamlScore)
        return DocumentContentType.Markdown;

      if (yamlScore >= 2)
        return DocumentContentType.Yaml;

      // Weak signal fallback: single heading at start still counts as Markdown
      if (trimmed.StartsWith("# ") || trimmed.StartsWith("## "))
        return DocumentContentType.Markdown;

      return DocumentContentType.Text;
    }

    private static int ScoreMarkdown(string sample)
    {
      int score = 0;

      // Strong signals — patterns that are unique to Markdown (3 pts each)
      if (MarkdownBoldPattern.IsMatch(sample)) score += 3;
      if (MarkdownLinkPattern.IsMatch(sample)) score += 3;
      if (MarkdownImagePattern.IsMatch(sample)) score += 3;
      if (sample.Contains("```"))              score += 3;
      if (MarkdownStrikePattern.IsMatch(sample)) score += 2;

      // Headings — ambiguous with YAML comments, so lower weight.
      // Multiple headings strengthen the signal (capped at 3).
      var headingCount = MarkdownHeadingPattern.Matches(sample).Count;
      score += Math.Min(headingCount, 3);

      return score;
    }

    private static int ScoreYaml(string sample)
    {
      int score = 0;

      // Document separator at start
      if (sample.TrimStart().StartsWith("---"))
        score += 2;

      // Root-level key: value lines (no indentation)
      var rootCount = YamlRootKeyValuePattern.Matches(sample).Count;
      score += Math.Min(rootCount, 3);

      // Indented key: value — structural nesting is a strong YAML signal (2x weight)
      var indentedCount = YamlIndentedKeyValuePattern.Matches(sample).Count;
      score += Math.Min(indentedCount * 2, 6);

      // Indented sequence items (  - item)
      var sequenceCount = YamlIndentedSequencePattern.Matches(sample).Count;
      score += Math.Min(sequenceCount, 3);

      return score;
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
