using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using eXeMeL.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

namespace eXeMeL.ViewModel
{
  public class XmlUtilityViewModel : ViewModelBase
  {
    private Settings Settings { get; }
    private string _documentText;
    private bool _isXmlValid;



    public XmlUtilityViewModel(Settings settings)
    {
      this.Settings = settings;
    }



    public string DocumentText
    {
      get { return this._documentText; }
      set
      {
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



    public void ParseDocumentText()
    {
      try
      {
        var root = XElement.Parse(this.DocumentText);

        this.IsXmlValid = true;
      }
      catch (Exception)
      {
        this.IsXmlValid = false;
      }
    }
  }


  public class XmlElementViewModel : ViewModelBase
  {
    
  }
}
