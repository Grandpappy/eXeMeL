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
  internal static class SettingsIO
  {
    public static void SaveSettings(SettingsBase settings)
    {
      using (var s = new MemoryStream())
      {
        using (var registryKey = RegistryAccess.OpenRegistryKey())
        {
          var serializer = new DataContractJsonSerializer(settings.GetType());
          serializer.WriteObject(s, settings);

          var value = Encoding.UTF8.GetString(s.ToArray());

          registryKey.SetValue("Settings", value);
        }
      }
    }



    public static T LoadSettings<T>()
      where T : SettingsBase, new()
    {
      try
      {
        using (var registryKey = RegistryAccess.OpenRegistryKey())
        {
          var value = registryKey.GetValue("Settings") as string;
          using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(value)))
          {
            var serializer = new DataContractJsonSerializer(typeof(T));
            return serializer.ReadObject(memoryStream) as T;
          }
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
