using System.Globalization;

namespace UBViews.Converters;

/// <summary>
/// Value converter that translates true to <see cref="Visibility.Collapsed"/> 
/// and false to <see cref="Visibility.Visible"/>.
/// </summary>
public sealed class BooleanNegationToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (value is bool && !(bool)value) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value is bool && !(bool)value) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is Visibility && (Visibility)value != Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility && (Visibility)value != Visibility.Visible;
    }
}
