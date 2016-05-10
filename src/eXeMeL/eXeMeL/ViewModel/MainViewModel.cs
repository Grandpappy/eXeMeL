using System.Windows.Input;
using eXeMeL.Messages;
using eXeMeL.Model;
using GalaSoft.MvvmLight;
using ICSharpCode.AvalonEdit.Document;
using System.Xml.Linq;
using GalaSoft.MvvmLight.Command;

namespace eXeMeL.ViewModel
{
  public class MainViewModel : ViewModelBase
  {
    private string _status;
    private SyntaxHighlightingManager _highlightingManager;
    private ApplicationThemeManager _applicationThemeManager;
    private string _toolInformation;
    private EditorMode _editorMode;


    public Settings Settings { get; private set; }
    public EditorViewModel Editor { get; private set; }
    public XmlUtilityViewModel XmlUtility { get; private set; }
    public string Status { get { return this._status; } private set { Set(() => this.Status, ref this._status, value); } }
    public string ToolInformation { get { return this._toolInformation; } set { Set(() => this.ToolInformation, ref this._toolInformation, value); } }
    public SyntaxHighlightingManager HighlightingManager { get { return this._highlightingManager; } private set { Set(() => this.HighlightingManager, ref this._highlightingManager, value); } }
    public ApplicationThemeManager ApplicationThemeManager { get { return this._applicationThemeManager; } private set { Set(() => this.ApplicationThemeManager, ref this._applicationThemeManager, value); } }
    public ICommand ToggleEditorModeCommand { get; private set; }
    public EditorMode EditorMode { get { return this._editorMode; } private set { Set(() => this.EditorMode, ref this._editorMode, value); } }
    //public XElement ParsedXml { get; set; }



    public MainViewModel()
    {
      this.Settings = SettingsIO.LoadSettings<Settings>();
      this.HighlightingManager = new SyntaxHighlightingManager(this.Settings);
      this.ApplicationThemeManager = new ApplicationThemeManager(this.Settings);
      this.Editor = new EditorViewModel(this.Settings);
      this.XmlUtility = new XmlUtilityViewModel(this.Settings);
      this.ToggleEditorModeCommand = new RelayCommand(ToggleEditorModeCommand_Execute);
      this.MessengerInstance.Register<ApplicationClosingMessage>(this, HandleApplicationClosingMessage);
      this.MessengerInstance.Register<DisplayApplicationStatusMessage>(this, HandleDisplayApplicationStatusMessage);
      this.MessengerInstance.Register<DisplayToolInformationMessage>(this, HandleDisplayToolInformationMessage);
      this.MessengerInstance.Register<DocumentRefreshCompleted>(this, HandleDocumentRefreshCompletedMessage);
    }



    private void HandleDocumentRefreshCompletedMessage(DocumentRefreshCompleted message)
    {
      //this.EditorMode = EditorMode.Editor;
      this.XmlUtility.DocumentText = message.NewDocumentText;
    }



    private void ToggleEditorModeCommand_Execute()
    {
      if (this.EditorMode == EditorMode.Editor)
      {
        this.XmlUtility.DocumentText = this.Editor.Document.Text;
        this.EditorMode = EditorMode.XmlUtility;
      }
      else
      {
        this.EditorMode = EditorMode.Editor;
      }

      this.MessengerInstance.Send(new EditorModeChangedMessage(this.EditorMode));
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

  public enum EditorMode { Editor, XmlUtility }
}