using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace eXeMeL.Utilities
{
  /// <summary>
  /// Applies bold and italic visual formatting to Markdown text in AvalonEdit.
  /// The markers (**bold**, *italic*, ***both***) remain visible but the text
  /// between them renders with the appropriate font weight/style.
  /// </summary>
  public class MarkdownFormattingTransformer : DocumentColorizingTransformer
  {
    private static readonly Regex BoldItalicPattern = new(@"\*{3}(.+?)\*{3}", RegexOptions.Compiled);
    private static readonly Regex BoldStarPattern = new(@"\*{2}(.+?)\*{2}", RegexOptions.Compiled);
    private static readonly Regex BoldUnderscorePattern = new(@"__(.+?)__", RegexOptions.Compiled);
    private static readonly Regex ItalicStarPattern = new(@"(?<!\*)\*(?!\*)(.+?)(?<!\*)\*(?!\*)", RegexOptions.Compiled);
    private static readonly Regex ItalicUnderscorePattern = new(@"(?<!_)_(?!_)(.+?)(?<!_)_(?!_)", RegexOptions.Compiled);

    protected override void ColorizeLine(DocumentLine line)
    {
      try
      {
        if (line.Length == 0) return;

        var lineText = CurrentContext.Document.GetText(line.Offset, line.Length);
        var lineStart = line.Offset;
        var lineEnd = line.EndOffset;

        ApplyPattern(lineText, lineStart, lineEnd, BoldItalicPattern, FontWeights.Bold, FontStyles.Italic);
        ApplyPattern(lineText, lineStart, lineEnd, BoldStarPattern, FontWeights.Bold, FontStyles.Normal);
        ApplyPattern(lineText, lineStart, lineEnd, BoldUnderscorePattern, FontWeights.Bold, FontStyles.Normal);
        ApplyPattern(lineText, lineStart, lineEnd, ItalicStarPattern, FontWeights.Normal, FontStyles.Italic);
        ApplyPattern(lineText, lineStart, lineEnd, ItalicUnderscorePattern, FontWeights.Normal, FontStyles.Italic);
      }
      catch
      {
        // Regex or offset errors — skip this line silently
      }
    }

    private void ApplyPattern(string lineText, int lineStart, int lineEnd, Regex pattern, FontWeight weight, FontStyle style)
    {
      foreach (Match match in pattern.Matches(lineText))
      {
        var start = lineStart + match.Index;
        var end = start + match.Length;

        // Clamp to line boundaries
        if (start < lineStart) start = lineStart;
        if (end > lineEnd) end = lineEnd;
        if (start >= end) continue;

        try
        {
          ChangeLinePart(start, end, element =>
          {
            var tf = element.TextRunProperties.Typeface;
            var newWeight = weight != FontWeights.Normal ? weight : tf.Weight;
            var newStyle = style != FontStyles.Normal ? style : tf.Style;
            element.TextRunProperties.SetTypeface(new Typeface(
              tf.FontFamily, newStyle, newWeight, tf.Stretch));
          });
        }
        catch
        {
          // ChangeLinePart can throw on edge cases — skip
        }
      }
    }
  }
}
