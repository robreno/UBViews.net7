using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;

namespace UBViews.Models.Audio;
public class MediaStatePair
{
    // See: https://www.xamboy.com/2021/08/17/using-state-machine-in-xamarin-forms-part-1/
    // See: https://github.com/CrossGeeks/StateMachineVideoPlayerXFSample
    public MediaStatePair(string previousState, string newState)
    { 
        PreviousState = previousState; 
        NewState = NewState; 
    }

    public string PreviousState { get; set; }
    public string NewState { get; set; }

    public string GetState(MediaElementState mes)
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
