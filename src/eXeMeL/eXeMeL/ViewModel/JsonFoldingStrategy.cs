using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;

namespace eXeMeL.ViewModel
{
  /// <summary>
  /// Folding strategy for JSON documents. Creates fold regions for
  /// matching { } and [ ] pairs, respecting string literals.
  /// </summary>
  public class JsonFoldingStrategy
  {
    public void UpdateFoldings(FoldingManager manager, TextDocument document)
    {
      var folds = CreateNewFoldings(document);
      manager.UpdateFoldings(folds, -1);
    }

    private List<NewFolding> CreateNewFoldings(TextDocument document)
    {
      var folds = new List<NewFolding>();
      var text = document.Text;
      var stack = new Stack<int>(); // stack of opening brace/bracket offsets
      bool inString = false;
      bool escaped = false;

      for (int i = 0; i < text.Length; i++)
      {
        char c = text[i];

        if (escaped)
        {
          escaped = false;
          continue;
        }

        if (c == '\\' && inString)
        {
          escaped = true;
          continue;
        }

        if (c == '"')
        {
          inString = !inString;
          continue;
        }

        if (inString)
          continue;

        if (c == '{' || c == '[')
        {
          stack.Push(i);
        }
        else if (c == '}' || c == ']')
        {
          if (stack.Count > 0)
          {
            int openOffset = stack.Pop();
            // Only create fold if it spans multiple lines
            var openLine = document.GetLineByOffset(openOffset).LineNumber;
            var closeLine = document.GetLineByOffset(i).LineNumber;
            if (closeLine > openLine)
            {
              folds.Add(new NewFolding(openOffset, i + 1));
            }
          }
        }
      }

      folds.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
      return folds;
    }
  }
}
