using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using eXeMeL.Messages;
using eXeMeL.Model;
using eXeMeL.ViewModel.UtilityOperationMessages;
using GalaSoft.MvvmLight.Messaging;

namespace eXeMeL.ViewModel
{
  public class XmlUtilityOperations
  {
    public Settings Settings { get; set; }
    public Func<ElementViewModel> GetRoot { get; set; }
    public Func<ElementViewModel> GetStartOfXPath { get; set; }



    public XmlUtilityOperations(Settings settings, Func<ElementViewModel> getRoot, Func<ElementViewModel> getStartOfXPath )
    {
      this.Settings = settings;
      this.GetRoot = getRoot;
      this.GetStartOfXPath = getStartOfXPath;
      Messenger.Default.Register<CollapseAllOtherElementsMessage>(this, HandleCollapseAllOtherElementsMessage);
      Messenger.Default.Register<ExpandAllChildElementsMessage>(this, HandleExpandAllChildElementsMessage);
      Messenger.Default.Register<BuildXPathFromRootMessage>(this, HandleBuildXPathFromRootMessage);
      Messenger.Default.Register<BuildXPathFromStartMessage>(this, HandleBuildXpathFromStartMessage);
    }



    private void HandleBuildXpathFromStartMessage(BuildXPathFromStartMessage message)
    {
      var startOfXPath = this.GetStartOfXPath();
      if (startOfXPath == null)
        return;

      var startElementAncestors = GetOrderedAncestorsFromElementToRoot(startOfXPath);
      var currentElementAncestors = GetOrderedAncestorsFromElementToRoot(message.Element);

      ElementViewModel commonAncestor = null;
      foreach (var startElementAncestor in startElementAncestors)
      {
        foreach (var currentElementAncestor in currentElementAncestors)
        {
          if (currentElementAncestor != startElementAncestor)
            continue;

          commonAncestor = startElementAncestor;
          break;
        }

        if (commonAncestor != null)
          break;
      }

      var numberOfElementsUpTheAncestorChainFromStart = startElementAncestors.IndexOf(commonAncestor);
      var numberOfElementsUpTheAncestorChainFromCurrent = currentElementAncestors.IndexOf(commonAncestor);


      var prefix = string.Concat(Enumerable.Repeat(@"../", numberOfElementsUpTheAncestorChainFromStart));
      var postfix = string.Join(@"/",
        currentElementAncestors.Take(numberOfElementsUpTheAncestorChainFromCurrent).Select(x => x.Name).Reverse().ToArray());

      var fullXpath = prefix + postfix;

      SendOutputBasedOnTarget(fullXpath, message.OutputTarget);
    }



    private void HandleBuildXPathFromRootMessage(BuildXPathFromRootMessage message)
    {
      var ancestors = GetOrderedAncestorsFromRootToElement(message.Element);
      var ancestorNames = ancestors.Select(x => x.Name).ToList();

      // Ignore the root element in the xpath, since that's where we're starting from
      var xPath = "/" + string.Join("/", ancestorNames.Skip(1).ToArray());

      var outputTarget = message.OutputTarget;
      SendOutputBasedOnTarget(xPath, outputTarget);
    }



    private static void SendOutputBasedOnTarget(string xPath, OutputTarget outputTarget)
    {
      if (outputTarget == OutputTarget.XPathEditor)
        Messenger.Default.Send(new ReplaceXPathMessage(xPath));
      else
        Clipboard.SetText(xPath);
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



    private List<ElementViewModel> GetOrderedAncestorsFromRootToElement(ElementViewModel element)
    {
      var list = GetOrderedAncestorsFromElementToRoot(element);
      list.Reverse();
      return list;
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