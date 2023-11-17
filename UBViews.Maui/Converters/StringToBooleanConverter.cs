using System.Globalization;

namespace UBViews.Converters
{
    /// <summary>
    /// 
    /// </summary>
    public partial class StringToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? false : ((string)value == "true" ? true : false);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
