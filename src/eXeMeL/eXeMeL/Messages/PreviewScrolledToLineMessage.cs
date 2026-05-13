namespace eXeMeL.Messages
{
  /// <summary>
  /// Broadcast when the Markdown preview user scrolls, so the editor can optionally
  /// follow along (preview -> editor direction of scroll sync).
  /// </summary>
  /// <param name="Line">1-based source line number.</param>
  public record PreviewScrolledToLineMessage(int Line);
}
