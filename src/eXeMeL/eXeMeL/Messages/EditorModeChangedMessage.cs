using eXeMeL.ViewModel;

namespace eXeMeL.Messages
{
  public class EditorModeChangedMessage
  {
    public EditorMode EditorMode { get; }



    public EditorModeChangedMessage(EditorMode newMode)
    {
      this.EditorMode = newMode;
    }
  }
}