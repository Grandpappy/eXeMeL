using eXeMeL.Messages;
using eXeMeL.Model;
using eXeMeL.ViewModel.XmlCleaners;
using eXeMeL.ViewModel.JsonCleaners;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using eXeMeL.Utilities;
using System.Collections.ObjectModel;
using Microsoft.Win32;


namespace eXeMeL.ViewModel
{
  public class EditorViewModel : ObservableObject
  {
    private bool _IsContentFromFile;
    private TextDocument _Document;
    private string _FilePath;
    private string _FileName;
    private DocumentContentType _contentType = DocumentContentType.Xml;

    private List<XmlCleanerBase> XmlCleaners;
    private List<JsonCleanerBase> JsonCleaners;
    public ObservableCollection<DocumentSnapshot> Snapshots { get; set; }


    public TextDocument Document
    {
      get { return _Document; }
      private set
      {
        SetProperty(ref _Document, value);
        this.FindViewModel.Document = this.Document;
      }
    }


    public DocumentContentType ContentType
    {
      get => _contentType;
      set
      {
        if (SetProperty(ref _contentType, value))
        {
          WeakReferenceMessenger.Default.Send(new ContentTypeChangedMessage(value));
        }
      }
    }


    public bool IsContentFromFile
    {
      get { return _IsContentFromFile; }
      private set { SetProperty(ref _IsContentFromFile, value); }
    }



    public string FilePath
    {
      get { return _FilePath; }
      private set { SetProperty(ref _FilePath, value); }
    }



    public string FileName
    {
      get { return _FileName; }
      private set { SetProperty(ref _FileName, value); }
    }



    public Settings Settings { get; private set; }
    public ICommand CopyCommand { get; private set; }
    public ICommand RefreshCommand { get; private set; }
    public ICommand CopyDecodedXmlFromCursorPositionCommand { get; private set; }
    public ICommand DelveIntoDecodedXmlFromCursorPositionCommand { get; private set; }
    public ICommand CreateSnapshotCommand { get; private set; }
    public ICommand ChangeToSnapshotCommand { get; private set; }
    public ICommand SaveCommand { get; private set; }
    public ICommand OpenCommand { get; private set; }
    public EditorFindViewModel FindViewModel { get; private set; }
    public event EventHandler RefreshComplete;
    public TextViewPosition CaretPosition { get; set; }


    public EditorViewModel()
    {
      this.CopyCommand = new RelayCommand(CopyCommand_Execute);
      this.RefreshCommand = new RelayCommand(RefreshCommand_Execute);
      this.CopyDecodedXmlFromCursorPositionCommand = new RelayCommand(CopyDecodedXmlFromCursorPositionCommand_Execute, CopyDecodedXmlFromCursorPositionCommand_CanExecute);
      this.DelveIntoDecodedXmlFromCursorPositionCommand = new RelayCommand(DelveIntoDecodedXmlFromCursorPositionCommand_Execute);
      this.CreateSnapshotCommand = new RelayCommand(CreateSnapshotCommand_Execute);
      this.ChangeToSnapshotCommand = new RelayCommand<DocumentSnapshot>(ChangeToSnapshotCommand_Execute);
      this.SaveCommand = new RelayCommand(SaveCommand_Execute);
      this.OpenCommand = new RelayCommand(OpenCommand_Execute);
      this.Snapshots = new ObservableCollection<DocumentSnapshot>();
      this.FindViewModel = new EditorFindViewModel();

      this.XmlCleaners = new List<XmlCleanerBase>()
      {
        new UrlEncodingCleaner(),
        new TrimCleaner(),
        new NewLineCleaner(),
        new SurroundingGarbageCleaner(),
        new VisualStudioCleaner(),
        new VisualStudioVBScriptCleaner(),
        new AddedRootCleaner(),
        new FormatCleaner()
      };

      this.JsonCleaners = new List<JsonCleanerBase>()
      {
        new JsonUrlEncodingCleaner(),
        new JsonTrimCleaner(),
        new JsonEscapeCleaner(),
        new JsonSurroundingGarbageCleaner(),
        new JsonFormatCleaner()
      };

      if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
      {
        this.Document = new TextDocument() { Text = "<Root IsValue=\"true\"><FirstChild Name=\"Robby\" Address=\"1521 Greenway Dr\"><Toys>All of them</Toys></FirstChild></Root>" };
        this.Snapshots.Add(new DocumentSnapshot(new TextDocument(), "Original"));
        this.Snapshots.Add(new DocumentSnapshot(new TextDocument(), "1"));
        this.Snapshots.Add(new DocumentSnapshot(new TextDocument(), "Current"));
      }
      else
      {
        this.Document = new TextDocument();
      }
    }



    public EditorViewModel(Settings settings)
      : this()
    {
      this.Settings = settings;
    }



