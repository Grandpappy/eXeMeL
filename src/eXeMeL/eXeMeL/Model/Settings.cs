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
using System.Windows.Media;
using eXeMeL.Utilities;
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
    private string _FontFamily;
    private Brush _editorBrush;
    private Brush _elementBrush;
    private Brush _attributeNameBrush;
    private Brush _attributeValueBrush;


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
      set
      {
        _SyntaxHighlightingStyle = value;
        NotifyPropertyChanged("SyntaxHighlightingStyle");
        UpdateBrushes();
      }
    }



    [DataMember]
    public ApplicationTheme ApplicationTheme
    {
      get { return _ApplicationTheme; }
      set { _ApplicationTheme = value; NotifyPropertyChanged("ApplicationTheme"); NotifyPropertyChanged("EditorBrush"); }
    }



    [DataMember]
    public string FontFamily
    {
      get { return this._FontFamily; }
      set { _FontFamily = value; NotifyPropertyChanged("FontFamily"); }
    }



    public Brush EditorBrush
    {
      get { return this._editorBrush; }
      set { _editorBrush = value; NotifyPropertyChanged("EditorBrush"); }
    }



    public Brush ElementBrush
    {
      get { return this._elementBrush; }
      set { _elementBrush = value; NotifyPropertyChanged("ElementBrush"); }
    }



    public Brush AttributeNameBrush
    {
      get { return this._attributeNameBrush; }
      set { _attributeNameBrush = value; NotifyPropertyChanged("AttributeNameBrush"); }
    }



    public Brush AttributeValueBrush
    {
      get { return this._attributeValueBrush; }
      set { _attributeValueBrush = value; NotifyPropertyChanged("AttributeValueBrush"); }
    }



    public Settings()
      : base()
    {
      this.ShowEditorLineNumbers = true;
      this.WrapEditorText = true;
      this.EditorFontSize = DEFAULT_EDITOR_FONT_SIZE;
      this.SyntaxHighlightingStyle = Model.SyntaxHighlightingStyle.Light_Earthy;
      this.ApplicationTheme = Model.ApplicationTheme.Light;
      this.FontFamily = "Consolas";
    }



    private void UpdateBrushes()
    {
      this.EditorBrush = GetBrushForCurrentTheme(ThemeBrushTarget.EditorContent);
      this.ElementBrush = GetBrushForCurrentTheme(ThemeBrushTarget.Element);
      this.AttributeNameBrush = GetBrushForCurrentTheme(ThemeBrushTarget.AttributeName);
      this.AttributeValueBrush = GetBrushForCurrentTheme(ThemeBrushTarget.AttributeValue);
    }



    private Brush GetBrushForCurrentTheme(ThemeBrushTarget target)
    {
      var attribute = this.SyntaxHighlightingStyle.GetAttributes<AssociatedThemeBrushAttribute>()
         .FirstOrDefault(x => (x.AssociatedTheme == this.ApplicationTheme || x.AssociatedTheme == ApplicationTheme.Any)
                              && x.Target == target);

      if (attribute == null)
        return new SolidColorBrush(Colors.Red);
      else
        return attribute.AssociatedBrush;
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
