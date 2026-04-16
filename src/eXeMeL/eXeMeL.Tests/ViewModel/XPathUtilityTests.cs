using System;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using eXeMeL.Model;
using eXeMeL.ViewModel;
using eXeMeL.ViewModel.UtilityOperationMessages;
using CommunityToolkit.Mvvm.Messaging;
using Xunit;

namespace eXeMeL.Tests.ViewModel
{
  public class XPathUtilityTests : IDisposable
  {
    private const string TestXml =
      @"<Root attr1=""val1""><Child1><GrandChild/></Child1><Child2>text</Child2></Root>";

    private readonly ElementViewModel _root;
    private readonly ElementViewModel _child1;
    private readonly ElementViewModel _child2;
    private readonly ElementViewModel _grandChild;


    public XPathUtilityTests()
    {
      WeakReferenceMessenger.Default.Reset();
      var xElement = XElement.Parse(TestXml);
      _root = new ElementViewModel(xElement, null);
      _child1 = _root.ChildElements[0];
      _child2 = _root.ChildElements[1];
      _grandChild = _child1.ChildElements[0];
    }


    public void Dispose()
    {
      WeakReferenceMessenger.Default.Reset();
    }


    #region ElementViewModel Tree Construction

    [Fact]
    public void Constructor_RootHasTwoChildren()
    {
      Assert.Equal(2, _root.ChildElements.Count);
    }


    [Fact]
    public void Constructor_RootHasOneAttribute()
    {
      Assert.Equal(1, _root.Attributes.Count);
      Assert.Equal("attr1", _root.Attributes[0].Name);
      Assert.Equal("val1", _root.Attributes[0].Value);
    }


    [Fact]
    public void Constructor_Child1HasOneChildAndNoAttributes()
    {
      Assert.Equal(1, _child1.ChildElements.Count);
      Assert.Empty(_child1.Attributes);
    }


    [Fact]
    public void Constructor_Child2HasInnerText()
    {
      Assert.Equal("text", _child2.InnerText);
      Assert.True(_child2.HasInnerText);
    }


    [Fact]
    public void Constructor_GrandChildHasNoChildrenAndNoInnerText()
    {
      Assert.Empty(_grandChild.ChildElements);
      Assert.False(_grandChild.HasInnerText);
    }


    [Fact]
    public void Constructor_AllElementsExpandedByDefault()
    {
      Assert.True(_root.IsExpanded);
      Assert.True(_child1.IsExpanded);
      Assert.True(_child2.IsExpanded);
      Assert.True(_grandChild.IsExpanded);
    }

    #endregion


    #region GetElementAndAllDescendents

    [Fact]
    public void GetElementAndAllDescendents_ReturnsFourElements()
    {
      var all = _root.GetElementAndAllDescendents();

      Assert.Equal(4, all.Count);
      Assert.Same(_root, all[0]);
      Assert.Same(_child1, all[1]);
      Assert.Same(_grandChild, all[2]);
      Assert.Same(_child2, all[3]);
    }

    #endregion


    #region CollapseAllChildElements

    [Fact]
    public void CollapseAllChildElements_CollapsesAllDescendants()
    {
      _root.CollapseAllChildElements();

      // Root itself is unchanged by CollapseAllChildElements (only children)
      Assert.True(_root.IsExpanded);
      Assert.False(_child1.IsExpanded);
      Assert.False(_child2.IsExpanded);
      Assert.False(_grandChild.IsExpanded);
    }

    #endregion


    #region CollapseAllChildElementsExcept

    [Fact]
    public void CollapseAllChildElementsExcept_CollapsesAllIncludingExcluded()
    {
      // Note: CollapseAllChildElementsExcept sets IsExpanded=false on each child BEFORE
      // the recursive self-check. The excluded element's own children are not recursed into,
      // but the element itself IS collapsed by its parent. The actual UI code in
      // XmlUtilityOperations re-expands ancestors separately after calling this method.
      _root.CollapseAllChildElementsExcept(_grandChild);

      Assert.True(_root.IsExpanded);        // Root is the caller, not touched
      Assert.False(_child1.IsExpanded);      // Collapsed
      Assert.False(_child2.IsExpanded);      // Collapsed
      Assert.False(_grandChild.IsExpanded);  // Also collapsed (parent sets it before recursion)
    }

