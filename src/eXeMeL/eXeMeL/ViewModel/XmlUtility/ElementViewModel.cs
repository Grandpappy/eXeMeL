using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Xml.Linq;
using eXeMeL.ViewModel.UtilityOperationMessages;
using GalaSoft.MvvmLight.Command;

namespace eXeMeL.ViewModel
{
  public class ElementViewModel : XmlNodeViewModel
  {
    public List<ElementViewModel> ChildElements { get; private set; }
    public List<AttributeViewModel> Attributes { get; private set; }
    public XElement InternalElement { get; private set; }
    private bool _IsExpanded;
    public string InnerText { get; set; }
    public bool HasInnerText => !string.IsNullOrWhiteSpace(this.InnerText);
    public ICommand CollapseAllOtherElementsCommand { get; }
    public ICommand ExpandAllChildElementsCommand { get; }
    public ICommand BuildXPathFromRootCommand { get; }
    public ICommand CopyXPathFromRootCommand { get; }
    public ICommand SetStartXPathCommand { get; set; }
    public ICommand BuildXPathFromStartCommand { get; set; }
    public ICommand CopyXPathFromStartCommand { get; set; }



    public bool IsExpanded
    {
      get { return this._IsExpanded; }
      set { Set(() => this.IsExpanded, ref this._IsExpanded, value); }
    }






    public ElementViewModel(XElement element, ElementViewModel parent)
      : base(parent, element.Name.LocalName, element.Value, element.Name.NamespaceName)
    {
      this.InnerText = (element.FirstNode as XText)?.Value;
      this.InternalElement = element;
      this.ChildElements = new List<ElementViewModel>();
      this.Attributes = new List<AttributeViewModel>();
      this.IsExpanded = true;
      this.CollapseAllOtherElementsCommand = new RelayCommand<ElementViewModel>(CollapseAllOtherElementsCommand_Execute);
      this.ExpandAllChildElementsCommand = new RelayCommand<ElementViewModel>(ExpandAllChildElementsCommand_Execute);
      this.BuildXPathFromRootCommand = new RelayCommand<ElementViewModel>(BuildXPathFromRootCommand_Execute);
      this.CopyXPathFromRootCommand = new RelayCommand<ElementViewModel>(CopyXPathFromRootCommand_Execute);
      this.SetStartXPathCommand = new RelayCommand<ElementViewModel>(SetStartXPathCommand_Execute);
      this.BuildXPathFromStartCommand = new RelayCommand<ElementViewModel>(BuildXPathFromStartCommand_Execute);
      this.CopyXPathFromStartCommand = new RelayCommand<ElementViewModel>(CopyXPathFromStartCommand_Execute);


      Populate();
    }



    private void CopyXPathFromStartCommand_Execute(ElementViewModel element)
    {
      this.MessengerInstance.Send(new BuildXPathFromStartMessage(element, OutputTarget.Clipboard));
    }



    private void BuildXPathFromStartCommand_Execute(ElementViewModel element)
    {
      this.MessengerInstance.Send(new BuildXPathFromStartMessage(element, OutputTarget.XPathEditor));
    }



    private void SetStartXPathCommand_Execute(ElementViewModel element)
    {
      this.MessengerInstance.Send(new SetStartElementForXPathMessage(element));
    }



    private void BuildXPathFromRootCommand_Execute(ElementViewModel element)
    {
      this.MessengerInstance.Send(new BuildXPathFromRootMessage(element, OutputTarget.XPathEditor));
    }



    private void CopyXPathFromRootCommand_Execute(ElementViewModel element)
    {
      this.MessengerInstance.Send(new BuildXPathFromRootMessage(element, OutputTarget.Clipboard));
    }



    private void ExpandAllChildElementsCommand_Execute(ElementViewModel element)
    {
      this.MessengerInstance.Send(new ExpandAllChildElementsMessage(element));
    }



    private void CollapseAllOtherElementsCommand_Execute(ElementViewModel element)
    {
      this.MessengerInstance.Send(new CollapseAllOtherElementsMessage(element));
    }



    //private static ElementViewModel FindRoot(ElementViewModel element)
    //{
    //  var currentElement = element;
    //  while (currentElement.Parent != null)
    //  {
    //    currentElement = currentElement.Parent;
    //  }
    //  return currentElement;
    //}



    public void Populate()
    {
      foreach (var xmlAttribute in this.InternalElement.Attributes())
      {
        var attribute = new AttributeViewModel(xmlAttribute, this);
        this.Attributes.Add(attribute);
      }

      foreach (var xmlElement in this.InternalElement.Elements())
      {
        var element = new ElementViewModel(xmlElement, this);
        this.ChildElements.Add(element);
        //element.Populate();
      }
    }



    public void CollapseAllChildElements()
    {
      foreach (var child in this.ChildElements)
      {
        child.IsExpanded = false;
        child.CollapseAllChildElements();
      }
    }



    public void CollapseAllChildElementsExcept(ElementViewModel elementToExclude)
    {
      if (this == elementToExclude)
        return;

      foreach (var child in this.ChildElements)
      {
        child.IsExpanded = false;
        child.CollapseAllChildElementsExcept(elementToExclude);
      }
    }



    public void ExpandAllChildren()
    {
      foreach (var child in this.ChildElements)
      {
        child.IsExpanded = true;
        child.ExpandAllChildren();
      }
    }



    public List<ElementViewModel> GetElementAndAllDescendents()
    {
      var l = new List<ElementViewModel> {this};

      foreach (var child in this.ChildElements)
      {
        l.AddRange(child.GetElementAndAllDescendents());
      }

      return l;
    }



    public void RaiseBringIntoView()
    {
      var handler = this.BringIntoView;
      handler?.Invoke(this, new EventArgs());
    }



    public event BringIntoViewEventHandler BringIntoView;
    public delegate void BringIntoViewEventHandler(object sender, EventArgs e);

  }
}