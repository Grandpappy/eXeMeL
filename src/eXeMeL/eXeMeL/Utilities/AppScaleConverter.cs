using System;
using System.Globalization;
using System.Windows.Data;

namespace eXeMeL.Utilities
{
  public class AppScaleConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is double scale)
        return $"{(int)Math.Round(scale * 100)}%";
      return "100%";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
