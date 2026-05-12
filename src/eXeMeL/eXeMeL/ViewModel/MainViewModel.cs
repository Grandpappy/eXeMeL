using System;
using System.Collections.Generic;
using System.Windows.Input;
using eXeMeL.Messages;
using eXeMeL.Model;
using eXeMeL.ViewModel.JsonUtility;
using eXeMeL.ViewModel.MarkdownUtility;
using eXeMeL.ViewModel.YamlUtility;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace eXeMeL.ViewModel
{
  public class MainViewModel : ObservableObject
  {
    private static readonly double[] ScaleLevels = { 0.75, 0.80, 0.90, 1.00, 1.10, 1.20, 1.25, 1.30, 1.40, 1.50, 1.60, 1.70, 1.75, 1.80, 1.90, 2.00 };
    public IReadOnlyList<double> ZoomLevels { get; } = ScaleLevels;

    private string _status;
    private SyntaxHighlightingManager _highlightingManager;
    private ApplicationThemeManager _applicationThemeManager;
    private string _toolInformation;
    private EditorMode _editorMode;


    public Settings Settings { get; private set; }
    public EditorViewModel Editor { get; private set; }
    public XmlUtilityViewModel XmlUtility { get; private set; }
    public JsonUtilityViewModel JsonUtility { get; private set; }
    public YamlUtilityViewModel YamlUtility { get; private set; }
    public MarkdownUtilityViewModel MarkdownUtility { get; private set; }
    public string Status { get { return this._status; } private set { SetProperty(ref this._status, value); } }
    public string ToolInformation { get { return this._toolInformation; } set { SetProperty(ref this._toolInformation, value); } }
    public SyntaxHighlightingManager HighlightingManager { get { return this._highlightingManager; } private set { SetProperty(ref this._highlightingManager, value); } }
    public ApplicationThemeManager ApplicationThemeManager { get { return this._applicationThemeManager; } private set { SetProperty(ref this._applicationThemeManager, value); } }
    public ICommand ToggleEditorModeCommand { get; private set; }
    public ICommand ZoomInCommand { get; private set; }
    public ICommand ZoomOutCommand { get; private set; }
    public ICommand ResetZoomCommand { get; private set; }
    public EditorMode EditorMode { get { return this._editorMode; } private set { SetProperty(ref this._editorMode, value); } }



    public MainViewModel()
    {
      SettingsMigrator.MigrateIfNeeded();
      this.Settings = SettingsIO.LoadSettings<Settings>();
      this.HighlightingManager = new SyntaxHighlightingManager(this.Settings);
      this.ApplicationThemeManager = new ApplicationThemeManager(this.Settings);
      this.Editor = new EditorViewModel(this.Settings);
      this.XmlUtility = new XmlUtilityViewModel(this.Settings);
      this.JsonUtility = new JsonUtilityViewModel(this.Settings);
      this.YamlUtility = new YamlUtilityViewModel(this.Settings);
      this.MarkdownUtility = new MarkdownUtilityViewModel(this.Settings);
      this.ToggleEditorModeCommand = new RelayCommand(ToggleEditorModeCommand_Execute);
      this.ZoomInCommand = new RelayCommand(ZoomIn_Execute, () => Settings.AppScale < ScaleLevels[ScaleLevels.Length - 1]);
      this.ZoomOutCommand = new RelayCommand(ZoomOut_Execute, () => Settings.AppScale > ScaleLevels[0]);
      this.ResetZoomCommand = new RelayCommand(() => Settings.AppScale = 1.0);
      Settings.PropertyChanged += Settings_PropertyChanged;
      WeakReferenceMessenger.Default.Register<ApplicationClosingMessage>(this, (r, m) => HandleApplicationClosingMessage(m));
      WeakReferenceMessenger.Default.Register<DisplayApplicationStatusMessage>(this, (r, m) => HandleDisplayApplicationStatusMessage(m));
      WeakReferenceMessenger.Default.Register<DisplayToolInformationMessage>(this, (r, m) => HandleDisplayToolInformationMessage(m));
      WeakReferenceMessenger.Default.Register<DocumentRefreshCompleted>(this, (r, m) => HandleDocumentRefreshCompletedMessage(m));
    }



    private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(Settings.AppScale))
      {
        ((RelayCommand)ZoomInCommand).NotifyCanExecuteChanged();
        ((RelayCommand)ZoomOutCommand).NotifyCanExecuteChanged();
      }
    }

    private void ZoomIn_Execute()
    {
      int idx = Array.FindLastIndex(ScaleLevels, l => l <= Settings.AppScale + 0.001);
      if (idx < ScaleLevels.Length - 1)
        Settings.AppScale = ScaleLevels[idx + 1];
    }

    private void ZoomOut_Execute()
    {
      int idx = Array.FindIndex(ScaleLevels, l => l >= Settings.AppScale - 0.001);
      if (idx > 0)
        Settings.AppScale = ScaleLevels[idx - 1];
    }


    private void HandleDocumentRefreshCompletedMessage(DocumentRefreshCompleted message)
    {
      switch (this.Editor.ContentType)
      {
        case DocumentContentType.Json:
          this.JsonUtility.DocumentText = message.NewDocumentText;
          break;
        case DocumentContentType.Yaml:
          this.YamlUtility.DocumentText = message.NewDocumentText;
          break;
        case DocumentContentType.Xml:
          this.XmlUtility.DocumentText = message.NewDocumentText;
          break;
        case DocumentContentType.Markdown:
          this.MarkdownUtility.DocumentText = message.NewDocumentText;
          break;
        // Text: no utility to feed
      }
    }



    private void ToggleEditorModeCommand_Execute()
    {
      if (this.EditorMode == EditorMode.Editor)
      {
        // Don't toggle to utility for Text — no viewer
        if (this.Editor.ContentType == DocumentContentType.Text)
          return;

        var text = this.Editor.Document.Text;
        switch (this.Editor.ContentType)
        {
          case DocumentContentType.Json:
            this.JsonUtility.DocumentText = text;
            break;
          case DocumentContentType.Yaml:
            this.YamlUtility.DocumentText = text;
            break;
          case DocumentContentType.Markdown:
            this.MarkdownUtility.DocumentText = text;
            break;
          default:
            this.XmlUtility.DocumentText = text;
            break;
        }

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
