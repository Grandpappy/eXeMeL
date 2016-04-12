using eXeMeL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;
using System.ComponentModel;

namespace eXeMeL.View
{
  /// <summary>
  /// Interaction logic for SettingsView.xaml
  /// </summary>
  public partial class SettingsView : UserControl
  {
    public SettingsView()
    {
      InitializeComponent();
    }



    private void ResetFontSizeButton_Click(object sender, RoutedEventArgs e)
    {
      (this.DataContext as Settings).EditorFontSize = Settings.DefaultEditorFontSize;
    }



    public string CurrentVersion
    {
      get
      {
        return ApplicationVersionControl.GetPublishedVersion().ToString();
      }
    }
  }



  public class EnumerationExtension : MarkupExtension
  {
    private Type _enumType;


    public EnumerationExtension(Type enumType)
    {
      if (enumType == null)
        throw new ArgumentNullException("enumType");

      EnumType = enumType;
    }

    public Type EnumType
    {
      get { return _enumType; }
      private set
      {
        if (_enumType == value)
          return;

        var enumType = Nullable.GetUnderlyingType(value) ?? value;

        if (enumType.IsEnum == false)
          throw new ArgumentException("Type must be an Enum.");

        _enumType = value;
      }
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      var enumValues = Enum.GetValues(EnumType);

      return (
        from object enumValue in enumValues
        where DisplayInSettings(enumValue) 
        select new EnumerationMember
        {
          Value = enumValue,
          Description = GetDescription(enumValue)
        }).ToArray();
    }

    private string GetDescription(object enumValue)
    {
      var descriptionAttribute = EnumType
        .GetField(enumValue.ToString())
        .GetCustomAttributes(typeof(DescriptionAttribute), false)
        .FirstOrDefault() as DescriptionAttribute;


      return descriptionAttribute != null
        ? descriptionAttribute.Description
        : enumValue.ToString();
    }

    private bool DisplayInSettings(object enumValue)
    {
      var attribute = EnumType
        .GetField(enumValue.ToString())
        .GetCustomAttributes(typeof(DoNotDisplayInSettingsAttribute), false)
        .FirstOrDefault() as DoNotDisplayInSettingsAttribute;

      if (attribute == null)
        return true;
      else
        return false;
    }

    public class EnumerationMember
    {
      public string Description { get; set; }
      public object Value { get; set; }
    }
  }

}
