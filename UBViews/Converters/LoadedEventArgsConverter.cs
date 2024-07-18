namespace UBViews.Converters;

using System.Globalization;
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Mvvm.Input;

public class LoadedEventArgsConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
        var args = (EventArgs)value;
        var editor = GetParameter(parameter);
        return value;
    }

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
        throw new NotImplementedException();
	}

    Editor GetParameter(object parameter)
    {
        if (parameter is Editor)
            return (Editor)parameter;
        return null;
    }
}
