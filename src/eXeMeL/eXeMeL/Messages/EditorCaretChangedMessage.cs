namespace eXeMeL.Messages
{
  /// <summary>
  /// Broadcast when the editor's caret moves to a different source line (typically a click
  /// or arrow-key navigation). Used to flash a highlight on the matching line in the Markdown
  /// preview, separate from scroll-syncing.
  /// </summary>
  /// <param name="Line">1-based source line number of the caret.</param>
  public record EditorCaretChangedMessage(int Line);
}
