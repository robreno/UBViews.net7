using UBViews.Services;
using UBViews.ViewModels;

namespace UBViews.Views;

/// <summary>
/// 
/// </summary>
public partial class AppSettingsPage : ContentPage
{
    /// <summary>
    /// 
    /// </summary>
    private int maxNumber;
    private bool showPids;
    private double lineHeight;

    private bool maxNumDirty;
    private bool showPidsDirty;
    private bool lineHeightDirty;

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
            maxNumber = await appSettingsService.Get("max_query_results", 50);
            maxNumberEntry.Text = maxNumber.ToString("0");
            showPids = await appSettingsService.Get("show_reference_pids", false);
            pidsCheckBox.IsChecked = showPids;
            lineHeight = await appSettingsService.Get("line_height", 1.0);
            lineHeightStepper.Value = lineHeight;
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