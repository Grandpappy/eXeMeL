using System.Threading.Tasks;
using eXeMeL.Utilities;
using Xunit;

namespace eXeMeL.Tests.Utilities
{
  public class EncodedXmlExtractorTests
  {
    [Fact]
    public async Task GetDecodedXmlAroundIndex_ReturnsDecodedTextElement()
    {
      // Caret is inside a text element between > and <
      var xml = "<root>&lt;inner&gt;value&lt;/inner&gt;</root>";
      var extractor = new EncodedXmlExtractor(xml);

      // Find the position inside the text content (after first >)
      var caretOffset = xml.IndexOf('&');
      var result = await extractor.GetDecodedXmlAroundIndexAsync(caretOffset);

      Assert.NotNull(result);
      Assert.Equal("<inner>value</inner>", result);
    }

    [Fact]
    public async Task GetDecodedXmlAroundIndex_ReturnsDecodedAttributeValue()
    {
      // Caret is inside an attribute value between quotes
      var xml = "<root attr=\"&lt;value&gt;\"/>";
      var extractor = new EncodedXmlExtractor(xml);

      // Position inside the attribute value (after the opening quote)
      var quoteIndex = xml.IndexOf('"');
      var caretOffset = quoteIndex + 2; // Inside the attribute value
      var result = await extractor.GetDecodedXmlAroundIndexAsync(caretOffset);

      Assert.NotNull(result);
      Assert.Equal("<value>", result);
    }

    [Fact]
    public async Task GetDecodedXmlAroundIndex_ReturnsNullWhenCaretOnTag()
    {
      // Caret is on a < character (not in text element or attribute)
      var xml = "<root>text</root>";
      var extractor = new EncodedXmlExtractor(xml);

      // Position at the opening < of root
      var result = await extractor.GetDecodedXmlAroundIndexAsync(0);

      Assert.Null(result);
    }

    [Fact]
    public async Task GetDecodedXmlAroundIndex_ReturnsNullWhenCaretBeyondLength()
    {
      var xml = "<root/>";
      var extractor = new EncodedXmlExtractor(xml);

      var result = await extractor.GetDecodedXmlAroundIndexAsync(xml.Length);

      Assert.Null(result);
    }

    [Fact]
    public async Task GetDecodedXmlAroundIndex_DecodesHtmlEntities()
    {
      var xml = "<root>&amp;&lt;&gt;&quot;</root>";
      var extractor = new EncodedXmlExtractor(xml);

      // Position inside the text content
      var caretOffset = xml.IndexOf('&');
      var result = await extractor.GetDecodedXmlAroundIndexAsync(caretOffset);

      Assert.NotNull(result);
      Assert.Equal("&<>\"", result);
    }

    [Fact]
    public async Task GetDecodedXmlAroundIndex_HandlesPlainTextContent()
    {
      var xml = "<root>plain text</root>";
      var extractor = new EncodedXmlExtractor(xml);

      var caretOffset = xml.IndexOf('p'); // inside "plain text"
      var result = await extractor.GetDecodedXmlAroundIndexAsync(caretOffset);

      Assert.NotNull(result);
      Assert.Equal("plain text", result);
    }

    [Fact]
    public async Task GetDecodedXmlAroundIndex_ReturnsNullWhenCaretInsideTagName()
    {
      var xml = "<root>text</root>";
      var extractor = new EncodedXmlExtractor(xml);

      // Position at 'r' in <root> - walking backwards hits '<' first, so not text element
      // And no '"' is encountered, so not attribute either -> null
      var caretOffset = 1; // the 'r' in <root>
      var result = await extractor.GetDecodedXmlAroundIndexAsync(caretOffset);

      Assert.Null(result);
    }

    [Fact]
    public async Task GetDecodedXmlAroundIndex_HandlesAttributeWithEncodedXml()
    {
      var xml = "<item data=\"&lt;nested/&gt;\" />";
      var extractor = new EncodedXmlExtractor(xml);

      // Caret inside the attribute value
      var firstQuote = xml.IndexOf('"');
      var caretOffset = firstQuote + 3; // well inside the attribute value
      var result = await extractor.GetDecodedXmlAroundIndexAsync(caretOffset);

      Assert.NotNull(result);
      Assert.Equal("<nested/>", result);
    }

    [Fact]
    public async Task GetDecodedXmlAroundIndex_ReturnsNullAtPositionZero()
    {
      // Position 0 in IsCaretInTextElement: while(caretOffset > 0) fails immediately,
      // returns false. Same for IsCaretInAttribute. So returns null.
      var xml = "<root>value</root>";
      var extractor = new EncodedXmlExtractor(xml);

      var result = await extractor.GetDecodedXmlAroundIndexAsync(0);

      Assert.Null(result);
    }

    [Fact]
    public async Task GetDecodedXmlAroundIndex_HandlesMultipleTextElements()
    {
      var xml = "<a>first</a><b>second</b>";
      var extractor = new EncodedXmlExtractor(xml);

      // Caret inside "second"
      var caretOffset = xml.IndexOf("second");
      var result = await extractor.GetDecodedXmlAroundIndexAsync(caretOffset);

      Assert.NotNull(result);
      Assert.Equal("second", result);
    }

    [Fact]
    public async Task GetDecodedXmlAroundIndex_HandlesNestedElements()
    {
      var xml = "<outer><inner>content</inner></outer>";
      var extractor = new EncodedXmlExtractor(xml);

      // Caret inside "content"
      var caretOffset = xml.IndexOf("content");
      var result = await extractor.GetDecodedXmlAroundIndexAsync(caretOffset);

      Assert.NotNull(result);
      Assert.Equal("content", result);
    }
  }
}
