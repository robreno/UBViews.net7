namespace UBViews.Models.Audio;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class AudioFlag
{
    public enum AudioStatus { Off, On };
    public AudioStatus State { get; set; }

    public AudioFlag()
    {
        SetAudioStatus(AudioStatus.Off);
    }
    public AudioFlag(AudioStatus status)
    {
        SetAudioStatus(status);
    }
    public void SetAudioStatus(AudioStatus status)
    {
        switch(status)
        {
            case AudioStatus.Off:
                State = AudioStatus.Off;
                break;
            case AudioStatus.On:
                State = AudioStatus.On;
                break;
        }
    }

    public string GetAudioStatusString()
    {
        string _status = string.Empty;
        switch (State)
        {
            case AudioStatus.Off:
                 _status = "Off";
                break;
            case AudioStatus.On:
                _status = "On";
                break;
        }
        return _status;
    }
    public AudioStatus GetAudioStatus()
    {
        return State;
    }
}
