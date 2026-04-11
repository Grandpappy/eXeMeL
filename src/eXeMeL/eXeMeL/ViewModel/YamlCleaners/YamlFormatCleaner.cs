using System;
using System.IO;
using YamlDotNet.RepresentationModel;

namespace eXeMeL.ViewModel.YamlCleaners
{
  internal class YamlFormatCleaner : YamlCleanerBase
  {
    public override void Clean(YamlCleanerContext context)
    {
      try
      {
        var yaml = new YamlStream();
        using (var reader = new StringReader(context.TextToClean))
        {
          yaml.Load(reader);
        }

        using (var writer = new StringWriter())
        {
          yaml.Save(writer, assignAnchors: false);
          var result = writer.ToString();
          // YamlDotNet appends "..." document end marker — strip it if unwanted
          if (result.EndsWith("..." + Environment.NewLine))
            result = result[..^(3 + Environment.NewLine.Length)];
          context.TextToClean = result.TrimEnd();
        }

        context.IsParsedSuccessfully = true;
      }
      catch (Exception ex)
      {
        context.ErrorMessage = $"YAML parse error: {ex.Message}";
      }
    }
  }
}
