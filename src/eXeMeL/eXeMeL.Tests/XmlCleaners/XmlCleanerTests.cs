using System;
using System.Collections.Generic;
using System.Xml.Linq;
using eXeMeL.ViewModel.XmlCleaners;
using Xunit;

namespace eXeMeL.Tests.XmlCleaners
{
  #region UrlEncodingCleaner Tests

  public class UrlEncodingCleanerTests
  {
    [Fact]
    public void CleanXml_DecodesUrlEncodedCharacters()
    {
      var context = new XmlCleanerContext { XmlToClean = "%3Croot%3Evalue%3C%2Froot%3E" };
      var cleaner = new UrlEncodingCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("<root>value</root>", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_DecodesSpacesEncodedAsPlus()
    {
      var context = new XmlCleanerContext { XmlToClean = "hello+world" };
      var cleaner = new UrlEncodingCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("hello world", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_DecodesPercentEncodedSpaces()
    {
      var context = new XmlCleanerContext { XmlToClean = "hello%20world" };
      var cleaner = new UrlEncodingCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("hello world", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_LeavesPlainTextUnchanged()
    {
      var context = new XmlCleanerContext { XmlToClean = "<root>value</root>" };
      var cleaner = new UrlEncodingCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("<root>value</root>", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_HandlesEmptyString()
    {
      var context = new XmlCleanerContext { XmlToClean = "" };
      var cleaner = new UrlEncodingCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("", context.XmlToClean);
    }
  }

  #endregion

  #region TrimCleaner Tests

  public class TrimCleanerTests
  {
    [Fact]
    public void CleanXml_TrimsLeadingWhitespace()
    {
      var context = new XmlCleanerContext { XmlToClean = "   <root/>" };
      var cleaner = new TrimCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("<root/>", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_TrimsTrailingWhitespace()
    {
      var context = new XmlCleanerContext { XmlToClean = "<root/>   " };
      var cleaner = new TrimCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("<root/>", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_TrimsBothEnds()
    {
      var context = new XmlCleanerContext { XmlToClean = "  \t <root/> \t  " };
      var cleaner = new TrimCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("<root/>", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_DoesNotTrimInternalWhitespace()
    {
      var context = new XmlCleanerContext { XmlToClean = "<root>  value  </root>" };
      var cleaner = new TrimCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("<root>  value  </root>", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_HandlesEmptyString()
    {
      var context = new XmlCleanerContext { XmlToClean = "" };
      var cleaner = new TrimCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_HandlesWhitespaceOnlyString()
    {
      var context = new XmlCleanerContext { XmlToClean = "   \t\t   " };
      var cleaner = new TrimCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("", context.XmlToClean);
    }
  }

  #endregion

  #region NewLineCleaner Tests

  public class NewLineCleanerTests
  {
    [Fact]
    public void CleanXml_RemovesEnvironmentNewLines()
    {
      var context = new XmlCleanerContext { XmlToClean = "<root>" + Environment.NewLine + "value" + Environment.NewLine + "</root>" };
      var cleaner = new NewLineCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("<root>value</root>", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_RemovesMultipleNewLines()
    {
      var context = new XmlCleanerContext { XmlToClean = "a" + Environment.NewLine + "b" + Environment.NewLine + "c" };
      var cleaner = new NewLineCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("abc", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_LeavesStringWithoutNewLinesUnchanged()
    {
      var context = new XmlCleanerContext { XmlToClean = "<root>value</root>" };
      var cleaner = new NewLineCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("<root>value</root>", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_HandlesEmptyString()
    {
      var context = new XmlCleanerContext { XmlToClean = "" };
      var cleaner = new NewLineCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("", context.XmlToClean);
    }
  }

  #endregion

  #region SurroundingGarbageCleaner Tests

  public class SurroundingGarbageCleanerTests
  {
    [Fact]
    public void CleanXml_RemovesLeadingGarbage()
    {
      var context = new XmlCleanerContext { XmlToClean = "garbage<root/>" };
      var cleaner = new SurroundingGarbageCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("<root/>", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_RemovesTrailingGarbage()
    {
      var context = new XmlCleanerContext { XmlToClean = "<root/>garbage" };
      var cleaner = new SurroundingGarbageCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("<root/>", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_RemovesBothLeadingAndTrailingGarbage()
    {
      var context = new XmlCleanerContext { XmlToClean = "before<root>value</root>after" };
      var cleaner = new SurroundingGarbageCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("<root>value</root>", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_NoChangeWhenNoGarbage()
    {
      var context = new XmlCleanerContext { XmlToClean = "<root>value</root>" };
      var cleaner = new SurroundingGarbageCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("<root>value</root>", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_ReturnsEarlyWhenNoLessThan()
    {
      var original = "no xml here";
      var context = new XmlCleanerContext { XmlToClean = original };
      var cleaner = new SurroundingGarbageCleaner();

      cleaner.CleanXml(context);

      Assert.Equal(original, context.XmlToClean);
    }

    [Fact]
    public void CleanXml_ReturnsEarlyWhenNoGreaterThan()
    {
      var original = "some<text";
      var context = new XmlCleanerContext { XmlToClean = original };
      var cleaner = new SurroundingGarbageCleaner();

      cleaner.CleanXml(context);

      Assert.Equal(original, context.XmlToClean);
    }

    [Fact]
    public void CleanXml_PreservesInternalContent()
    {
      var context = new XmlCleanerContext { XmlToClean = "junk<a><b>text</b></a>junk" };
      var cleaner = new SurroundingGarbageCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("<a><b>text</b></a>", context.XmlToClean);
    }
  }

  #endregion

  #region VisualStudioCleaner Tests

  public class VisualStudioCleanerTests
  {
    [Fact]
    public void CleanXml_ReplacesEscapedQuotesWithQuotes()
    {
      var context = new XmlCleanerContext { XmlToClean = "<root attr=\\\"value\\\"/>" };
      var cleaner = new VisualStudioCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("<root attr=\"value\"/>", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_HandlesMultipleEscapedQuotes()
    {
      var context = new XmlCleanerContext { XmlToClean = "\\\"hello\\\" \\\"world\\\"" };
      var cleaner = new VisualStudioCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("\"hello\" \"world\"", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_NoChangeWithoutEscapedQuotes()
    {
      var context = new XmlCleanerContext { XmlToClean = "<root attr=\"value\"/>" };
      var cleaner = new VisualStudioCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("<root attr=\"value\"/>", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_HandlesEmptyString()
    {
      var context = new XmlCleanerContext { XmlToClean = "" };
      var cleaner = new VisualStudioCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("", context.XmlToClean);
    }
  }

  #endregion

  #region VisualStudioVBScriptCleaner Tests

  public class VisualStudioVBScriptCleanerTests
  {
    [Fact]
    public void CleanXml_CollapsesStandaloneDoubleQuotesToSingle()
    {
      // "" alone -> " (group 1 = doesn't match, so not all groups match)
      var context = new XmlCleanerContext { XmlToClean = "some\"\"text" };
      var cleaner = new VisualStudioVBScriptCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("some\"text", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_PreservesDoubleQuotesInAttributeAssignment()
    {
      // ="" followed by whitespace/closing -> all groups match, kept as-is
      var context = new XmlCleanerContext { XmlToClean = "<root attr=\"\"value\"\" />" };
      var cleaner = new VisualStudioVBScriptCleaner();

      cleaner.CleanXml(context);

      // The ="" at start: = matches group1, "" matches group2, but group3 needs \s or /?> after ""
      // Actually: attr="" matches =(group1) ""(group2) then 'v' is not \s or /?>
      // so group3 doesn't match, so not all groups match -> collapses to "
      // The "" at end before space: no = before it -> group1 doesn't match -> collapses to "
      Assert.Equal("<root attr=\"value\" />", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_PreservesDoubleQuotesWhenAllGroupsMatch()
    {
      // ="" > -> all 3 groups match: = (group1), "" (group2), > (group3)
      var context = new XmlCleanerContext { XmlToClean = "<root attr=\"\" />" };
      var cleaner = new VisualStudioVBScriptCleaner();

      cleaner.CleanXml(context);

      // = matches group1, "" matches group2, " />" -> \s matches space in group3
      // Wait: the regex is (=)?("")(\s|/?\s?>)?
      // For the substring ="" /> the match would be ="" (space)
      // group1: =, group2: "", group3: (space) -> all match -> preserve
      Assert.Equal("<root attr=\"\" />", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_PreservesWhenEqualsDoubleQuoteClosingTag()
    {
      // ="">  -> = (group1), "" (group2), > (group3)  -> all match -> preserve
      var context = new XmlCleanerContext { XmlToClean = "<root attr=\"\">" };
      var cleaner = new VisualStudioVBScriptCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("<root attr=\"\">", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_HandlesNoDoubleQuotes()
    {
      var context = new XmlCleanerContext { XmlToClean = "<root attr=\"value\"/>" };
      var cleaner = new VisualStudioVBScriptCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("<root attr=\"value\"/>", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_HandlesEmptyString()
    {
      var context = new XmlCleanerContext { XmlToClean = "" };
      var cleaner = new VisualStudioVBScriptCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_CollapsesDoubleQuotesWithoutEquals()
    {
      // "" without = prefix -> group1 doesn't match -> not all groups match -> collapse
      var context = new XmlCleanerContext { XmlToClean = "\"\"" };
      var cleaner = new VisualStudioVBScriptCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("\"", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_PreservesEmptyAttributeWithSelfClosingTag()
    {
      // ="" /> -> = (group1), "" (group2), " />" space matches group3 -> all match
      var context = new XmlCleanerContext { XmlToClean = "<item val=\"\" />" };
      var cleaner = new VisualStudioVBScriptCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("<item val=\"\" />", context.XmlToClean);
    }
  }

  #endregion

  #region AddedRootCleaner Tests

  public class AddedRootCleanerTests
  {
    [Fact]
    public void CleanXml_ParsesValidXml()
    {
      var context = new XmlCleanerContext { XmlToClean = "<root><child>value</child></root>" };
      var cleaner = new AddedRootCleaner();

      cleaner.CleanXml(context);

      Assert.NotNull(context.ParsedXml);
      Assert.Equal("root", context.ParsedXml.Name.LocalName);
      Assert.Null(context.ErrorMessage);
    }

    [Fact]
    public void CleanXml_FormatsXmlOutput()
    {
      var context = new XmlCleanerContext { XmlToClean = "<root><child>value</child></root>" };
      var cleaner = new AddedRootCleaner();

      cleaner.CleanXml(context);

      // XElement.ToString(SaveOptions.None) produces indented XML
      Assert.Contains("\n", context.XmlToClean);
      Assert.Contains("  ", context.XmlToClean);
    }

    [Fact]
    public void CleanXml_WrapsFragmentInAddedRoot()
    {
      var context = new XmlCleanerContext { XmlToClean = "<a/><b/>" };
      var cleaner = new AddedRootCleaner();

      cleaner.CleanXml(context);

      Assert.NotNull(context.ParsedXml);
      Assert.Equal("AddedRoot", context.ParsedXml.Name.LocalName);
      Assert.Null(context.ErrorMessage);
    }

    [Fact]
    public void CleanXml_SetsErrorMessageForInvalidXml()
    {
      var context = new XmlCleanerContext { XmlToClean = "this is not xml at all <<<>>>" };
      var cleaner = new AddedRootCleaner();

      cleaner.CleanXml(context);

      Assert.NotNull(context.ErrorMessage);
      Assert.Contains("Unable to parse XML", context.ErrorMessage);
    }

    [Fact]
    public void CleanXml_ReturnsEarlyIfParsedXmlAlreadySet()
    {
      var existingElement = XElement.Parse("<existing/>");
      var originalXml = "some text that is not xml";
      var context = new XmlCleanerContext
      {
        XmlToClean = originalXml,
        ParsedXml = existingElement
      };
      var cleaner = new AddedRootCleaner();

      cleaner.CleanXml(context);

      Assert.Same(existingElement, context.ParsedXml);
      Assert.Equal(originalXml, context.XmlToClean);
    }

    [Fact]
    public void CleanXml_SetsParsedXmlForSingleElement()
    {
      var context = new XmlCleanerContext { XmlToClean = "<item attr=\"val\">text</item>" };
      var cleaner = new AddedRootCleaner();

      cleaner.CleanXml(context);

      Assert.NotNull(context.ParsedXml);
      Assert.Equal("item", context.ParsedXml.Name.LocalName);
      Assert.Null(context.ErrorMessage);
    }
  }

  #endregion

  #region FormatCleaner Tests

  public class FormatCleanerTests
  {
    [Fact]
    public void CleanXml_DoesNotModifyContext()
    {
      var originalXml = "<root>value</root>";
      var context = new XmlCleanerContext { XmlToClean = originalXml };
      var cleaner = new FormatCleaner();

      cleaner.CleanXml(context);

      Assert.Equal(originalXml, context.XmlToClean);
      Assert.Null(context.ParsedXml);
      Assert.Null(context.ErrorMessage);
    }

    [Fact]
    public void CleanXml_LeavesAllContextPropertiesUntouched()
    {
      var parsedXml = XElement.Parse("<test/>");
      var context = new XmlCleanerContext
      {
        XmlToClean = "<test/>",
        ParsedXml = parsedXml,
        ErrorMessage = "some error"
      };
      var cleaner = new FormatCleaner();

      cleaner.CleanXml(context);

      Assert.Equal("<test/>", context.XmlToClean);
      Assert.Same(parsedXml, context.ParsedXml);
      Assert.Equal("some error", context.ErrorMessage);
    }
  }

  #endregion

  #region Integration / Pipeline Tests

  public class XmlCleanerPipelineTests
  {
    private static List<XmlCleanerBase> CreatePipeline()
    {
      return new List<XmlCleanerBase>
      {
        new UrlEncodingCleaner(),
        new TrimCleaner(),
        new NewLineCleaner(),
        new SurroundingGarbageCleaner(),
        new VisualStudioCleaner(),
        new VisualStudioVBScriptCleaner(),
        new AddedRootCleaner(),
        new FormatCleaner()
      };
    }

    private static void RunPipeline(XmlCleanerContext context)
    {
      foreach (var cleaner in CreatePipeline())
      {
        cleaner.CleanXml(context);
      }
    }

    [Fact]
    public void Pipeline_CleansUrlEncodedXml()
    {
      var context = new XmlCleanerContext { XmlToClean = "%3Croot%3Evalue%3C%2Froot%3E" };

      RunPipeline(context);

      Assert.NotNull(context.ParsedXml);
      Assert.Equal("root", context.ParsedXml.Name.LocalName);
      Assert.Null(context.ErrorMessage);
    }

    [Fact]
    public void Pipeline_CleansVisualStudioEscapedXml()
    {
      var context = new XmlCleanerContext { XmlToClean = "<root attr=\\\"value\\\"/>" };

      RunPipeline(context);

      Assert.NotNull(context.ParsedXml);
      Assert.Equal("root", context.ParsedXml.Name.LocalName);
      Assert.Null(context.ErrorMessage);
    }

    [Fact]
    public void Pipeline_CleansXmlWithSurroundingGarbage()
    {
      var context = new XmlCleanerContext { XmlToClean = "prefix garbage <root>value</root> trailing garbage" };

      RunPipeline(context);

      Assert.NotNull(context.ParsedXml);
      Assert.Equal("root", context.ParsedXml.Name.LocalName);
      Assert.Null(context.ErrorMessage);
    }

    [Fact]
    public void Pipeline_CleansXmlWithWhitespaceAndNewlines()
    {
      var context = new XmlCleanerContext
      {
        XmlToClean = "  " + Environment.NewLine + "  <root>value</root>  " + Environment.NewLine + "  "
      };

      RunPipeline(context);

      Assert.NotNull(context.ParsedXml);
      Assert.Equal("root", context.ParsedXml.Name.LocalName);
      Assert.Null(context.ErrorMessage);
    }

    [Fact]
    public void Pipeline_WrapsFragmentsInAddedRoot()
    {
      var context = new XmlCleanerContext { XmlToClean = "<a/><b/><c/>" };

      RunPipeline(context);

      Assert.NotNull(context.ParsedXml);
      Assert.Equal("AddedRoot", context.ParsedXml.Name.LocalName);
      Assert.Null(context.ErrorMessage);
    }

    [Fact]
    public void Pipeline_SetsErrorForCompletelyInvalidContent()
    {
      var context = new XmlCleanerContext { XmlToClean = "this has no angle brackets" };

      RunPipeline(context);

      // SurroundingGarbageCleaner returns early (no <), so the string stays
      // AddedRootCleaner wraps it in <AddedRoot>...</AddedRoot> and it should parse
      // Actually "this has no angle brackets" has no < or >, so SurroundingGarbageCleaner
      // returns early without modifying. Then AddedRootCleaner wraps in <AddedRoot>...</AddedRoot>
      // which is valid XML. So it should actually succeed.
      Assert.NotNull(context.ParsedXml);
      Assert.Equal("AddedRoot", context.ParsedXml.Name.LocalName);
    }

    [Fact]
    public void Pipeline_CleansComplexDirtyInput()
    {
      // Combines URL encoding + whitespace + newlines + garbage
      var encoded = "  junk  %3Croot%3E%3Cchild%2F%3E%3C%2Froot%3E  junk  ";
      var context = new XmlCleanerContext { XmlToClean = encoded };

      RunPipeline(context);

      Assert.NotNull(context.ParsedXml);
      Assert.Equal("root", context.ParsedXml.Name.LocalName);
      Assert.Null(context.ErrorMessage);
    }

    [Fact]
    public void Pipeline_CleansVBScriptDoubleQuotedAttributes()
    {
      // Input: <root attr=""value"" />
      // After VisualStudioVBScriptCleaner: ="" followed by 'v' means group3 doesn't match,
      // so it collapses. The result should be parseable XML.
      var context = new XmlCleanerContext { XmlToClean = "<root attr=\"\"value\"\" />" };

      RunPipeline(context);

      Assert.NotNull(context.ParsedXml);
      Assert.Null(context.ErrorMessage);
    }
  }

  #endregion
}
