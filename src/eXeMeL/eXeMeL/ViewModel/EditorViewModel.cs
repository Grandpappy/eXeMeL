using eXeMeL.Messages;
using eXeMeL.Model;
using eXeMeL.ViewModel.XmlCleaners;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Web;
using eXeMeL.Utilities;
using System.Collections.ObjectModel;
using Microsoft.Win32;


namespace eXeMeL.ViewModel
{
  public class EditorViewModel : ViewModelBase
  {
    private bool _IsContentFromFile;
    private TextDocument _Document;
    private string _FilePath;
    private string _FileName;

    private List<XmlCleanerBase> Cleaners;
    public ObservableCollection<DocumentSnapshot> Snapshots { get; set; }


    public TextDocument Document
    {
      get { return _Document; }
      private set 
      { 
        this.Set(() => Document, ref _Document, value);
        this.FindViewModel.Document = this.Document;
      }
    }


    
    public bool IsContentFromFile
    {
      get { return _IsContentFromFile; }
      private set { this.Set(() => this.IsContentFromFile, ref _IsContentFromFile, value); }
    }


    
    public string FilePath
    {
      get { return _FilePath; }
      private set { this.Set(() => this.FilePath, ref _FilePath, value); }
    }



    public string FileName
    {
      get { return _FileName; }
      private set { this.Set(() => this.FileName, ref _FileName, value); }
    }



    public Settings Settings { get; private set; }
    public ICommand CopyCommand { get; private set; }
    public ICommand RefreshCommand { get; private set; }
    public ICommand CopyDecodedXmlFromCursorPositionCommand { get; private set; }
    public ICommand DelveIntoDecodedXmlFromCursorPositionCommand { get; private set; }
    public ICommand CreateSnapshotCommand { get; private set; }
    public ICommand ChangeToSnapshotCommand { get; private set; }
    public ICommand SaveCommand { get; private set; }
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
      this.Snapshots = new ObservableCollection<DocumentSnapshot>();
      this.FindViewModel = new EditorFindViewModel();
      this.Cleaners = new List<XmlCleanerBase>()
        {
          new TrimCleaner(),
          new NewLineCleaner(),
          new SurroundingGarbageCleaner(),
          new VisualStudioCleaner(),
          new VisualStudioVBScriptCleaner(),
          new AddedRootCleaner(),
          new FormatCleaner()
        };


      if (IsInDesignMode)
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


    
    public async Task<string> CleanXmlIfPossibleAsync(string xml)
    {
      if (!XmlShouldBeCleaned(xml))
        return xml;

      var context = new XmlCleanerContext() { XmlToClean = xml };

      await CleanXml(context);

      return context.XmlToClean;
    }



    private async Task CleanXml(XmlCleanerContext context)
    {
      await Task.Run(() =>
        {
          foreach (var cleaner in this.Cleaners)
          {
            cleaner.CleanXml(context);

            if (!string.IsNullOrWhiteSpace(context.ErrorMessage))
            {
              this.MessengerInstance.Send(new DisplayApplicationStatusMessage(context.ErrorMessage));
              return;
            }
          }

          if (context.ParsedXml != null)
          {
            this.MessengerInstance.Send(new DisplayApplicationStatusMessage("XML parsed correctly"));
          }
          else
          {
            this.MessengerInstance.Send(new DisplayApplicationStatusMessage("Text was not able to be parsed into XML"));
          }
        });

      return;
    }



    private bool XmlShouldBeCleaned(string xml)
    {
      int firstLessThanIndex = xml.IndexOf('<');
      if (firstLessThanIndex < 0)
        return false;

      int lastGreaterThanIndex = xml.LastIndexOf('>');
      if (lastGreaterThanIndex < 0)
        return false;

      if (firstLessThanIndex < lastGreaterThanIndex)
        return true;
      else
        return false;
    }



    private async Task SetDocumentTextFromClipboardAsync()
    {
      // Unselect text, because reloading can cause exceptions if the new text length is shorter than 
      // the range (position and length) of the selected text in the viewer control.
      this.MessengerInstance.Send(new UnselectTextInEditorMessage());
      var text = await CleanXmlIfPossibleAsync(Clipboard.GetText());

      this.IsContentFromFile = false;
      this.FilePath = null;
      this.FileName = "From Clipboard";

      ReplaceOldDocumentWithNewDocument(text);

      this.MessengerInstance.Send(new DisplayToolInformationMessage(string.Empty));
      this.MessengerInstance.Send(new DocumentRefreshCompleted(text));
      
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

      this.MessengerInstance.Send(new DocumentTextReplacedMessage());
    }



    private void ReplaceCurrentDocumentText(string newText)
    {
      this.Document.Text = newText;
      this.MessengerInstance.Send(new DocumentTextReplacedMessage());
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
        var cleanedText = await CleanXmlIfPossibleAsync(decodedText);
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

        // Unselect text, because reloading can cause exceptions if the new text length is shorter than 
        // the range (position and length) of the selected text in the viewer control.
        this.MessengerInstance.Send(new UnselectTextInEditorMessage());

        this.IsContentFromFile = true;
        this.FilePath = filePath;
        this.FileName = Path.GetFileName(filePath);

        ReplaceOldDocumentWithNewDocument(fileContents);

        RaiseRefreshComplete();

        this.MessengerInstance.Send(new DisplayApplicationStatusMessage("File opened: " + Path.GetFileName(filePath)));
      }
      catch (Exception ex)
      {
        this.MessengerInstance.Send(new DisplayApplicationStatusMessage("Error opening file: " + ex.Message));
      }
    }



    private void RaiseRefreshComplete()
    {
      var handler = RefreshComplete;
      if (handler != null)
      {
        handler(this, EventArgs.Empty);
      }
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
        using (var file = new StreamWriter(this.FilePath))
        {
          await file.WriteAsync(this.Document.Text);
        }
      }
      else
      {
        var saveDialog = new SaveFileDialog();
        //dlg.FileName = "Document"; // Default file name
        saveDialog.DefaultExt = ".xml"; // Default file extension
        saveDialog.Filter = "XML documents (.xml)|*.xml"; // Filter files by extension

        // Show save file dialog box
        Nullable<bool> result = saveDialog.ShowDialog();

        // Process save file dialog box results
        if (result == true)
        {
          // Save document
          this.FilePath = saveDialog.FileName;
          this.FileName = Path.GetFileName(this.FilePath);
          this.IsContentFromFile = true;

          using (var file = new StreamWriter(this.FilePath))
          {
            await file.WriteAsync(this.Document.Text);
          }
        }

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
      // Unselect text, because reloading can cause exceptions if the new text length is shorter than 
      // the range (position and length) of the selected text in the viewer control.
      this.MessengerInstance.Send(new UnselectTextInEditorMessage());

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
