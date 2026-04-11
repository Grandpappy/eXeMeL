using eXeMeL.Model;

namespace eXeMeL.Messages
{
  public class ContentTypeChangedMessage
  {
    public DocumentContentType ContentType { get; }

    public ContentTypeChangedMessage(DocumentContentType contentType)
    {
      ContentType = contentType;
    }
  }
}
