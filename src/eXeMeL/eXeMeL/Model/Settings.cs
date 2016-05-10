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
    private SyntaxHighlightingStyle _syntaxHighlightingStyle;
    private ApplicationTheme _applicationTheme;
    public const double DefaultEditorFontSize = 16;


    private bool _wrapEditorText;
    private bool _showEditorLineNumbers;
    private double _editorFontSize;
    private string _fontFamily;
    private Brush _editorBrush;
    private Brush _elementBrush;
    private Brush _attributeNameBrush;
    private Brush _attributeValueBrush;
    private Brush _hoverBackgroundBrush;
    private Brush _currentXPathTargetBrush;
    private Brush _currentXPathStartBrush;
    private bool _highlightOtherInstancesOfSelection;



    [DataMember]
    public bool ShowEditorLineNumbers
    {
      get { return this._showEditorLineNumbers; }
      set { this._showEditorLineNumbers = value; NotifyPropertyChanged("ShowEditorLineNumbers"); }
    }



    [DataMember]
    public bool WrapEditorText
    {
      get { return this._wrapEditorText; }
      set { this._wrapEditorText = value; NotifyPropertyChanged("WrapEditorText"); }
    }



    [DataMember]
    public double EditorFontSize
    {
      get { return this._editorFontSize; }
      set { this._editorFontSize = value; NotifyPropertyChanged("EditorFontSize"); }
    }



    [DataMember]
    public SyntaxHighlightingStyle SyntaxHighlightingStyle
    {
      get { return this._syntaxHighlightingStyle; }
      set
      {
        this._syntaxHighlightingStyle = value;
        NotifyPropertyChanged("SyntaxHighlightingStyle");
        UpdateBrushes();
      }
    }



    [DataMember]
    public ApplicationTheme ApplicationTheme
    {
      get { return this._applicationTheme; }
      set { this._applicationTheme = value; NotifyPropertyChanged("ApplicationTheme"); NotifyPropertyChanged("EditorBrush"); }
    }



    [DataMember]
    public string FontFamily
    {
      get { return this._fontFamily; }
      set { this._fontFamily = value; NotifyPropertyChanged("FontFamily"); }
    }



    [DataMember]
    public bool HighlightOtherInstancesOfSelection
    {
      get { return this._highlightOtherInstancesOfSelection; }
      set { this._highlightOtherInstancesOfSelection = value; NotifyPropertyChanged("HighlightOtherInstancesOfSelection"); }
    }



    public Brush EditorBrush
    {
      get { return this._editorBrush; }
      set { this._editorBrush = value; NotifyPropertyChanged("EditorBrush"); }
    }



    public Brush ElementBrush
    {
      get { return this._elementBrush; }
      set { this._elementBrush = value; NotifyPropertyChanged("ElementBrush"); }
    }



    public Brush AttributeNameBrush
    {
      get { return this._attributeNameBrush; }
      set { this._attributeNameBrush = value; NotifyPropertyChanged("AttributeNameBrush"); }
    }



    public Brush AttributeValueBrush
    {
      get { return this._attributeValueBrush; }
      set {this._attributeValueBrush = value; NotifyPropertyChanged("AttributeValueBrush"); }
    }



    public Brush HoverBackgroundBrush
    {
      get { return this._hoverBackgroundBrush; }
      set { this._hoverBackgroundBrush = value; NotifyPropertyChanged("HoverBackgroundBrush"); }
    }



    public Brush CurrentXPathTargetBrush
    {
      get { return this._currentXPathTargetBrush; }
      set { this._currentXPathTargetBrush = value; NotifyPropertyChanged("CurrentXPathTargetBrush"); }
    }



    public Brush CurrentXPathStartBrush
    {
      get { return this._currentXPathStartBrush; }
      set { this._currentXPathStartBrush = value; NotifyPropertyChanged("CurrentXPathStartBrush"); }
    }



    public Settings()
    {
      this.ShowEditorLineNumbers = true;
      this.WrapEditorText = true;
      this.EditorFontSize = DefaultEditorFontSize;
      this.SyntaxHighlightingStyle = SyntaxHighlightingStyle.Light_Earthy;
      this.ApplicationTheme = ApplicationTheme.Light;
      this.FontFamily = "Consolas";
    }



    private void UpdateBrushes()
    {
      this.EditorBrush = GetBrushForCurrentTheme(ThemeBrushTarget.EditorContent);
      this.ElementBrush = GetBrushForCurrentTheme(ThemeBrushTarget.Element);
      this.AttributeNameBrush = GetBrushForCurrentTheme(ThemeBrushTarget.AttributeName);
      this.AttributeValueBrush = GetBrushForCurrentTheme(ThemeBrushTarget.AttributeValue);
      this.HoverBackgroundBrush = GetBrushForCurrentTheme(ThemeBrushTarget.HoverBackground);
      this.CurrentXPathTargetBrush = GetBrushForCurrentTheme(ThemeBrushTarget.CurrentXPathTarget);
      this.CurrentXPathStartBrush = GetBrushForCurrentTheme(ThemeBrushTarget.CurrentXPathStart);
    }



    private Brush GetBrushForCurrentTheme(ThemeBrushTarget target)
    {
      var attribute = this.SyntaxHighlightingStyle.GetAttributes<AssociatedThemeBrushAttribute>()
         .FirstOrDefault(x => (x.AssociatedTheme == this.ApplicationTheme || x.AssociatedTheme == ApplicationTheme.Any)
                              && x.Target == target);

      return attribute?.AssociatedBrush ?? new SolidColorBrush(Colors.Red);
    }



    protected void NotifyPropertyChanged(string propertyName)
    {
      var handler = this.PropertyChanged;
      handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }

}
