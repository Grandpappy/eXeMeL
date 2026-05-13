namespace eXeMeL.Messages
{
  /// <summary>
  /// Broadcast when the editor's caret moves to a different source line (click, arrow keys,
  /// typing across lines). Used to flash a highlight on the matching line in the Markdown
  /// preview, and to scroll that line into view if it's currently off-screen.
  /// </summary>
  /// <param name="Line">1-based source line number of the caret.</param>
  /// <param name="ScrollHint">Where the caret sits in the editor's viewport, 0.0 (top) to
  /// 1.0 (bottom). The preview uses this to position the matching line at approximately
  /// the same proportional spot when it has to scroll.</param>
  public record EditorCaretChangedMessage(int Line, double ScrollHint);
}
