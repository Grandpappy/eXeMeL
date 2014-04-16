using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace eXeMeL.Model
{
  public enum SyntaxHighlightingStyle 
  {
    [Description("Bright")]
    Bright,

    [Description("Earthy")]
    Earthy 
  }


  [DataContract]
  public abstract class SettingsBase : INotifyPropertyChanged
  {
    protected void NotifyPropertyChanged(string propertyName)
    {
      var handler = this.PropertyChanged;
      if (handler != null)
      {
        handler(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }




  [DataContract]
  public class Settings : SettingsBase
  {
    private SyntaxHighlightingStyle _SyntaxHighlightingStyle;
    public const double DEFAULT_EDITOR_FONT_SIZE = 16;


    private bool _WrapEditorText;
    private bool _ShowEditorLineNumbers;
    private double _EditorFontSize;



    [DataMember]
    public bool ShowEditorLineNumbers
    {
      get { return _ShowEditorLineNumbers; }
      set { _ShowEditorLineNumbers = value; NotifyPropertyChanged("ShowEditorLineNumbers"); }
    }



    [DataMember]
    public bool WrapEditorText
    {
      get { return _WrapEditorText; }
      set { _WrapEditorText = value; NotifyPropertyChanged("WrapEditorText"); }
    }



    [DataMember]
    public double EditorFontSize
    {
      get { return _EditorFontSize; }
      set { _EditorFontSize = value; NotifyPropertyChanged("EditorFontSize"); }
    }


    [DataMember]
    public SyntaxHighlightingStyle SyntaxHighlightingStyle
    {
      get { return _SyntaxHighlightingStyle; }
      set { _SyntaxHighlightingStyle = value; NotifyPropertyChanged("SyntaxHighlightingStyle"); }
    }



    public Settings()
      : base()
    {
      this.ShowEditorLineNumbers = true;
      this.WrapEditorText = true;
      this.EditorFontSize = DEFAULT_EDITOR_FONT_SIZE;
      this.SyntaxHighlightingStyle = Model.SyntaxHighlightingStyle.Earthy;
    }
  }
}
