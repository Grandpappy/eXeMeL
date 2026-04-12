using System;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;

namespace eXeMeL.ViewModel
{
  /// <summary>
  /// Indent-based folding strategy for YAML documents.
  /// Creates fold regions when indentation increases.
  /// </summary>
  public class YamlFoldingStrategy
  {
    public void UpdateFoldings(FoldingManager manager, TextDocument document)
    {
      var folds = CreateNewFoldings(document);
      manager.UpdateFoldings(folds, -1);
    }

    private List<NewFolding> CreateNewFoldings(TextDocument document)
    {
      var folds = new List<NewFolding>();
      var stack = new Stack<(int indent, int startOffset)>();

      for (int i = 1; i <= document.LineCount; i++)
      {
        var line = document.GetLineByNumber(i);
        var text = document.GetText(line.Offset, line.Length);

        // Skip blank and comment-only lines
        var trimmed = text.TrimStart();
        if (trimmed.Length == 0 || trimmed[0] == '#')
          continue;

        int indent = text.Length - trimmed.Length;

        // Pop all stack entries with >= indent (they're closed by this line)
        while (stack.Count > 0 && stack.Peek().indent >= indent)
        {
          var entry = stack.Pop();
          var prevLine = document.GetLineByNumber(Math.Max(1, i - 1));
          var endOffset = prevLine.EndOffset;
          if (endOffset > entry.startOffset)
          {
            folds.Add(new NewFolding(entry.startOffset, endOffset));
          }
        }

        // If this line ends with : or starts a block, it might start a fold
        if (trimmed.EndsWith(':') || trimmed.EndsWith(":-") ||
            (trimmed.Contains(':') && !trimmed.StartsWith('-')))
        {
          stack.Push((indent, line.Offset));
        }
        else if (trimmed.StartsWith("- "))
        {
          // Array items at this indent level can group
          if (stack.Count == 0 || stack.Peek().indent < indent)
          {
            stack.Push((indent, line.Offset));
          }
        }
      }

      // Close remaining open folds at document end
      var lastLine = document.GetLineByNumber(document.LineCount);
      while (stack.Count > 0)
      {
        var entry = stack.Pop();
        if (lastLine.EndOffset > entry.startOffset)
          folds.Add(new NewFolding(entry.startOffset, lastLine.EndOffset));
      }

      folds.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
      return folds;
    }
  }
}
