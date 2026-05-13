namespace eXeMeL.Messages
{
  /// <summary>
  /// Broadcast when the editor's viewport scrolls (or its content changes scroll position),
  /// so the Markdown preview can scroll to the matching <c>[data-line]</c> element.
  /// </summary>
  /// <param name="Line">1-based source line number of the first visible line in the editor.</param>
  public record EditorScrolledToLineMessage(int Line);
}
