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


namespace eXeMeL.ViewModel
{
  public class EditorViewModel : ViewModelBase
  {
    private TextDocument _Document;
    private List<XmlCleanerBase> Cleaners;


    public TextDocument Document
    {
      get { return _Document; }
      set { this.Set(() => Document, ref _Document, value); }
    }

    public Settings Settings { get; private set; }
    public ICommand CopyCommand { get; private set; }
    public ICommand RefreshCommand { get; private set; }
    public ICommand CopyDecodedXmlFromCursorPositionCommand { get; private set; }
    public ICommand ReplaceEditorContentsWithDecodedXmlFromCursorPositionCommand { get; private set; }
    public EditorFindViewModel FindViewModel { get; private set; }
    public event EventHandler RefreshComplete;
    public TextViewPosition CaretPosition { get; set; }



    public EditorViewModel(Settings settings)
    {
      this.Settings = settings;

      this.CopyCommand = new RelayCommand(CopyCommand_Execute);
      this.RefreshCommand = new RelayCommand(RefreshCommand_Execute);
      this.CopyDecodedXmlFromCursorPositionCommand = new RelayCommand(CopyDecodedXmlFromCursorPositionCommand_Execute, CopyDecodedXmlFromCursorPositionCommand_CanExecute);
      this.ReplaceEditorContentsWithDecodedXmlFromCursorPositionCommand = new RelayCommand(ReplaceEditorContentsWithDecodedXmlFromCursorPositionCommand_Execute);
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
      }
      else
      {
        this.Document = new TextDocument();
      }

      this.FindViewModel = new EditorFindViewModel(this.Document);
    }



    async public Task<string> CleanXmlIfPossibleAsync(string xml)
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
              this.MessengerInstance.Send<DisplayApplicationStatusMessage>(new DisplayApplicationStatusMessage(context.ErrorMessage));
              return;
            }
          }

          if (context.ParsedXml != null)
          {
            this.MessengerInstance.Send<DisplayApplicationStatusMessage>(new DisplayApplicationStatusMessage("XML parsed correctly"));
          }
          else
          {
            this.MessengerInstance.Send<DisplayApplicationStatusMessage>(new DisplayApplicationStatusMessage("Text was not able to be parsed into XML"));
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



    async private Task SetDocumentTextFromClipboardAsync()
    {
      var text = await CleanXmlIfPossibleAsync(Clipboard.GetText());

      ReplaceDocumentText(text);

      var handler = RefreshComplete;
      if (handler != null)
      {
        handler(this, EventArgs.Empty);
      }
    }



    private void ReplaceDocumentText(string newText)
    {
      this.Document.Text = newText;
      this.MessengerInstance.Send(new DocumentTextReplacedMessage());
    }



    async private void RefreshCommand_Execute()
    {
      await SetDocumentTextFromClipboardAsync();
    }



    async private void CopyDecodedXmlFromCursorPositionCommand_Execute()
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



    async private void ReplaceEditorContentsWithDecodedXmlFromCursorPositionCommand_Execute()
    {
      var decodedText = await GetDecodedTextAtCaretPositionAsync();
      if (decodedText != null)
      {
        var cleanedText = await CleanXmlIfPossibleAsync(decodedText);
        ReplaceDocumentText(cleanedText);
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
        ReplaceDocumentText(fileContents);

        RaiseRefreshComplete();

        this.MessengerInstance.Send<DisplayApplicationStatusMessage>(new DisplayApplicationStatusMessage("File opened: " + Path.GetFileName(filePath)));
      }
      catch (Exception ex)
      {
        this.MessengerInstance.Send<DisplayApplicationStatusMessage>(new DisplayApplicationStatusMessage("Error opening file: " + ex.Message));
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



    private async Task<string> LoadFileContentsAsync(string filePath)
    {
      return await Task<string>.Run(() => { return File.ReadAllText(filePath); } );
    }

    
  }
}
