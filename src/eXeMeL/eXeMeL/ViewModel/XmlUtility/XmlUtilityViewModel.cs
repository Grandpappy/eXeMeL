using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
          foreach (var x in this.Root.GetElementAndAllDescendents())
          {
            x.IsXPathTarget = false;
            x.IsXPathStart = false;
          }

          if (this.StartOfXPath != null)
          {
            this.StartOfXPath.IsXPathStart = true;
          }
        }
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
      set
      {
        SetProperty(ref this._isXmlValid, value);
        OnPropertyChanged(nameof(IsXmlValid));
      }
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
        this.StartOfXPath = this.Root;
      }
    }



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

            ParseElement(root);

            this.IsXmlValid = true;
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



    private void ParseElement(XElement root)
    {
      this.Root = new ElementViewModel(root, null);
      //this.Root.Populate();
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
          var allElements = this.Root.GetElementAndAllDescendents();
          allElements.ForEach(x => x.IsXPathTarget = false);

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

          var bringNextIntoView = true;

          foreach (var foundXElement in foundXElements)
          {
            foreach (var currentElement in allElements)
            {
              if (this.ElementUpdateCancellation.IsCancellationRequested)
              {
                CompleteCurrentElementUpdateAction();
                return;
              }

              if (currentElement.InternalElement == foundXElement)
              {
                currentElement.IsXPathTarget = true;
                if (bringNextIntoView)
                {
                  currentElement.RaiseBringIntoView();
                  bringNextIntoView = false;
                }
              }
            }
          }

          if (this.ElementUpdateCancellation.IsCancellationRequested)
          {
            CompleteCurrentElementUpdateAction();
            return;
          }

          // TODO Handle attributes
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
