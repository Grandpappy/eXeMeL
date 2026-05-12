using eXeMeL.Model;
using Xunit;

namespace eXeMeL.Tests.Model
{
  #region Content Detection

  public class ContentTypeDetector_DetectTests
  {
    // --- First-character fast paths ---

    [Fact]
    public void Detect_JsonObject_ReturnsJson() =>
      Assert.Equal(DocumentContentType.Json, ContentTypeDetector.Detect("{\"key\":\"value\"}"));

    [Fact]
    public void Detect_JsonArray_ReturnsJson() =>
      Assert.Equal(DocumentContentType.Json, ContentTypeDetector.Detect("[1, 2, 3]"));

    [Fact]
    public void Detect_JsonWithLeadingWhitespace_ReturnsJson() =>
      Assert.Equal(DocumentContentType.Json, ContentTypeDetector.Detect("   {\"key\": true}"));

    [Fact]
    public void Detect_XmlElement_ReturnsXml() =>
      Assert.Equal(DocumentContentType.Xml, ContentTypeDetector.Detect("<root><child/></root>"));

    [Fact]
    public void Detect_XmlDeclaration_ReturnsXml() =>
      Assert.Equal(DocumentContentType.Xml, ContentTypeDetector.Detect("<?xml version=\"1.0\"?>"));

    // --- URL-encoded fast paths ---

    [Fact]
    public void Detect_UrlEncodedXml_ReturnsXml() =>
      Assert.Equal(DocumentContentType.Xml, ContentTypeDetector.Detect("%3Croot%3E%3C/root%3E"));

    [Fact]
    public void Detect_UrlEncodedJson_ReturnsJson() =>
      Assert.Equal(DocumentContentType.Json, ContentTypeDetector.Detect("%7B%22key%22%3A1%7D"));

    [Fact]
    public void Detect_UrlEncodedJsonArray_ReturnsJson() =>
      Assert.Equal(DocumentContentType.Json, ContentTypeDetector.Detect("%5B1%2C2%5D"));

    // --- Scoring: Markdown ---

    [Fact]
    public void Detect_MarkdownWithBoldAndLinks_ReturnsMarkdown()
    {
      var md = "Some text with **bold** and a [link](http://example.com)";
      Assert.Equal(DocumentContentType.Markdown, ContentTypeDetector.Detect(md));
    }

    [Fact]
    public void Detect_MarkdownWithCodeFenceAndHeadings_ReturnsMarkdown()
    {
      var md = "# Title\n\nSome text\n\n```\ncode block\n```\n\n## Another heading";
      Assert.Equal(DocumentContentType.Markdown, ContentTypeDetector.Detect(md));
    }

    [Fact]
    public void Detect_MarkdownWithImageAndStrike_ReturnsMarkdown()
    {
      var md = "Here is ![an image](pic.png) and ~~strikethrough~~ text";
      Assert.Equal(DocumentContentType.Markdown, ContentTypeDetector.Detect(md));
    }

    [Fact]
    public void Detect_SingleHeadingAtStart_ReturnsMarkdown() =>
      Assert.Equal(DocumentContentType.Markdown, ContentTypeDetector.Detect("# Just a heading"));

    [Fact]
    public void Detect_H2HeadingAtStart_ReturnsMarkdown() =>
      Assert.Equal(DocumentContentType.Markdown, ContentTypeDetector.Detect("## Subheading"));

    // --- Scoring: YAML ---

    [Fact]
    public void Detect_YamlWithDocumentSeparator_ReturnsYaml()
    {
      var yaml = "---\nname: test\nvalue: 123";
      Assert.Equal(DocumentContentType.Yaml, ContentTypeDetector.Detect(yaml));
    }

    [Fact]
    public void Detect_YamlWithNestedKeys_ReturnsYaml()
    {
      var yaml = "server:\n  host: localhost\n  port: 8080\n  ssl: true";
      Assert.Equal(DocumentContentType.Yaml, ContentTypeDetector.Detect(yaml));
    }

    [Fact]
    public void Detect_YamlWithSequences_ReturnsYaml()
    {
      var yaml = "items:\n  - first\n  - second\n  - third";
      Assert.Equal(DocumentContentType.Yaml, ContentTypeDetector.Detect(yaml));
    }

