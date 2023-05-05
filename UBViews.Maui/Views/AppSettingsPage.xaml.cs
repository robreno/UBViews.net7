using UBViews.Services;
using UBViews.ViewModels;

namespace UBViews.Views;

public partial class AppSettingsPage : ContentPage
{
    private int maxNumber;
    private bool showPids;
    private double lineHeight;

    private bool maxNumDirty;
    private bool showPidsDirty;
    private bool lineHeightDirty;

    private IAppSettingsService appSettingsService;

    public AppSettingsPage(AppSettingsViewModel vm, IAppSettingsService appSettingsService)
    {
        InitializeComponent();
        BindingContext = vm;
        this.appSettingsService = appSettingsService;
    }

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