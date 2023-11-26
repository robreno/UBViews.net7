namespace UBViews.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UBViews.Services;

using UBViews.Models.Audio;
public partial class AudioService
{
    IAudioService audioService;

    private readonly string _className = "AudioService";
    public AudioService(IAudioService audioService)
    {
        this.audioService = audioService;
    }

    public async Task<AudioMarker> GetAtAsync(int index)
    {
        string _methodName = "GetAtAsync";

        try
        {
            var marker = await audioService.GetAt(index);
            return marker;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_className}.{_methodName} => ", ex.Message, "Ok");
            return null;
        }
    }

    public async Task ClearAsync()
    {
        string _methodName = "ClearAsync";

        try
        {
            await audioService.Clear();
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_className}.{_methodName} => ", ex.Message, "Ok");
            return;
        }
    }

    public async Task InsertAsync(AudioMarker mediaMarker)
    {
        string _methodName = "InsertAsync";

        try
        {
            await audioService.Insert(mediaMarker);
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_className}.{_methodName} => ", ex.Message, "Ok");
            return;
        }
    }
}
