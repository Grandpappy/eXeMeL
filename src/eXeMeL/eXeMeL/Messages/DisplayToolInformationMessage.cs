namespace eXeMeL.Messages
{
  public class DisplayToolInformationMessage
  {
    public string Information { get; private set; }

    public DisplayToolInformationMessage(string message)
    {
      this.Information = message;
    }
  }
}