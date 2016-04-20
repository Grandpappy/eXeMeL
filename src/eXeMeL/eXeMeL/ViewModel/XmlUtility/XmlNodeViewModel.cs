using GalaSoft.MvvmLight;

namespace eXeMeL.ViewModel
{
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