    #endregion


    #region ExpandAllChildren

    [Fact]
    public void ExpandAllChildren_RestoresExpansionAfterCollapse()
    {
      _root.CollapseAllChildElements();

      // Verify collapsed first
      Assert.False(_child1.IsExpanded);
      Assert.False(_grandChild.IsExpanded);

      _root.ExpandAllChildren();

      Assert.True(_child1.IsExpanded);
      Assert.True(_child2.IsExpanded);
      Assert.True(_grandChild.IsExpanded);
    }

    #endregion


    #region XPath From Root

    [Fact]
    public void HandleBuildXPathFromRootMessage_ProducesCorrectXPath()
    {
      string capturedXPath = null;
      WeakReferenceMessenger.Default.Register<ReplaceXPathMessage>(this, (r, msg) => capturedXPath = msg.XPath);

      var settings = new Settings();
      var operations = new XmlUtilityOperations(settings, () => _root, () => _root);

      WeakReferenceMessenger.Default.Send(
        new BuildXPathFromRootMessage(_grandChild, OutputTarget.XPathEditor));

      Assert.NotNull(capturedXPath);
      Assert.Equal("/Child1/GrandChild", capturedXPath);
      GC.KeepAlive(operations);
    }


    [Fact]
    public void HandleBuildXPathFromRootMessage_ForChild2_ProducesCorrectXPath()
    {
      string capturedXPath = null;
      WeakReferenceMessenger.Default.Register<ReplaceXPathMessage>(this, (r, msg) => capturedXPath = msg.XPath);

      var settings = new Settings();
      var operations = new XmlUtilityOperations(settings, () => _root, () => _root);

      WeakReferenceMessenger.Default.Send(
        new BuildXPathFromRootMessage(_child2, OutputTarget.XPathEditor));

      Assert.NotNull(capturedXPath);
      Assert.Equal("/Child2", capturedXPath);
      GC.KeepAlive(operations);
    }

    #endregion


    #region XmlUtilityViewModel ParseDocumentText

    [Fact]
    public void ParseDocumentText_ValidXml_SetsIsXmlValidTrue()
    {
      var settings = new Settings();
      var viewModel = new XmlUtilityViewModel(settings);

      viewModel.DocumentText = TestXml;

      // ParseDocumentText runs on a background Task; wait for it to complete.
      WaitUntil(() => !viewModel.IsBusy, timeoutMs: 2000);

      Assert.True(viewModel.IsXmlValid);
      Assert.NotNull(viewModel.Root);
      Assert.Equal("Root", viewModel.Root.Name);
    }


    [Fact]
    public void ParseDocumentText_InvalidXml_DoesNotSetIsXmlValidTrue()
    {
      var settings = new Settings();
      var viewModel = new XmlUtilityViewModel(settings);

      // Set invalid XML on a fresh view model (IsXmlValid defaults to false).
      // The parse task will fault, and IsXmlValid should remain false.
      viewModel.DocumentText = "<not<valid>xml";
      WaitUntil(() => !viewModel.IsBusy, timeoutMs: 2000);

      Assert.False(viewModel.IsXmlValid);
      Assert.Null(viewModel.Root);
    }

    #endregion


    #region Helpers

    /// <summary>
    /// Spins until the predicate returns true or the timeout elapses.
    /// </summary>
    private static void WaitUntil(Func<bool> predicate, int timeoutMs)
    {
      var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
      while (!predicate() && DateTime.UtcNow < deadline)
      {
        Thread.Sleep(50);
      }
    }

    #endregion
  }
}
