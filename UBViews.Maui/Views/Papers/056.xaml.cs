// ------------------------------------------------------------------------------
// <auto-generated>
//     This code,including the associated .xaml file, was generated by tools.
//     Runtime Version T4CodeGen for .cs files: 1.0.0.0
//     Runtime Version TransformUblm for .xaml: 1.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
using System.ComponentModel;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Core.Primitives;
using UBViews.ViewModels;
using UBViews.AttachedProperties;

namespace UBViews.Views;

/// <summary>
/// 
/// </summary>
public partial class _056 : ContentPage
{
	const string loadOnlineMp3 = "Load Online MP3";
	const string loadHls = "Load HTTP Live Stream (HLS)";
	const string loadLocalResource = "Load Local Resource";
	const string resetSource = "Reset Source to null";
	const string partTitle = "Foreword Material";
    const string paperTitle = "Foreword";
    const string paperAuthor = "Divine Counselor";

	/// <summary>
    /// CStor
    /// </summary>
    /// <param name="vm"></param>
	public _056(XamlPaperViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
		vm.contentPage = this;
		vm.Title = partTitle;
        vm.PaperTitle = paperTitle;
        vm.PaperAuthor = paperAuthor;
		mediaElement.PropertyChanged += MediaElement_PropertyChanged;
	}

	/// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
	void MediaElement_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == MediaElement.DurationProperty.PropertyName)
        {
            //logger.LogInformation("Duration: {newDuration}", mediaElement.Duration);
            //positionSlider.Maximum = mediaElement.Duration.TotalSeconds;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
	void OnMediaOpened(object sender, EventArgs e)
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnStateChanged(object sender, MediaStateChangedEventArgs e)
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnMediaFailed(object sender, MediaFailedEventArgs e)
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnMediaEnded(object sender, EventArgs e)
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnPositionChanged(object sender, MediaPositionChangedEventArgs e)
    {
        //positionSlider.Value = e.Position.TotalSeconds;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnSeekCompleted(object sender, EventArgs e)
    {
        if (mediaElement.Speed >= 1)
        {
            mediaElement.Speed -= 1;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnSpeedPlusClicked(object sender, EventArgs e)
    {
        if (mediaElement.Speed < 10)
        {
            mediaElement.Speed += 1;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnVolumeMinusClicked(object sender, EventArgs e)
    {
        if (mediaElement.Volume >= 0)
        {
            if (mediaElement.Volume < .1)
            {
                mediaElement.Volume = 0;

                return;
            }

            mediaElement.Volume -= .1;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnVolumePlusClicked(object sender, EventArgs e)
    {
        if (mediaElement.Volume < 1)
        {
            if (mediaElement.Volume > .9)
            {
                mediaElement.Volume = 1;

                return;
            }

            mediaElement.Volume += .1;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnPlayClicked(object sender, EventArgs e)
    {
        mediaElement.Play();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnPauseClicked(object sender, EventArgs e)
    {
        mediaElement.Pause();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnStopClicked(object sender, EventArgs e)
    {
        mediaElement.Stop();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnMuteClicked(object sender, EventArgs e)
    {
        mediaElement.ShouldMute = !mediaElement.ShouldMute;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void OnUnloaded(object sender, EventArgs e)
    {
        // Stop and cleanup MediaElement when we navigate away
        mediaElement.Handler?.DisconnectHandler();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void Slider_DragCompleted(object sender, EventArgs e)
    {
        ArgumentNullException.ThrowIfNull(sender);

        //var newValue = ((Slider)sender).Value;
        //mediaElement.SeekTo(TimeSpan.FromSeconds(newValue));
        //mediaElement.Play();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void Slider_DragStarted(object sender, EventArgs e)
    {
        mediaElement.Pause();
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    async void ChangeSourceClicked(System.Object sender, System.EventArgs e)
    {
        var result = await DisplayActionSheet("Choose a source", "Cancel", null,
            loadOnlineMp3, loadLocalResource, resetSource);

        switch (result)
        {
            case loadOnlineMp3:
                mediaElement.Source =
                    MediaSource.FromUri(
                        "https://s3.amazonaws.com/urantia/media/en/U0.mp3");
                return;

            case resetSource:
                mediaElement.Source = null;
                return;

            case loadLocalResource:
                if (DeviceInfo.Platform == DevicePlatform.MacCatalyst
                    || DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    mediaElement.Source = MediaSource.FromResource("UB.000.mp3");
                }
                else if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    mediaElement.Source = MediaSource.FromResource("UB.000.mp3");
                }
                else if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    mediaElement.Source = MediaSource.FromResource("UB.000.mp3");
                }
                return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    async void ChangeAspectClicked(System.Object sender, System.EventArgs e)
    {
        var resultAspect = await DisplayActionSheet("Choose aspect ratio",
            "Cancel", null, Aspect.AspectFit.ToString(),
            Aspect.AspectFill.ToString(), Aspect.Fill.ToString());

        if (resultAspect.Equals("Cancel"))
        {
            return;
        }

        if (!Enum.TryParse(typeof(Aspect), resultAspect, true, out var aspectEnum)
            || aspectEnum is null)
        {
            await DisplayAlert("Error", "There was an error determining the selected aspect",
                "OK");

            return;
        }

        mediaElement.Aspect = (Aspect)aspectEnum;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_Clicked(object sender, EventArgs e)
    {
        Resources["IsVisiblePidStyle"] = Resources["IsNotVisiblePidStyle"];
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnDisappearing(object sender, EventArgs e)
    {
        mediaElement.Stop();
    }
}

