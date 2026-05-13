namespace eXeMeL.Messages
{
  /// <summary>
  /// Broadcast when the editor's caret moves to a new line, so the Markdown preview
  /// can scroll to the matching <c>[data-line]</c> element.
  /// </summary>
  /// <param name="Line">1-based source line number.</param>
  public record EditorCaretLineChangedMessage(int Line);
}
