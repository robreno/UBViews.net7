namespace UBViews.Models.Audio;
public class MediaStatePair
{
    public MediaStatePair(string previousState, string newState)
    { PreviousState = previousState; NewState = NewState; }
    public string PreviousState { get; set; }
    public string NewState { get; set; }
}
