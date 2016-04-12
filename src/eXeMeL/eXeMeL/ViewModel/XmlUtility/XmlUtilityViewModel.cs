using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public Settings Settings { get; }
    private string _documentText;
    private bool _isXmlValid;
    private ElementViewModel _root;
    private bool _isBusy;



    public XmlUtilityViewModel(Settings settings)
    {
      this.Settings = settings;
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
    }
  }


  public class ElementViewModel : XmlNodeViewModel
  {
    public ObservableCollection<ElementViewModel> ChildElements { get; private set; }
    public ObservableCollection<AttributeViewModel> Attributes { get; private set; }
    private XElement InternalElement { get; set; }
    private bool _IsExpanded;
    public bool IsExpanded
    {
      get { return this._IsExpanded; }
      set { Set(() => this.IsExpanded, ref this._IsExpanded, value); }
    }



    public ElementViewModel(XElement element, ElementViewModel parent)
      : base(parent, element.Name.LocalName, element.Value, element.Name.NamespaceName)
    {
      this.InternalElement = element;
      this.ChildElements = new ObservableCollection<ElementViewModel>();
      this.Attributes = new ObservableCollection<AttributeViewModel>();
      this.IsExpanded = true;

      Populate();
    }



    private void Populate()
    {
      foreach (var xmlAttribute in this.InternalElement.Attributes())
      {
        this.Attributes.Add(new AttributeViewModel(xmlAttribute, this));
      }

      foreach (var xmlElement in this.InternalElement.Elements())
      {
        this.ChildElements.Add(new ElementViewModel(xmlElement, this));
      }
    }
  }



  public class AttributeViewModel : XmlNodeViewModel
  {
    public XAttribute InternalAttribute { get; }



    public AttributeViewModel(XAttribute xmlAttribute, ElementViewModel parent)
      : base(parent, xmlAttribute.Name.LocalName, xmlAttribute.Value, xmlAttribute.Name.NamespaceName)
    {
      this.InternalAttribute = xmlAttribute;
    }
  }


  public abstract class XmlNodeViewModel : ViewModelBase
  {
    public ElementViewModel Parent { get; }
    public string Name { get; }
    public string Value { get; }
    public string NamespaceName { get; }



    protected XmlNodeViewModel(ElementViewModel parent, string name, string value, string namespaceName)
    {
      this.Parent = parent;
      this.Name = name;
      this.Value = value;
      this.NamespaceName = namespaceName;
    }
  }
}
