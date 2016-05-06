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
      this.UtilityOperations = new XmlUtilityOperations(settings, () => this.Root, () => this.StartOfXPath);
      this.MessengerInstance.Register< ReplaceXPathMessage>(this, HandleReplaceXPathMessage);
      Messenger.Default.Register<DocumentRefreshCompleted>(this, HandleDocumentRefressMessage);
      Messenger.Default.Register<SetStartElementForXPathMessage>(this, HandleSetStartElementForXPathMessage);
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
        Set(() => this.XPath, ref this._xPath, value);
        UpdateElementsInXPath();
      }
    }



    private ElementViewModel StartOfXPath
    {
      get { return this._startOfXPath; }
      set
      {
        Set(() => this.StartOfXPath, ref this._startOfXPath, value);
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
      set { Set(() => this.StartOfXPathText, ref this._startOfXPathText, value); }
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
      set
      {
        Set(() => this.Root, ref this._root, value);
        this.StartOfXPath = this.Root;
      }
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

          this.MessengerInstance.Send(new DisplayApplicationStatusMessage(foundXElements.Count + " element found.  " + attributes.Count + " attributes found"));

          
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

      var t = new Task(() => actionToRun());
      t.Start();
    }


    private CancellationTokenSource ElementUpdateCancellation { get; set; }
    private object _elementUpdateLock = new object();
    private ElementViewModel _startOfXPath;
    private string _startOfXPathText;
    private Action CurrentElementAction { get; set; }
    private Action NextElementAction { get; set; }
  }
}
