using eXeMeL.ViewModel.JsonCleaners;
using Xunit;

namespace eXeMeL.Tests.JsonCleaners
{
  public class JsonEscapeCleanerTests
  {
    [Fact]
    public void Clean_ValidJsonWithWindowsPaths_DoesNotCorrupt()
    {
      // Bug: JSON with Windows paths (\\) followed by " was triggering
      // the escape cleaner, which then corrupted the content by unescaping
      // valid JSON escape sequences like \\ → \ and \n → newline.
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
      // This IS the case the cleaner should handle: JSON pasted from a
      // C# string literal where the entire structure is escaped.
      var escaped = "{\\\"name\\\":\\\"test\\\",\\\"value\\\":123}";

      var context = new JsonCleanerContext { TextToClean = escaped };
      var cleaner = new JsonEscapeCleaner();

      cleaner.Clean(context);

      Assert.Contains("\"name\"", context.TextToClean);
      Assert.Contains("\"test\"", context.TextToClean);
    }
  }
}
