using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace eXeMeL.ViewModel
{
  public class AllSelectionColorizer : DocumentColorizingTransformer
  {
    private TextEditor Editor { get; set; }

    public AllSelectionColorizer(TextEditor editor)
    {
      Editor = editor;
    }
    protected override void ColorizeLine(DocumentLine line)
    {
      if (string.IsNullOrEmpty(Editor.SelectedText)) return;

      int lineStartOffset = line.Offset;
      string text = CurrentContext.Document.GetText(line);
      int start = 0;
      int index;

      while ((index = text.IndexOf(Editor.SelectedText, start)) >= 0)
      {
        base.ChangeLinePart(
            lineStartOffset + index, // startOffset
            lineStartOffset + index + Editor.SelectionLength, // endOffset
            (VisualLineElement element) => {
              // maybe change color according to theme later
              element.TextRunProperties.SetForegroundBrush(Brushes.White);
              element.TextRunProperties.SetBackgroundBrush(Brushes.DodgerBlue);
            });
        start = index + 1; // search for next occurrence
      }
    }
  }
}
