using eXeMeL.Messages;
using eXeMeL.Model;
using eXeMeL.ViewModel.XmlCleaners;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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




    public EditorViewModel(Settings settings)
    {
      this.Settings = settings;

      this.CopyCommand = new RelayCommand(CopyCommand_Execute);
      this.RefreshCommand = new RelayCommand(RefreshCommand_Execute);
      this.Cleaners = new List<XmlCleanerBase>()
        {
          new TrimCleaner(),
          new NewLineCleaner(),
          new SurroundingGarbageCleaner(),
          new VisualStudioCleaner(),
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



    async private void SetDocumentTextFromClipboard()
    {
      var text = await CleanXmlIfPossibleAsync(Clipboard.GetText());

      this.Document.Text = text;
    }



    private void RefreshCommand_Execute()
    {
      SetDocumentTextFromClipboard();
    }



    private void CopyCommand_Execute()
    {
      Clipboard.SetText(this.Document.Text);
    }
  }
}
