using eXeMeL.ViewModel.JsonCleaners;
using Xunit;

namespace eXeMeL.Tests.JsonCleaners
{
  #region JsonEscapeCleaner

  public class JsonEscapeCleanerTests
  {
    [Fact]
    public void Clean_ValidJsonWithWindowsPaths_DoesNotCorrupt()
    {
      var json = """
        {
          "path": "C:\\Users\\nathan.jones",
          "output": "C:\\TylerDev\\nuget\\packages\\"
        }
        """;

      var context = new JsonCleanerContext { TextToClean = json };
      var cleaner = new JsonEscapeCleaner();

      cleaner.Clean(context);

      Assert.Contains("C:\\\\Users\\\\nathan.jones", context.TextToClean);
      Assert.DoesNotContain("\n", context.TextToClean.Replace("\r\n", "").Replace("\n", "NEWLINE"));
    }

    [Fact]
    public void Clean_DoubleEscapedJson_DoesUnescape()
    {
      var escaped = "{\\\"name\\\":\\\"test\\\",\\\"value\\\":123}";

      var context = new JsonCleanerContext { TextToClean = escaped };
      var cleaner = new JsonEscapeCleaner();

      cleaner.Clean(context);

      Assert.Contains("\"name\"", context.TextToClean);
      Assert.Contains("\"test\"", context.TextToClean);
    }

    [Fact]
    public void Clean_NormalJson_NoChange()
    {
      var json = "{\"name\": \"test\"}";
      var context = new JsonCleanerContext { TextToClean = json };
      new JsonEscapeCleaner().Clean(context);
      Assert.Equal(json, context.TextToClean);
    }

    [Fact]
    public void Clean_DoubleEscapedArray_DoesUnescape()
    {
      var escaped = "[\\\"a\\\",\\\"b\\\"]";
      var context = new JsonCleanerContext { TextToClean = escaped };
      new JsonEscapeCleaner().Clean(context);
      Assert.Contains("\"a\"", context.TextToClean);
    }
  }

  #endregion

  #region JsonUrlEncodingCleaner

  public class JsonUrlEncodingCleanerTests
  {
    [Fact]
    public void Clean_UrlEncodedJson_Decodes()
    {
      var context = new JsonCleanerContext { TextToClean = "%7B%22key%22%3A%22value%22%7D" };
      new JsonUrlEncodingCleaner().Clean(context);
      Assert.Equal("{\"key\":\"value\"}", context.TextToClean);
    }

    [Fact]
    public void Clean_PlainJson_NoChange()
    {
      var json = "{\"key\": 1}";
      var context = new JsonCleanerContext { TextToClean = json };
      new JsonUrlEncodingCleaner().Clean(context);
      Assert.Equal(json, context.TextToClean);
    }
  }

  #endregion

  #region JsonTrimCleaner

  public class JsonTrimCleanerTests
  {
    [Fact]
    public void Clean_TrimsWhitespace()
    {
      var context = new JsonCleanerContext { TextToClean = "  {\"a\":1}  \n" };
      new JsonTrimCleaner().Clean(context);
      Assert.Equal("{\"a\":1}", context.TextToClean);
    }

    [Fact]
    public void Clean_NoWhitespace_NoChange()
    {
      var json = "{\"a\":1}";
      var context = new JsonCleanerContext { TextToClean = json };
      new JsonTrimCleaner().Clean(context);
      Assert.Equal(json, context.TextToClean);
    }
  }

  #endregion

  #region JsonSurroundingGarbageCleaner

  public class JsonSurroundingGarbageCleanerTests
  {
    [Fact]
    public void Clean_RemovesLeadingGarbage()
    {
      var context = new JsonCleanerContext { TextToClean = "some garbage {\"key\": 1}" };
      new JsonSurroundingGarbageCleaner().Clean(context);
      Assert.Equal("{\"key\": 1}", context.TextToClean);
    }

    [Fact]
    public void Clean_RemovesTrailingGarbage()
    {
      var context = new JsonCleanerContext { TextToClean = "{\"key\": 1} trailing text" };
      new JsonSurroundingGarbageCleaner().Clean(context);
      Assert.Equal("{\"key\": 1}", context.TextToClean);
    }

    [Fact]
    public void Clean_RemovesBothSides()
    {
      var context = new JsonCleanerContext { TextToClean = "prefix [1, 2] suffix" };
      new JsonSurroundingGarbageCleaner().Clean(context);
      Assert.Equal("[1, 2]", context.TextToClean);
    }

    [Fact]
    public void Clean_NoGarbage_NoChange()
    {
      var json = "{\"key\": 1}";
      var context = new JsonCleanerContext { TextToClean = json };
      new JsonSurroundingGarbageCleaner().Clean(context);
      Assert.Equal(json, context.TextToClean);
    }

    [Fact]
    public void Clean_NoBracesOrBrackets_NoChange()
    {
      var text = "just plain text";
      var context = new JsonCleanerContext { TextToClean = text };
      new JsonSurroundingGarbageCleaner().Clean(context);
      Assert.Equal(text, context.TextToClean);
    }

    [Fact]
    public void Clean_ObjectBeforeArray_PicksObject()
    {
      var context = new JsonCleanerContext { TextToClean = "garbage {\"a\":1} then [2]" };
      new JsonSurroundingGarbageCleaner().Clean(context);
      Assert.StartsWith("{", context.TextToClean);
      Assert.EndsWith("}", context.TextToClean);
    }
  }

  #endregion

  #region JsonFormatCleaner

  public class JsonFormatCleanerTests
  {
    [Fact]
    public void Clean_CompactJson_PrettyPrints()
    {
      var context = new JsonCleanerContext { TextToClean = "{\"name\":\"test\",\"value\":42}" };
      new JsonFormatCleaner().Clean(context);
      Assert.Contains("\n", context.TextToClean);
      Assert.Contains("  ", context.TextToClean);
      Assert.True(context.IsParsedSuccessfully);
    }

    [Fact]
    public void Clean_InvalidJson_SetsErrorMessage()
    {
      var context = new JsonCleanerContext { TextToClean = "{invalid json" };
      new JsonFormatCleaner().Clean(context);
      Assert.False(context.IsParsedSuccessfully);
      Assert.Contains("JSON parse error", context.ErrorMessage);
    }

    [Fact]
    public void Clean_JsonArray_PrettyPrints()
    {
      var context = new JsonCleanerContext { TextToClean = "[1,2,3]" };
      new JsonFormatCleaner().Clean(context);
      Assert.True(context.IsParsedSuccessfully);
      Assert.Contains("1", context.TextToClean);
    }
  }

  #endregion
}
