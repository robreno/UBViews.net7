using System.Globalization;
using CommunityToolkit.Maui.Core.Primitives;

namespace UBViews.Converters;
public class MediaStateChangedEventArgsConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
        var args = value as MediaStateChangedEventArgs;
        string previousState = GetState(args.PreviousState);
        string newState = GetState(args.NewState);
        return newState;
    }

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
        throw new NotImplementedException();
	}

    private string GetState(MediaElementState mes)
    {
        string state = string.Empty;
        switch (mes)
        {
            case MediaElementState.None:
                state = "None";
                break;
            case MediaElementState.Opening:
                state = "Opening";
                break;
            case MediaElementState.Buffering:
                state = "Buffering";
                break;
            case MediaElementState.Playing:
                state = "Playing";
                break;
            case MediaElementState.Paused:
                state = "Paused";
                break;
            case MediaElementState.Stopped:
                state = "Stopped";
                break;
            case MediaElementState.Failed:
                state = "Failed";
                break;
        }
        return state;
    }
}
