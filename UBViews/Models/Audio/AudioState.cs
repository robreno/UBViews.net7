namespace UBViews.Models.Audio
{
    // See: https://learn.microsoft.com/th-th/dotnet/framework/windows-workflow-foundation/state-machine-workflows?WT.mc_id=DOP-MVP-5003636
    // See: https://github.com/dotnet-state-machine/stateless
    // See: https://www.xamboy.com/2021/08/17/using-state-machine-in-xamarin-forms-part-1/
    // See: https://github.com/CrossGeeks/StateMachineVideoPlayerXFSample
    public enum AudioState
    {
        Idle,
        Playing,
        Paused,
        Stopped
    }
}
