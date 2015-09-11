using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace TemperatureReader.ClientApp.Converters
{
  /// <summary>
  /// Converts true to the value of parameter
  /// </summary>
  public class BoolToVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, string language)
    {
      if (parameter == null)
      {
        parameter = Visibility.Visible;
      }

      if (value is bool)
      {
        var bValue = (bool)value;
        var visibility = (Visibility)Enum.Parse(typeof(Visibility), parameter.ToString(), true);
        if (bValue) return visibility;
        return visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
      }
      return parameter;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
      throw new NotImplementedException();
    }
  }
}
