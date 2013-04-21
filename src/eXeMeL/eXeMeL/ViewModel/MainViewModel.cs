using eXeMeL.Messages;
using eXeMeL.Model;
using GalaSoft.MvvmLight;
using ICSharpCode.AvalonEdit.Document;
using System.Xml.Linq;

namespace eXeMeL.ViewModel
{
  /// <summary>
  /// This class contains properties that the main View can data bind to.
  /// <para>
  /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
  /// </para>
  /// <para>
  /// You can also use Blend to data bind with the tool's support.
  /// </para>
  /// <para>
  /// See http://www.galasoft.ch/mvvm
  /// </para>
  /// </summary>
  public class MainViewModel : ViewModelBase
  {
    //private string _XmlString;
    private XElement _ParsedXml;


    public Settings Settings { get; private set; }
    public EditorViewModel Editor { get; private set; }



    public XElement ParsedXml
    {
      get { return _ParsedXml; }
      set { _ParsedXml = value; }
    }






    public MainViewModel()
    {
      this.Settings = SettingsIO.ReadSettingsFile<Settings>();
      this.Editor = new EditorViewModel(this.Settings);
      this.MessengerInstance.Register<ApplicationClosingMessage>(this, HandleApplicationClosingMessage);

      //if (IsInDesignMode)
      //{
        //this.ParsedXml = 
        //  new XElement("Root",
        //    new XAttribute("IsValue", true),
        //    new XElement("FirstChild",
        //      new XAttribute("Name", "Robby"),
        //      new XAttribute("Address", "1521 Greenway Dr"),
        //      new XElement("Toys", "All of them")
        //    )
        //  );




        //this.XmlString = this.ParsedXml.ToString();
      //}
      //else
      //{
      //  // Code runs "for real"
      //}
    }



    private void HandleApplicationClosingMessage(ApplicationClosingMessage message)
    {
      SettingsIO.WriteSettingsFile(this.Settings);
    }
  }
}