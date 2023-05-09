#define MEDIA_STATE
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;

namespace UBViews.Models.Audio;
public class MediaStatePair
{
    // See: https://learn.microsoft.com/th-th/dotnet/framework/windows-workflow-foundation/state-machine-workflows?WT.mc_id=DOP-MVP-5003636
    // See: https://www.xamboy.com/2021/08/17/using-state-machine-in-xamarin-forms-part-1/
    // See: https://github.com/CrossGeeks/StateMachineVideoPlayerXFSample

    protected Stack<string> audioStateStack = new Stack<string>();

    public MediaStatePair()
    { 
        PreviousState = "None"; 
        CurrentState = "None"; 
    }

    public string PreviousState { get; set; }
    public string CurrentState { get; set; }

    public void SetState(string state)
    {
        PreviousState = CurrentState;
        CurrentState = state;
        audioStateStack.Push(state);
    }

    public string PeekState()
    {
        return audioStateStack.Peek();
    }

    public string GetState()
    {
        return audioStateStack.Pop();
    }

    public MediaElementState GetState(string state)
    {
        MediaElementState mes = MediaElementState.None;
        switch (state)
        {
            case "None":
                mes = MediaElementState.None;
                break;
            case "Opening":
                mes = MediaElementState.Opening;
                break;
            case "Buffering":
                mes = MediaElementState.Buffering;
                break;
            case "Playing":
                mes = MediaElementState.Playing;
                break;
            case "Paused":
                mes = MediaElementState.Paused;
                break;
            case "Stopped":
                mes = MediaElementState.Stopped;
                break;
            case "Failed":
                mes = MediaElementState.Failed;
                break;
        }
        return mes;
    }
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
