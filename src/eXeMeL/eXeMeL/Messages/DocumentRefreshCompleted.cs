namespace eXeMeL.Messages
{
  public class DocumentRefreshCompleted
  {
    public string NewDocumentText { get; }

    public DocumentRefreshCompleted(string newDocumentText)
    {
      this.NewDocumentText = newDocumentText;
    }
  }
}