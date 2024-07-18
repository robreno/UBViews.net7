namespace UBViews.Converters;

using System.Globalization;
using CommunityToolkit.Maui.Core.Primitives;

using UBViews.Models;

public class FocusedEventArgsConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
        
        var args = (FocusEventArgs)value;
        var editor = (Editor)args.VisualElement;
        var id = editor.StyleId;
        var name = editor.ClassId;
        return value;
    }

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
        throw new NotImplementedException();
	}
}
