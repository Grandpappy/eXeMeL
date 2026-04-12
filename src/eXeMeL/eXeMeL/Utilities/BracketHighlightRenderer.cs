using System;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace eXeMeL.Utilities
{
  /// <summary>
  /// Highlights matching bracket pairs when the caret is next to a bracket.
  /// Supports (), [], {}, and <> for XML/JSON/YAML.
  /// </summary>
  public class BracketHighlightRenderer : IBackgroundRenderer
  {
    private readonly TextEditor _editor;
    private int? _openBracketOffset;
    private int? _closeBracketOffset;

    private static readonly Brush HighlightBrush;

    static BracketHighlightRenderer()
    {
      HighlightBrush = new SolidColorBrush(Color.FromArgb(40, 180, 180, 180));
      HighlightBrush.Freeze();
    }

    public BracketHighlightRenderer(TextEditor editor)
    {
      _editor = editor;
      _editor.TextArea.Caret.PositionChanged += (s, e) => UpdateBrackets();
    }

    public KnownLayer Layer => KnownLayer.Selection;

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
      if (_openBracketOffset.HasValue)
        DrawBracketHighlight(textView, drawingContext, _openBracketOffset.Value);
      if (_closeBracketOffset.HasValue)
        DrawBracketHighlight(textView, drawingContext, _closeBracketOffset.Value);
    }

    private void DrawBracketHighlight(TextView textView, DrawingContext drawingContext, int offset)
    {
      if (offset < 0 || offset >= _editor.Document.TextLength)
        return;

      var segment = new TextSegment { StartOffset = offset, Length = 1 };
      foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, segment))
      {
        drawingContext.DrawRectangle(HighlightBrush, null,
          new Rect(rect.Location, new Size(rect.Width, rect.Height)));
      }
    }

    private void UpdateBrackets()
    {
      var oldOpen = _openBracketOffset;
      var oldClose = _closeBracketOffset;

      _openBracketOffset = null;
      _closeBracketOffset = null;

      var doc = _editor.Document;
      var offset = _editor.TextArea.Caret.Offset;

      if (doc == null || offset < 0 || offset > doc.TextLength)
      {
        if (oldOpen.HasValue || oldClose.HasValue)
          _editor.TextArea.TextView.InvalidateLayer(KnownLayer.Selection);
        return;
      }

      // Check character at caret and before caret
      if (offset < doc.TextLength && IsBracket(doc.GetCharAt(offset)))
        FindMatchingBracket(doc, offset);
      else if (offset > 0 && IsBracket(doc.GetCharAt(offset - 1)))
        FindMatchingBracket(doc, offset - 1);

      if (_openBracketOffset != oldOpen || _closeBracketOffset != oldClose)
        _editor.TextArea.TextView.InvalidateLayer(KnownLayer.Selection);
    }

    private void FindMatchingBracket(TextDocument doc, int offset)
    {
      char ch = doc.GetCharAt(offset);
      char match;
      bool forward;

      switch (ch)
      {
        case '(': match = ')'; forward = true; break;
        case ')': match = '('; forward = false; break;
        case '[': match = ']'; forward = true; break;
        case ']': match = '['; forward = false; break;
        case '{': match = '}'; forward = true; break;
        case '}': match = '{'; forward = false; break;
        case '<': match = '>'; forward = true; break;
        case '>': match = '<'; forward = false; break;
        default: return;
      }

      int depth = 1;
      int i = forward ? offset + 1 : offset - 1;

      while (i >= 0 && i < doc.TextLength && depth > 0)
      {
        char c = doc.GetCharAt(i);
        if (c == ch) depth++;
        else if (c == match) depth--;

        if (depth == 0)
        {
          _openBracketOffset = forward ? offset : i;
          _closeBracketOffset = forward ? i : offset;
          return;
        }

        i += forward ? 1 : -1;
      }
    }

    private static bool IsBracket(char c) =>
      c == '(' || c == ')' || c == '[' || c == ']' ||
      c == '{' || c == '}' || c == '<' || c == '>';
  }
}