    public async Task<string> CleanContentAsync(string text)
    {
      // Detect content type from the raw text
      ContentType = ContentTypeDetector.Detect(text);

      if (ContentType == DocumentContentType.Json)
        return await CleanJsonAsync(text);
      else
        return await CleanXmlAsync(text);
    }



    private async Task<string> CleanXmlAsync(string xml)
    {
      if (!XmlShouldBeCleaned(xml))
        return xml;

      var context = new XmlCleanerContext() { XmlToClean = xml };

      await Task.Run(() =>
      {
        foreach (var cleaner in this.XmlCleaners)
        {
          cleaner.CleanXml(context);

          if (!string.IsNullOrWhiteSpace(context.ErrorMessage))
          {
            WeakReferenceMessenger.Default.Send(new DisplayApplicationStatusMessage(context.ErrorMessage));
            return;
          }
        }

        if (context.ParsedXml != null)
          WeakReferenceMessenger.Default.Send(new DisplayApplicationStatusMessage("XML parsed correctly"));
        else
          WeakReferenceMessenger.Default.Send(new DisplayApplicationStatusMessage("Text was not able to be parsed into XML"));
      });

      return context.XmlToClean;
    }



    private async Task<string> CleanJsonAsync(string json)
    {
      var context = new JsonCleanerContext() { TextToClean = json };

      await Task.Run(() =>
      {
        foreach (var cleaner in this.JsonCleaners)
        {
          cleaner.Clean(context);

          if (!string.IsNullOrWhiteSpace(context.ErrorMessage))
          {
            WeakReferenceMessenger.Default.Send(new DisplayApplicationStatusMessage(context.ErrorMessage));
            return;
          }
        }

        if (context.IsParsedSuccessfully)
          WeakReferenceMessenger.Default.Send(new DisplayApplicationStatusMessage("JSON parsed correctly"));
        else
          WeakReferenceMessenger.Default.Send(new DisplayApplicationStatusMessage("Text was not able to be parsed as JSON"));
      });

      return context.TextToClean;
    }



    private bool XmlShouldBeCleaned(string xml)
    {
      int firstLessThanIndex = xml.IndexOf('<');
      int lastGreaterThanIndex = xml.LastIndexOf('>');

      if (firstLessThanIndex < 0 && lastGreaterThanIndex < 0)
        return true;

      if (firstLessThanIndex < 0)
        return false;

      if (lastGreaterThanIndex < 0)
        return false;

      if (firstLessThanIndex < lastGreaterThanIndex)
        return true;
      else
        return false;
    }



    private async Task SetDocumentTextFromClipboardAsync()
    {
      WeakReferenceMessenger.Default.Send(new UnselectTextInEditorMessage());
      var text = await CleanContentAsync(Clipboard.GetText());

      this.IsContentFromFile = false;
      this.FilePath = null;
      this.FileName = "From Clipboard";

      ReplaceOldDocumentWithNewDocument(text);

      WeakReferenceMessenger.Default.Send(new DisplayToolInformationMessage(string.Empty));
      WeakReferenceMessenger.Default.Send(new DocumentRefreshCompleted(text));

      var handler = this.RefreshComplete;
      handler?.Invoke(this, EventArgs.Empty);
    }



    private void ResetSnapshots()
    {
      this.Snapshots.Clear();
      this.Snapshots.Add(new DocumentSnapshot(this.Document));
      RenameAllSnapshots();
    }



    private void ReplaceOldDocumentWithNewDocument(string newText)
    {
      this.Document = new TextDocument() { Text = newText };
      ResetSnapshots();

      WeakReferenceMessenger.Default.Send(new DocumentTextReplacedMessage());
    }



    private void ReplaceCurrentDocumentText(string newText)
    {
      this.Document.Text = newText;
      WeakReferenceMessenger.Default.Send(new DocumentTextReplacedMessage());
    }



    private async void RefreshCommand_Execute()
    {
      await SetDocumentTextFromClipboardAsync();
    }



    private async void CopyDecodedXmlFromCursorPositionCommand_Execute()
    {
      var decodedText = await GetDecodedTextAtCaretPositionAsync();
      if (decodedText != null)
      {
        Clipboard.SetText(decodedText);
      }
    }



    private bool CopyDecodedXmlFromCursorPositionCommand_CanExecute()
    {
      return true;
    }



    private async void DelveIntoDecodedXmlFromCursorPositionCommand_Execute()
    {
      var decodedText = await GetDecodedTextAtCaretPositionAsync();
      if (decodedText != null)
      {
        var cleanedText = await CleanContentAsync(decodedText);
        ClearSnapshotsAfterDocument(this.Document);
        AddNewSnapshotWithNewText(cleanedText);
      }
    }



    private async Task<string> GetDecodedTextAtCaretPositionAsync()
    {
      var searchUtility = new EncodedXmlExtractor(this.Document.Text);
      var caretOffset = this.Document.GetOffset(this.CaretPosition.Location);

      var decodedText = await searchUtility.GetDecodedXmlAroundIndexAsync(caretOffset);
      return decodedText;
    }



