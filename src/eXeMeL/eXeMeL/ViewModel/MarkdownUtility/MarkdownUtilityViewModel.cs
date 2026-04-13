using CommunityToolkit.Mvvm.ComponentModel;
using eXeMeL.Model;

namespace eXeMeL.ViewModel.MarkdownUtility
{
  public class MarkdownUtilityViewModel : ObservableObject
  {
    public Settings Settings { get; }
    private string _documentText;

    public MarkdownUtilityViewModel(Settings settings)
    {
      this.Settings = settings;
    }

    public string DocumentText
    {
      get => _documentText;
      set => SetProperty(ref _documentText, value);
    }
  }
}
