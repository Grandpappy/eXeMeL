using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace eXeMeL.Model
{
  internal static class SettingsIO
  {
    public static void WriteSettingsFile(SettingsBase settings)
    {
      using (FileStream fs = new FileStream("Settings.json", FileMode.Create))
      {
        DataContractJsonSerializer serializer = new DataContractJsonSerializer(settings.GetType());
        serializer.WriteObject(fs, settings);
      }
    }



    public static T ReadSettingsFile<T>()
      where T : SettingsBase, new()
    {
      if (!File.Exists("Settings.json"))
      {
        return new T();
      }

      try
      {
        using (FileStream fs = new FileStream("Settings.json", FileMode.Open))
        {
          DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
          return serializer.ReadObject(fs) as T;
        }
      }
      catch
      {
        return new T();
      }
    }
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
    public const double DEFAULT_EDITOR_FONT_SIZE = 14.667;


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



    public Settings()
      : base()
    {
      this.ShowEditorLineNumbers = true;
      this.WrapEditorText = false;
      this.EditorFontSize = DEFAULT_EDITOR_FONT_SIZE;
    }
  }
}
