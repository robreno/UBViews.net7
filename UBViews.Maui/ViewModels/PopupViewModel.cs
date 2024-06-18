namespace UBViews.ViewModels;

using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

public partial class PopupViewModel : BaseViewModel
{
    #region Private Data Members
    public Popup popupPage;

    public VerticalStackLayout vslPopupContent;

    readonly string _class = "PopupViewModel";
    #endregion

    #region Constructor
    public PopupViewModel()
    {
 
    }
    #endregion

    #region Observable Properties
    #endregion

    #region Relay Commands

    [RelayCommand]
    async Task Tapped(string url)
    {
        string _method = "Tapped";
        try
        {
            string _url = url;
            await Launcher.OpenAsync(_url);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    [RelayCommand]
    async Task ClosePopup(object obj)
    {
        string _method = "ClosePopup";
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (popupPage != null)
                {
                    popupPage.Close();
                }
            });
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }
    #endregion

    #region Private Methods
    private async Task SendToastAsync(string message)
    {
        string _method = "SendToastAsync";
        try
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                ToastDuration duration = ToastDuration.Short;
                double fontSize = 14;
                var toast = Toast.Make(message, duration, fontSize);
                await toast.Show(cancellationTokenSource.Token);
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }
    #endregion
}
