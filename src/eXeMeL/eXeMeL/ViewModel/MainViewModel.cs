using eXeMeL.Messages;
using eXeMeL.Model;
using GalaSoft.MvvmLight;
using ICSharpCode.AvalonEdit.Document;
using System.Xml.Linq;

namespace eXeMeL.ViewModel
{
  public class MainViewModel : ViewModelBase
  {
    private string _Status;
    private XElement _ParsedXml;
    private SyntaxHighlightingManager _HighlightingManager;
    private ApplicationThemeManager _ApplicationThemeManager;
    private string _toolInformation;


    public Settings Settings { get; private set; }
    public EditorViewModel Editor { get; private set; }
    public string Status { get { return _Status; } private set { this.Set(() => Status, ref _Status, value); } }
    public string ToolInformation { get { return this._toolInformation; } set { Set(() => this.ToolInformation, ref _toolInformation, value); } }
    public SyntaxHighlightingManager HighlightingManager { get { return _HighlightingManager; } private set { Set(() => HighlightingManager, ref _HighlightingManager, value); } }
    public ApplicationThemeManager ApplicationThemeManager { get { return _ApplicationThemeManager; } private set { Set(() => ApplicationThemeManager, ref _ApplicationThemeManager, value); } }



    public XElement ParsedXml
    {
      get { return _ParsedXml; }
      set { _ParsedXml = value; }
    }



    public MainViewModel()
    {
      this.Settings = SettingsIO.LoadSettings<Settings>();
      this.HighlightingManager = new SyntaxHighlightingManager(this.Settings);
      this.ApplicationThemeManager = new ApplicationThemeManager(this.Settings);
      this.Editor = new EditorViewModel(this.Settings);
      this.MessengerInstance.Register<ApplicationClosingMessage>(this, HandleApplicationClosingMessage);
      this.MessengerInstance.Register<DisplayApplicationStatusMessage>(this, HandleDisplayApplicationStatusMessage);
      this.MessengerInstance.Register<DisplayToolInformationMessage>(this, HandleDisplayToolInformationMessage);
    }



    private void HandleApplicationClosingMessage(ApplicationClosingMessage message)
    {
      SettingsIO.SaveSettings(this.Settings);
    }



    private void HandleDisplayApplicationStatusMessage(DisplayApplicationStatusMessage message)
    {
      this.Status = message.NewStatus;
    }



    private void HandleDisplayToolInformationMessage(DisplayToolInformationMessage message)
    {
      this.ToolInformation = message.Information;
    }

  }
}