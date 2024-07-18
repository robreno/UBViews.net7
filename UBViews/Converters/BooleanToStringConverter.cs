using System.Globalization;

namespace UBViews.Converters
{
    /// <summary>
    /// 
    /// </summary>
    public partial class BooleanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? "False" : ((bool)value ? "True" : "False");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