    [Fact]
    public void Detect_FlatKeyValues_ReturnsYaml()
    {
      var yaml = "name: test\nversion: 1.0\nauthor: someone";
      Assert.Equal(DocumentContentType.Yaml, ContentTypeDetector.Detect(yaml));
    }

    // --- Scoring: Markdown wins over YAML ---

    [Fact]
    public void Detect_MarkdownWithYamlFrontMatter_ReturnsMarkdown()
    {
      // YAML front matter + Markdown body — Markdown signals should outweigh
      var content = "---\ntitle: My Post\n---\n\n# Heading\n\nSome **bold** text and a [link](url)";
      Assert.Equal(DocumentContentType.Markdown, ContentTypeDetector.Detect(content));
    }

    [Fact]
    public void Detect_MarkdownWithHeadingsAndIndentedBulletsOnly_ReturnsMarkdown()
    {
      // Heading-only Markdown (no bold/links/code) with indented bullet lists.
      // Indented bullets look like YAML sequences, so this used to score as a tie
      // and fall through to YAML. Multiple --- HR lines break the tie decisively.
      var content =
        "# Section One\n\n" +
        "- Top item\n  - Nested item\n  - Another nested\n\n" +
        "---\n\n" +
        "# Section Two\n\n" +
        "## Subsection\n\n" +
        "- Item\n  - Child\n\n" +
        "---\n\n" +
        "# Section Three\n\n" +
        "- More items\n  - Indented\n";
      Assert.Equal(DocumentContentType.Markdown, ContentTypeDetector.Detect(content));
    }

    // --- Edge cases ---

    [Fact]
    public void Detect_EmptyString_ReturnsText() =>
      Assert.Equal(DocumentContentType.Text, ContentTypeDetector.Detect(""));

    [Fact]
    public void Detect_Null_ReturnsText() =>
      Assert.Equal(DocumentContentType.Text, ContentTypeDetector.Detect(null));

    [Fact]
    public void Detect_WhitespaceOnly_ReturnsText() =>
      Assert.Equal(DocumentContentType.Text, ContentTypeDetector.Detect("   \n\t  "));

    [Fact]
    public void Detect_PlainText_ReturnsText() =>
      Assert.Equal(DocumentContentType.Text, ContentTypeDetector.Detect("Hello world, this is plain text."));
  }

  #endregion

  #region File Extension Detection

  public class ContentTypeDetector_FileExtensionTests
  {
    [Theory]
    [InlineData("file.json", DocumentContentType.Json)]
    [InlineData("file.xml", DocumentContentType.Xml)]
    [InlineData("file.csproj", DocumentContentType.Xml)]
    [InlineData("file.xaml", DocumentContentType.Xml)]
    [InlineData("file.xshd", DocumentContentType.Xml)]
    [InlineData("file.config", DocumentContentType.Xml)]
    [InlineData("file.yaml", DocumentContentType.Yaml)]
    [InlineData("file.yml", DocumentContentType.Yaml)]
    [InlineData("file.md", DocumentContentType.Markdown)]
    [InlineData("file.markdown", DocumentContentType.Markdown)]
    [InlineData("file.txt", DocumentContentType.Text)]
    [InlineData("file.log", DocumentContentType.Text)]
    public void DetectFromFileExtension_KnownExtensions_ReturnsCorrectType(string path, DocumentContentType expected) =>
      Assert.Equal(expected, ContentTypeDetector.DetectFromFileExtension(path));

    [Theory]
    [InlineData("file.cs")]
    [InlineData("file.py")]
    [InlineData("file")]
    public void DetectFromFileExtension_UnknownExtension_ReturnsNull(string path) =>
      Assert.Null(ContentTypeDetector.DetectFromFileExtension(path));

    [Fact]
    public void DetectFromFileExtension_Null_ReturnsNull() =>
      Assert.Null(ContentTypeDetector.DetectFromFileExtension(null));

    [Fact]
    public void DetectFromFileExtension_Empty_ReturnsNull() =>
      Assert.Null(ContentTypeDetector.DetectFromFileExtension(""));
  }

  #endregion
}
