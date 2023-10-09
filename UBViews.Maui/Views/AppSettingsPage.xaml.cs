namespace UBViews.Views;

using UBViews.Services;
using UBViews.ViewModels;

// TODO: I don't think any of the code behind is used now
// as it is all done in the viewmodel. Need to cleanup
// and remove it to simplify. First confirm it is never
// used.

/// <summary>
/// 
/// </summary>
public partial class AppSettingsPage : ContentPage
{
    /// <summary>
    /// 
    /// </summary>
    bool useCaching;
    int maxNumber;
    bool showPids;
    double lineHeight;
    int screenSize;

    bool useCachingDirty;
    bool maxNumDirty;
    bool showPidsDirty;
    bool lineHeightDirty;
    //bool screenSizeDirty;

    /// <summary>
    /// 
    /// </summary>
    private IAppSettingsService appSettingsService;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="vm"></param>
    /// <param name="appSettingsService"></param>
    public AppSettingsPage(AppSettingsViewModel vm, IAppSettingsService appSettingsService)
    {
        InitializeComponent();
        BindingContext = vm;
        vm.contentPage = this;
        this.appSettingsService = appSettingsService;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnLoaded(object sender, EventArgs e)
    {
        try
        {
            useCaching = await appSettingsService.Get("use_caching", false);
            maxNumber = await appSettingsService.Get("max_query_results", 50);
            this.maxNumberEntry.Text = maxNumber.ToString("0");
            showPids = await appSettingsService.Get("show_reference_pids", false);
            pidsCheckBox.IsChecked = showPids;
            lineHeight = await appSettingsService.Get("line_height", 1.0);
            lineHeightStepper.Value = lineHeight;
            screenSize = await appSettingsService.Get("window_size", 2);
            windowSizePicker.SelectedIndex = screenSize;

        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnMaxNumberStepperValueChanged(object sender, ValueChangedEventArgs e)
    {
        try
        {
            maxNumber = Convert.ToInt32(e.NewValue);
            maxNumDirty = true;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnLineHeightStepperValueChanged(object sender, ValueChangedEventArgs e)
    {
        try 
        {
            lineHeight = e.NewValue;
            lineHeightDirty = true;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnShowPidsCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        try
        {
            showPids = pidsCheckBox.IsChecked;
            showPidsDirty = true;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
    }

    private async void OnUseCachingCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        try
        {
            useCaching = cachingCheckBox.IsChecked;
            useCachingDirty = true;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnSaveSettingsButtonClicked(object sender, EventArgs e)
    {
        try
        {
            if (maxNumDirty)
            {
                Preferences.Default.Set("max_query_results", maxNumber);
                await appSettingsService.SetCache("max_query_results", maxNumber);
            }
            if (showPidsDirty)
            {
                Preferences.Default.Set("show_reference_pids", showPids);
                await appSettingsService.SetCache("show_reference_pids", showPids);
            }
            if (lineHeightDirty)
            {
                Preferences.Default.Set("line_height", lineHeight);
                await appSettingsService.SetCache("line_height", Convert.ToDouble(lineHeight));
            }
            await App.Current.MainPage.DisplayAlert("Settings", "Settings were cached!", "Ok");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
    }
}