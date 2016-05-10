using GalaSoft.MvvmLight;

namespace eXeMeL.ViewModel
{
  public abstract class XmlNodeViewModel : ViewModelBase
  {
    private bool _isAlongXPath;
    private bool _isXPathTarget;
    private bool _isXPathStart;
    public ElementViewModel Parent { get; }
    public string Name { get; }
    public string Value { get; }
    public string NamespaceName { get; }



    public bool IsAlongXPath
    {
      get { return this._isAlongXPath; }
      set { Set(() => this.IsAlongXPath, ref this._isAlongXPath, value); }
    }



    public bool IsXPathStart
    {
      get { return this._isXPathStart; }
      set { Set(() => this.IsXPathStart, ref this._isXPathStart, value); }
    }



    public bool IsXPathTarget
    {
      get { return this._isXPathTarget; }
      set { Set(() => this.IsXPathTarget, ref this._isXPathTarget, value); }
    }



    protected XmlNodeViewModel(ElementViewModel parent, string name, string value, string namespaceName)
    {
      this.Parent = parent;
      this.Name = name;
      this.Value = value;
      this.NamespaceName = namespaceName;
    }
  }
}