    private void CopyCommand_Execute()
    {
      Clipboard.SetText(this.Document.Text);
    }



    public async void OpenFileAsync(string filePath)
    {
      try
      {
        if (!File.Exists(filePath))
          return;

        var fileContents = await LoadFileContentsAsync(filePath);

        WeakReferenceMessenger.Default.Send(new UnselectTextInEditorMessage());

        // Detect content type from file extension first, fall back to content detection
        var ext = Path.GetExtension(filePath)?.ToLowerInvariant();
        ContentType = ext == ".json"
          ? DocumentContentType.Json
          : ContentTypeDetector.Detect(fileContents);

        this.IsContentFromFile = true;
        this.FilePath = filePath;
        this.FileName = Path.GetFileName(filePath);

        ReplaceOldDocumentWithNewDocument(fileContents);

        RaiseRefreshComplete();

        WeakReferenceMessenger.Default.Send(new DisplayApplicationStatusMessage("File opened: " + Path.GetFileName(filePath)));
      }
      catch (Exception ex)
      {
        WeakReferenceMessenger.Default.Send(new DisplayApplicationStatusMessage("Error opening file: " + ex.Message));
      }
    }



    private void RaiseRefreshComplete()
    {
      var handler = RefreshComplete;
      handler?.Invoke(this, EventArgs.Empty);
    }



    #region Snapshot Handling



    private void CreateSnapshotCommand_Execute()
    {
      AddNewSnapshotOfCurrentDocumentText();
    }



    private void ChangeToSnapshotCommand_Execute(DocumentSnapshot snapshot)
    {
      ChangeToSnapshot(snapshot);
    }



    private async void SaveCommand_Execute()
    {
      if (this.IsContentFromFile)
      {
        await File.WriteAllTextAsync(this.FilePath, this.Document.Text);
      }
      else
      {
        var defaultExt = ContentType == DocumentContentType.Json ? ".json" : ".xml";
        var filter = ContentType == DocumentContentType.Json
          ? "JSON files (.json)|*.json|All files (*.*)|*.*"
          : "XML documents (.xml)|*.xml|All files (*.*)|*.*";

        var saveDialog = new SaveFileDialog
        {
          DefaultExt = defaultExt,
          Filter = filter
        };

        if (saveDialog.ShowDialog() == true)
        {
          this.FilePath = saveDialog.FileName;
          this.FileName = Path.GetFileName(this.FilePath);
          this.IsContentFromFile = true;

          await File.WriteAllTextAsync(this.FilePath, this.Document.Text);
        }
      }
    }



    private void OpenCommand_Execute()
    {
      var openDialog = new OpenFileDialog
      {
        DefaultExt = ".xml",
        Filter = "XML and JSON files|*.xml;*.json|XML documents (.xml)|*.xml|JSON files (.json)|*.json|All files (*.*)|*.*"
      };

      if (openDialog.ShowDialog() == true)
      {
        OpenFileAsync(openDialog.FileName);
      }
    }



    private async Task<string> LoadFileContentsAsync(string filePath)
    {
      return await Task.Run(() => File.ReadAllText(filePath));
    }



    private void AddNewSnapshotOfCurrentDocumentText()
    {
      AddNewSnapshotWithNewText(this.Document.Text);
    }



    private void AddNewSnapshotWithNewText(string text)
    {
      WeakReferenceMessenger.Default.Send(new UnselectTextInEditorMessage());

      this.Document = new TextDocument() { Text = text };
      this.Snapshots.Add(new DocumentSnapshot(this.Document));

      RenameAllSnapshots();
    }



    private void RenameAllSnapshots()
    {
      var index = 0;
      foreach (var s in this.Snapshots)
      {
        if (index == 0)
        {
          s.Identifier = "Original";
        }
        else
        if (index == this.Snapshots.Count - 1)
        {
          s.Identifier = "Current";
        }
        else
        {
          s.Identifier = index.ToString();
        }

        index += 1;
      }
    }



    private void ChangeToSnapshot(DocumentSnapshot snapshot)
    {
      this.Document = snapshot.Document;
    }



    internal void ClearSnapshotsAfterDocument(TextDocument textDocument)
    {
      if (textDocument == null || this.Snapshots.Count <= 1 || textDocument == this.Snapshots.Last().Document)
        return;

      var snapshot = this.Snapshots.FirstOrDefault(x => x.Document == textDocument);
      if (snapshot == null)
        return;

      var indexOfItemToRemove = this.Snapshots.IndexOf(snapshot) + 1;
      var itemsToRemove = new List<DocumentSnapshot>();
      for (var i = indexOfItemToRemove; i < this.Snapshots.Count; i++)
      {
        itemsToRemove.Add(this.Snapshots.ElementAt(i));
      }

      itemsToRemove.ForEach(x => this.Snapshots.Remove(x));
    }


    #endregion


  }
}
