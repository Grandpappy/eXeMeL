using System.Windows.Input;
using eXeMeL.Messages;
using eXeMeL.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using ICSharpCode.AvalonEdit.Document;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace eXeMeL.ViewModel
{
  public class MainViewModel : ObservableObject
  {
    private string _status;
    private SyntaxHighlightingManager _highlightingManager;
    private ApplicationThemeManager _applicationThemeManager;
    private string _toolInformation;
    private EditorMode _editorMode;


    public Settings Settings { get; private set; }
    public EditorViewModel Editor { get; private set; }
    public XmlUtilityViewModel XmlUtility { get; private set; }
    public string Status { get { return this._status; } private set { SetProperty(ref this._status, value); } }
    public string ToolInformation { get { return this._toolInformation; } set { SetProperty(ref this._toolInformation, value); } }
    public SyntaxHighlightingManager HighlightingManager { get { return this._highlightingManager; } private set { SetProperty(ref this._highlightingManager, value); } }
    public ApplicationThemeManager ApplicationThemeManager { get { return this._applicationThemeManager; } private set { SetProperty(ref this._applicationThemeManager, value); } }
    public ICommand ToggleEditorModeCommand { get; private set; }
    public EditorMode EditorMode { get { return this._editorMode; } private set { SetProperty(ref this._editorMode, value); } }
    //public XElement ParsedXml { get; set; }



    public MainViewModel()
    {
      SettingsMigrator.MigrateIfNeeded();
      this.Settings = SettingsIO.LoadSettings<Settings>();
      this.HighlightingManager = new SyntaxHighlightingManager(this.Settings);
      this.ApplicationThemeManager = new ApplicationThemeManager(this.Settings);
      this.Editor = new EditorViewModel(this.Settings);
      this.XmlUtility = new XmlUtilityViewModel(this.Settings);
      this.ToggleEditorModeCommand = new RelayCommand(ToggleEditorModeCommand_Execute);
      WeakReferenceMessenger.Default.Register<ApplicationClosingMessage>(this, (r, m) => HandleApplicationClosingMessage(m));
      WeakReferenceMessenger.Default.Register<DisplayApplicationStatusMessage>(this, (r, m) => HandleDisplayApplicationStatusMessage(m));
      WeakReferenceMessenger.Default.Register<DisplayToolInformationMessage>(this, (r, m) => HandleDisplayToolInformationMessage(m));
      WeakReferenceMessenger.Default.Register<DocumentRefreshCompleted>(this, (r, m) => HandleDocumentRefreshCompletedMessage(m));
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

      WeakReferenceMessenger.Default.Send(new EditorModeChangedMessage(this.EditorMode));
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
