using System;
using System.Collections.Generic;
using System.Linq;
using eXeMeL.Model;
using eXeMeL.ViewModel.UtilityOperationMessages;
using GalaSoft.MvvmLight.Messaging;

namespace eXeMeL.ViewModel
{
  public class XmlUtilityOperations
  {
    public Settings Settings { get; set; }
    public Func<ElementViewModel> GetRoot { get; set; }



    public XmlUtilityOperations(Settings settings, Func<ElementViewModel> getRoot)
    {
      this.Settings = settings;
      this.GetRoot = getRoot;
      Messenger.Default.Register<CollapseAllOtherElementsMessage>(this, HandleCollapseAllOtherElementsMessage);
      Messenger.Default.Register<ExpandAllChildElementsMessage>(this, HandleExpandAllChildElementsMessage);
      Messenger.Default.Register<FindXPathFromRootMessage>(this, HandleFindXPathFromRootMessage);
    }



    private void HandleFindXPathFromRootMessage(FindXPathFromRootMessage message)
    {
      var ancestors = GetOrderedAncestorsFromElementToRoot(message.Element);
      ancestors.Reverse();
      var ancestorNames = ancestors.Select(x => x.Name);

      var xPath = string.Join("/", ancestorNames.ToArray());
      Messenger.Default.Send(new ReplaceXPathMessage(xPath));
    }



    private void HandleExpandAllChildElementsMessage(ExpandAllChildElementsMessage message)
    {
      message.Element.IsExpanded = true;
      message.Element.ExpandAllChildren();
    }



    private void HandleCollapseAllOtherElementsMessage(CollapseAllOtherElementsMessage message)
    {
      var rootElement = this.GetRoot();

      //var currentElement = message.Element;
      rootElement.CollapseAllChildElementsExcept(message.Element);

      GetOrderedAncestorsFromElementToRoot(message.Element).ForEach(x => x.IsExpanded = true);

      //while (currentElement.Parent != null)
      //{
      //  currentElement.IsExpanded = true;
      //  currentElement = currentElement.Parent;
      //}
    }



    private List<ElementViewModel> GetOrderedAncestorsFromElementToRoot(ElementViewModel element)
    {
      var list = new List<ElementViewModel>();
      var currentElement = element;
      while (currentElement.Parent != null)
      {
        list.Add(currentElement);
        currentElement = currentElement.Parent;
      }

      if (currentElement != null)
        list.Add(currentElement);

      return list;
    }
  }
}