using eXeMeL.ViewModel.YamlCleaners;
using Xunit;

namespace eXeMeL.Tests.YamlCleaners
{
  #region YamlTrimCleaner

  public class YamlTrimCleanerTests
  {
    [Fact]
    public void Clean_TrimsWhitespace()
    {
      var context = new YamlCleanerContext { TextToClean = "  name: test  \n" };
      new YamlTrimCleaner().Clean(context);
      Assert.Equal("name: test", context.TextToClean);
    }

    [Fact]
    public void Clean_NoWhitespace_NoChange()
    {
      var yaml = "key: value";
      var context = new YamlCleanerContext { TextToClean = yaml };
      new YamlTrimCleaner().Clean(context);
      Assert.Equal(yaml, context.TextToClean);
    }
  }

  #endregion

  #region YamlFormatCleaner

  public class YamlFormatCleanerTests
  {
    [Fact]
    public void Clean_ValidYaml_ParsesSuccessfully()
    {
      var yaml = "name: test\nversion: 1.0";
      var context = new YamlCleanerContext { TextToClean = yaml };
      new YamlFormatCleaner().Clean(context);
      Assert.True(context.IsParsedSuccessfully);
      Assert.Contains("name:", context.TextToClean);
      Assert.Contains("version:", context.TextToClean);
    }

    [Fact]
    public void Clean_NestedYaml_FormatsCorrectly()
    {
      var yaml = "server:\n  host: localhost\n  port: 8080";
      var context = new YamlCleanerContext { TextToClean = yaml };
      new YamlFormatCleaner().Clean(context);
      Assert.True(context.IsParsedSuccessfully);
      Assert.Contains("host:", context.TextToClean);
    }

    [Fact]
    public void Clean_YamlWithDocumentSeparator_ParsesSuccessfully()
    {
      var yaml = "---\nkey: value";
      var context = new YamlCleanerContext { TextToClean = yaml };
      new YamlFormatCleaner().Clean(context);
      Assert.True(context.IsParsedSuccessfully);
    }

    [Fact]
    public void Clean_InvalidYaml_SetsErrorMessage()
    {
      var yaml = ":\n  - :\n    - : [invalid";
      var context = new YamlCleanerContext { TextToClean = yaml };
      new YamlFormatCleaner().Clean(context);
      Assert.False(context.IsParsedSuccessfully);
      Assert.Contains("YAML parse error", context.ErrorMessage);
    }

    [Fact]
    public void Clean_StripsDocumentEndMarker()
    {
      var yaml = "name: test";
      var context = new YamlCleanerContext { TextToClean = yaml };
      new YamlFormatCleaner().Clean(context);
      Assert.DoesNotContain("...", context.TextToClean);
    }

    [Fact]
    public void Clean_YamlSequence_FormatsCorrectly()
    {
      var yaml = "items:\n- one\n- two\n- three";
      var context = new YamlCleanerContext { TextToClean = yaml };
      new YamlFormatCleaner().Clean(context);
      Assert.True(context.IsParsedSuccessfully);
      Assert.Contains("one", context.TextToClean);
      Assert.Contains("three", context.TextToClean);
    }
  }

  #endregion
}
