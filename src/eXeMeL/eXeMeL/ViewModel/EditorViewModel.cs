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
    public ICommand PasteCommand { get; private set; }




    public EditorViewModel(Settings settings)
    {
      this.Settings = settings;

      this.CopyCommand = new RelayCommand(CopyCommand_Execute);
      this.PasteCommand = new RelayCommand(PasteCommand_Execute);
      this.Cleaners = new List<XmlCleanerBase>()
        {
          new TrimCleaner(),
          new SurroundingGarbageCleaner(),
          new FormatCleaner()
        };

      //if (IsInDesignMode)
      //{
      //this.Document = new TextDocument() { Text = this.ParsedXml.ToString() };
      this.Document = new TextDocument() { Text = "<Root IsValue=\"true\"><FirstChild Name=\"Robby\" Address=\"1521 Greenway Dr\"><Toys>All of them</Toys></FirstChild></Root>" };

      //}
      //else
      //{
      //  // Code runs "for real"
      //}
    }



    public string CleanXmlIfPossible(string xml)
    {
      if (!XmlShouldBeCleaned(xml))
        return xml;

      var context = new XmlCleanerContext() { XmlToClean = xml };

      foreach (var cleaner in this.Cleaners)
      {
        cleaner.CleanXml(context);
      }

      return context.XmlToClean;
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



    private void PasteCommand_Execute()
    {
      this.Document.Text = CleanXmlIfPossible(Clipboard.GetText());
    }



    private void CopyCommand_Execute()
    {
      Clipboard.SetText(this.Document.Text);
    }
  }
}
