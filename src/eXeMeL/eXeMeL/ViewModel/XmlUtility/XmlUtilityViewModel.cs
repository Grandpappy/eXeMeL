using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using eXeMeL.Messages;
using eXeMeL.Model;
using eXeMeL.Utilities;
using eXeMeL.ViewModel.UtilityOperationMessages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace eXeMeL.ViewModel
{
  public class XmlUtilityViewModel : ObservableObject
  {
    public Settings Settings { get; }
    private string _documentText;
    private bool _isXmlValid;
    private ElementViewModel _root;
    private bool _isBusy;
    private string _xPath;

    // O(1) lookup from XElement to its ViewModel — rebuilt when tree is parsed
    private Dictionary<XElement, ElementViewModel> _elementIndex;



    public XmlUtilityViewModel(Settings settings)
    {
      this.Settings = settings;
      this.UtilityOperations = new XmlUtilityOperations(settings, () => this.Root, () => this.StartOfXPath);
      WeakReferenceMessenger.Default.Register<ReplaceXPathMessage>(this, (r, m) => HandleReplaceXPathMessage(m));
      WeakReferenceMessenger.Default.Register<DocumentRefreshCompleted>(this, (r, m) => HandleDocumentRefressMessage(m));
      WeakReferenceMessenger.Default.Register<SetStartElementForXPathMessage>(this, (r, m) => HandleSetStartElementForXPathMessage(m));
    }


    private void HandleDocumentRefressMessage(DocumentRefreshCompleted message)
    {
      this.StartOfXPath = null;
    }



    private void HandleSetStartElementForXPathMessage(SetStartElementForXPathMessage message)
    {
      this.StartOfXPath = message.Element;
    }



    public XmlUtilityOperations UtilityOperations { get; set; }



    public string XPath
    {
      get { return this._xPath; }
      set
      {
        SetProperty(ref this._xPath, value);
        UpdateElementsInXPath();
      }
    }



    private ElementViewModel StartOfXPath
    {
      get { return this._startOfXPath; }
      set
      {
        SetProperty(ref this._startOfXPath, value, nameof(StartOfXPath));
        UpdateStartOfXPathText();

        if (this.Root != null)
        {
          // Only reset XPath markers on the previously-marked elements, not the entire tree
          ClearAllXPathMarkers();

          if (this.StartOfXPath != null)
          {
            this.StartOfXPath.IsXPathStart = true;
          }
        }
      }
    }



    private void ClearAllXPathMarkers()
    {
      if (_elementIndex == null) return;

      foreach (var element in _elementIndex.Values)
      {
        if (element.IsXPathTarget)
          element.IsXPathTarget = false;
        if (element.IsXPathStart)
          element.IsXPathStart = false;
      }
    }



    private void UpdateStartOfXPathText()
    {
      this.StartOfXPathText = this.StartOfXPath?.Name ?? "Root";
    }



    public string StartOfXPathText
    {
      get { return this._startOfXPathText; }
      set { SetProperty(ref this._startOfXPathText, value); }
    }



    public string DocumentText
    {
      get { return this._documentText; }
      set
      {
        SetProperty(ref this._documentText, value);
        ParseDocumentText();
      }
    }



    public bool IsXmlValid
    {
      get { return this._isXmlValid; }
      set { SetProperty(ref this._isXmlValid, value); }
    }



    public bool IsBusy
    {
      get { return this._isBusy; }
      set { SetProperty(ref this._isBusy, value); }
    }




    public ElementViewModel Root
    {
      get { return this._root; }
      set
      {
        SetProperty(ref this._root, value);
        OnPropertyChanged(nameof(RootItems));
        this.StartOfXPath = this.Root;
      }
    }

    /// <summary>
    /// Wraps Root in a single-item list for TreeView binding.
    /// </summary>
    public List<ElementViewModel> RootItems =>
      this.Root != null ? new List<ElementViewModel> { this.Root } : new List<ElementViewModel>();



    public void ParseDocumentText()
    {
      try
      {
        this.IsBusy = true;

        _ = Task.Run(() =>
        {
          try
          {
            var root = XElement.Parse(this.DocumentText);

            // Build the entire ViewModel tree on the background thread
            var rootVm = new ElementViewModel(root, null);

            // Build the O(1) index on background thread
            var index = BuildElementIndex(rootVm);

            // Set properties — WPF auto-marshals PropertyChanged to the UI thread
            _elementIndex = index;
            this.Root = rootVm;
            this.IsXmlValid = true;
          }
          catch (Exception)
          {
            _elementIndex = null;
            this.Root = null;
            this.IsXmlValid = false;
          }
          finally
          {
            this.IsBusy = false;
          }
        });
      }
      catch (Exception)
      {
        this.Root = null;
        this.IsXmlValid = false;
      }
    }



    private static Dictionary<XElement, ElementViewModel> BuildElementIndex(ElementViewModel root)
    {
      var index = new Dictionary<XElement, ElementViewModel>();
      BuildElementIndexRecursive(root, index);
      return index;
    }


    private static void BuildElementIndexRecursive(ElementViewModel element, Dictionary<XElement, ElementViewModel> index)
    {
      index[element.InternalElement] = element;
      foreach (var child in element.ChildElements)
      {
        BuildElementIndexRecursive(child, index);
      }
    }



    private void HandleReplaceXPathMessage(ReplaceXPathMessage message)
    {
      this.XPath = message.Value;
    }



    private void UpdateElementsInXPath()
    {
      var xPathToUse = this.XPath;
      var xPathRoot = this.StartOfXPath;

      AddNewElementUpdateAction(() =>
      {
        try
        {
          // Clear previous XPath targets using the index (no full tree traversal needed)
          if (_elementIndex != null)
          {
            foreach (var element in _elementIndex.Values)
            {
              if (element.IsXPathTarget)
                element.IsXPathTarget = false;
            }
          }

          var result = (IEnumerable)xPathRoot.InternalElement.XPathEvaluate(xPathToUse);
          if (this.ElementUpdateCancellation.IsCancellationRequested)
          {
            CompleteCurrentElementUpdateAction();
            return;
          }

          var attributes = result.OfType<XAttribute>().ToList();
          var foundXElements = result.OfType<XElement>().ToList();

          WeakReferenceMessenger.Default.Send(new DisplayApplicationStatusMessage(foundXElements.Count + " element found.  " + attributes.Count + " attributes found"));

          if (this.ElementUpdateCancellation.IsCancellationRequested)
          {
            CompleteCurrentElementUpdateAction();
            return;
          }

          // O(1) lookup per match instead of O(n) nested loop
          var bringNextIntoView = true;
          foreach (var foundXElement in foundXElements)
          {
            if (this.ElementUpdateCancellation.IsCancellationRequested)
            {
              CompleteCurrentElementUpdateAction();
              return;
            }

            if (_elementIndex != null && _elementIndex.TryGetValue(foundXElement, out var vm))
            {
              vm.IsXPathTarget = true;
              if (bringNextIntoView)
              {
                vm.RaiseBringIntoView();
                bringNextIntoView = false;
              }
            }
          }
        }
        catch (Exception)
        {
          // XPath evaluation can throw on invalid expressions — silently ignore
        }
        finally
        {
          CompleteCurrentElementUpdateAction();
        }
      });
    }



    private void AddNewElementUpdateAction(Action action)
    {
      var startImmediately = false;
      lock (this._elementUpdateLock)
      {
        if (this.CurrentElementAction == null)
          startImmediately = true;

        this.ElementUpdateCancellation?.Cancel();

        this.NextElementAction = action;
      }

      if (startImmediately)
        StartNextElementUpdateAction();
    }



    private void CompleteCurrentElementUpdateAction()
    {
      var startNext = false;
      lock (this._elementUpdateLock)
      {
        if (this.NextElementAction != null)
          startNext = true;

        this.CurrentElementAction = null;
        this.ElementUpdateCancellation = null;
      }

      if (startNext)
        StartNextElementUpdateAction();
    }



    private void StartNextElementUpdateAction()
    {
      lock (this._elementUpdateLock)
      {
        this.ElementUpdateCancellation = new CancellationTokenSource();
        this.CurrentElementAction = this.NextElementAction;
        this.NextElementAction = null;
      }

      var actionToRun = this.CurrentElementAction;
      if (actionToRun == null)
        return;

      _ = Task.Run(() => actionToRun());
    }


    private CancellationTokenSource ElementUpdateCancellation { get; set; }
    private readonly object _elementUpdateLock = new object();
    private ElementViewModel _startOfXPath;
    private string _startOfXPathText;
    private Action CurrentElementAction { get; set; }
    private Action NextElementAction { get; set; }
  }
}
