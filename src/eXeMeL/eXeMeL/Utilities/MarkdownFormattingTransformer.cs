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
    // Match ***bold+italic***, **bold**, *italic* (in that priority order)
    private static readonly Regex BoldItalicPattern = new(@"\*{3}([^*]+)\*{3}", RegexOptions.Compiled);
    private static readonly Regex BoldPattern = new(@"\*{2}([^*]+)\*{2}", RegexOptions.Compiled);
    private static readonly Regex ItalicPattern = new(@"(?<!\*)\*(?!\*)([^*]+)\*(?!\*)", RegexOptions.Compiled);
    private static readonly Regex UnderscoreBoldPattern = new(@"__([^_]+)__", RegexOptions.Compiled);
    private static readonly Regex UnderscoreItalicPattern = new(@"(?<!_)_(?!_)([^_]+)_(?!_)", RegexOptions.Compiled);

    protected override void ColorizeLine(DocumentLine line)
    {
      var lineText = CurrentContext.Document.GetText(line);
      var lineStart = line.Offset;

      // Bold+Italic (*** ***)
      ApplyPattern(lineText, lineStart, BoldItalicPattern, fw: FontWeights.Bold, fs: FontStyles.Italic);

      // Bold (** **)
      ApplyPattern(lineText, lineStart, BoldPattern, fw: FontWeights.Bold, fs: null);
      ApplyPattern(lineText, lineStart, UnderscoreBoldPattern, fw: FontWeights.Bold, fs: null);

      // Italic (* *)
      ApplyPattern(lineText, lineStart, ItalicPattern, fw: null, fs: FontStyles.Italic);
      ApplyPattern(lineText, lineStart, UnderscoreItalicPattern, fw: null, fs: FontStyles.Italic);
    }

    private void ApplyPattern(string lineText, int lineStart, Regex pattern, FontWeight? fw, FontStyle? fs)
    {
      foreach (Match match in pattern.Matches(lineText))
      {
        var start = lineStart + match.Index;
        var end = start + match.Length;

        ChangeLinePart(start, end, element =>
        {
          var currentTypeface = element.TextRunProperties.Typeface;
          var newWeight = fw ?? currentTypeface.Weight;
          var newStyle = fs ?? currentTypeface.Style;
          element.TextRunProperties.SetTypeface(new Typeface(
            currentTypeface.FontFamily,
            newStyle,
            newWeight,
            currentTypeface.Stretch));
        });
      }
    }
  }
}
