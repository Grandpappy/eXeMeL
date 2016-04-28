using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using eXeMeL.Model;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace eXeMeL.ViewModel
{
  public class AllSelectionColorizer : DocumentColorizingTransformer
  {
    private TextEditor Editor { get; set; }
    public Settings Settings { get; set; }



    public AllSelectionColorizer(TextEditor editor, Settings settings)
    {
      this.Editor = editor;
      this.Settings = settings;
    }



    protected override void ColorizeLine(DocumentLine line)
    {
      if (!this.Settings.HighlightOtherInstancesOfSelection)
        return;

      if (string.IsNullOrEmpty(this.Editor.SelectedText)) return;

      var lineStartOffset = line.Offset;
      var text = this.CurrentContext.Document.GetText(line);
      var start = 0;
      int index;

      var caretOffset = this.Editor.CaretOffset;

      while ((index = text.IndexOf(this.Editor.SelectedText, start)) >= 0)
      {
        var startOffset = lineStartOffset + index;
        var endOffset = startOffset + this.Editor.SelectionLength;
        var isCaretInSelection = (caretOffset >= startOffset) && (caretOffset <= endOffset);

        if (!isCaretInSelection)
        {
          base.ChangeLinePart(
            startOffset,
            endOffset,
            (VisualLineElement element) =>
            {
              // maybe change color according to theme later
              element.TextRunProperties.SetForegroundBrush(Brushes.White);
              element.TextRunProperties.SetBackgroundBrush(Brushes.DodgerBlue);
            });
        }

        start = index + 1; // search for next occurrence
      }
    }
  }
}
