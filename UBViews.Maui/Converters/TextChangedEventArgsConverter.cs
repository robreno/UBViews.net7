namespace UBViews.Converters;

using System.Globalization;
using CommunityToolkit.Maui.Core.Primitives;

using UBViews.Models;

public class TextChangedEventArgsConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
        
        var args = (TextChangedEventArgs)value;
        var newText = args.NewTextValue;
        var oldText = args.OldTextValue;
        var editor = GetParameter(parameter);
        //var dto = new TextChangedEventArgsDto() 
        //{ 
        //    Sender = editor, 
        //    NewText = newText, 
        //    OldText = oldText, 
        //    Parameter = parameter 
        //};
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
