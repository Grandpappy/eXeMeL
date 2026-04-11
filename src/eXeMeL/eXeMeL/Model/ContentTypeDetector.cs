namespace eXeMeL.Model
{
  public static class ContentTypeDetector
  {
    public static DocumentContentType Detect(string content)
    {
      if (string.IsNullOrWhiteSpace(content))
        return DocumentContentType.Xml;

      foreach (var c in content)
      {
        if (char.IsWhiteSpace(c))
          continue;

        return c switch
        {
          '{' or '[' => DocumentContentType.Json,
          _ => DocumentContentType.Xml
        };
      }

      return DocumentContentType.Xml;
    }

    public static DocumentContentType DetectFromFileExtension(string filePath)
    {
      if (string.IsNullOrEmpty(filePath))
        return DocumentContentType.Xml;

      var ext = System.IO.Path.GetExtension(filePath)?.ToLowerInvariant();
      return ext switch
      {
        ".json" => DocumentContentType.Json,
        _ => DocumentContentType.Xml
      };
    }
  }
}
