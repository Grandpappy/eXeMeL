using System.Xml.Linq;

namespace eXeMeL.ViewModel
{
  public class AttributeViewModel : XmlNodeViewModel
  {
    public XAttribute InternalAttribute { get; }



    public AttributeViewModel(XAttribute xmlAttribute, ElementViewModel parent)
      : base(parent, xmlAttribute.Name.LocalName, xmlAttribute.Value, xmlAttribute.Name.NamespaceName)
    {
      this.InternalAttribute = xmlAttribute;
    }
  }
}