using Markdig;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdown.ColorCode;

namespace eXeMeL.ViewModel.MarkdownUtility
{
  /// <summary>
  /// Renders Markdown text to HTML for the WebView2-based preview. The pipeline is built once
  /// and reused — pipeline construction is the slow part; per-render parsing is fast.
  /// </summary>
  public class MarkdownPreviewRenderer
  {
    private readonly MarkdownPipeline _pipeline;

    public MarkdownPreviewRenderer()
    {
      _pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseYamlFrontMatter()
        .UseEmojiAndSmiley()
        .UseSoftlineBreakAsHardlineBreak()
        .UseColorCode(HtmlFormatterType.Css) // emit CSS classes (theme-swappable), not inline styles
        .Use<LineMappingExtension>()
        .Build();
    }

    public string ToHtml(string markdown)
    {
      if (string.IsNullOrEmpty(markdown)) return string.Empty;
      return Markdig.Markdown.ToHtml(markdown, _pipeline);
    }
  }

  /// <summary>
  /// Markdig extension that decorates every top-level block with a <c>data-line</c> attribute
  /// containing the 1-based source line, enabling bidirectional scroll sync between the
  /// AvalonEdit editor and the WebView2 preview.
  /// </summary>
  internal class LineMappingExtension : IMarkdownExtension
  {
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
      pipeline.DocumentProcessed -= OnDocumentProcessed;
      pipeline.DocumentProcessed += OnDocumentProcessed;
    }

    public void Setup(MarkdownPipeline pipeline, Markdig.Renderers.IMarkdownRenderer renderer) { }

    private static void OnDocumentProcessed(MarkdownDocument document)
    {
      foreach (var block in document)
        AnnotateBlock(block);
    }

    private static void AnnotateBlock(Block block)
    {
      // Markdig stores 0-based lines; convert to 1-based to match AvalonEdit's editor display.
      var line = block.Line + 1;
      block.GetAttributes().AddProperty("data-line", line.ToString());

      // Recurse into block containers so nested blocks (list items, blockquote contents, etc.)
      // also get data-line attributes — improves scroll-sync granularity within long lists.
      if (block is ContainerBlock container)
      {
        foreach (var child in container)
          AnnotateBlock(child);
      }
    }
  }
}
