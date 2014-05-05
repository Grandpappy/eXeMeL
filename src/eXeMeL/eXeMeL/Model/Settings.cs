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
  [DataContract]
  public class Settings : INotifyPropertyChanged
  {
    private SyntaxHighlightingStyle _SyntaxHighlightingStyle;
    private ApplicationTheme _ApplicationTheme;
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



    [DataMember]
    public ApplicationTheme ApplicationTheme
    {
      get { return _ApplicationTheme; }
      set { _ApplicationTheme = value; NotifyPropertyChanged("ApplicationTheme"); }
    }



    public Settings()
      : base()
    {
      this.ShowEditorLineNumbers = true;
      this.WrapEditorText = true;
      this.EditorFontSize = DEFAULT_EDITOR_FONT_SIZE;
      this.SyntaxHighlightingStyle = Model.SyntaxHighlightingStyle.Light_Earthy;
      this.ApplicationTheme = Model.ApplicationTheme.Light;
    }



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

}
