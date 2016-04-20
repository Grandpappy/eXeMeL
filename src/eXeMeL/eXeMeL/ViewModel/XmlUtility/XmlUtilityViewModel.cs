using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using eXeMeL.Model;
using eXeMeL.ViewModel.UtilityOperationMessages;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

namespace eXeMeL.ViewModel
{
  public class XmlUtilityViewModel : ViewModelBase
  {
    public Settings Settings { get; }
    private string _documentText;
    private bool _isXmlValid;
    private ElementViewModel _root;
    private bool _isBusy;
    private string _xPath;



    public XmlUtilityViewModel(Settings settings)
    {
      this.Settings = settings;
      this.UtilityOperations = new XmlUtilityOperations(settings, () => this.Root);
      this.MessengerInstance.Register< ReplaceXPathMessage>(this, HandleReplaceXPathMessage);
    }



    public XmlUtilityOperations UtilityOperations { get; set; }



    public string XPath
    {
      get { return this._xPath; }
      set { Set(() => this.XPath, ref this._xPath, value); }
    }



    public string DocumentText
    {
      get { return this._documentText; }
      set
      {
        //if (this.DocumentText == value)
        //{
        //  Task.Factory.StartNew(() =>
        //  {
        //    RaisePropertyChanged(() => this.IsBusy);
        //  });

        //  return;
        //}

        Set(() => this.DocumentText, ref this._documentText, value);
        ParseDocumentText();
      }
    }



    public bool IsXmlValid
    {
      get { return this._isXmlValid; }
      set
      {
        Set(() => this.IsXmlValid, ref this._isXmlValid, value); 
        RaisePropertyChanged(() => this.IsXmlValid);
      }
    }


    
    public bool IsBusy
    {
      get { return this._isBusy; }
      set { Set(() => this.IsBusy, ref this._isBusy, value); }
    }




    public ElementViewModel Root
    {
      get { return this._root; }
      set { Set(() => this.Root, ref this._root, value); }
    }



    public void ParseDocumentText()
    {
      try
      {
        this.IsBusy = true;

        Task t = new Task(() =>
        {
          try
          {
            var root = XElement.Parse(this.DocumentText);

            ParseElement(root);

            this.IsXmlValid = true;
          }
          finally
          {
            this.IsBusy = false;
          }
        });

        t.Start();

      }
      catch (Exception)
      {
        this.Root = null;
        this.IsXmlValid = false;
      }
    }



    private void ParseElement(XElement root)
    {
      this.Root = new ElementViewModel(root, null);
      //this.Root.Populate();
    }



    private void HandleReplaceXPathMessage(ReplaceXPathMessage message)
    {
      this.XPath = message.Value;
    }
  }
}
