namespace UBViews.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.FSharp.Collections;

using System.Xml.Linq;
using System.Linq;
using Microsoft.FSharp.Core;

using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Views;

using UBViews.Services;
using UBViews.Models.Query;
using UBViews.Models.Ubml;
using UBViews.Helpers;
using UBViews.Views;

using UBViews.Controls.Help;
using UBViews.ViewModels;


public partial class HelpViewModel : BaseViewModel
{
    public ContentPage contentPage;

    public HelpViewModel()
    {

    }

    [RelayCommand]
    async Task HelpPageAppearing()
    {
        try 
        {
            Title = "Help Popups";
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in MainViewModel.NavigateTo => ",
                ex.Message, "Cancel");
        }
    }

    [RelayCommand]
    async Task ShowPopup(string target)
    {
        try
        {
            Popup popup = null;
            if (target == "SettingsOverviewPopup")
            {
                popup = new SettingsOverviewPopup(new PopupViewModel());
            }
            else if (target == "ContactsOverviewPopup")
            {
                popup = new ContactsOverviewPopup(new PopupViewModel());
            }
            else if (target == "SharingOverviewPopup")
            {
                popup = new SharingOverviewPopup(new PopupViewModel());
            }
            else if (target == "AudioOverviewPopup")
            {
                popup = new AudioOverviewPopup(new PopupViewModel());
            }
            else if (target == "NavigationOverviewPopup")
            {
                popup = new NavigationOverviewPopup(new PopupViewModel());
            }
            else if (target == "ListeningOverviewPopup")
            {
                popup = new ListenOverviewPopup(new PopupViewModel());
            }
            else if (target == "SearchingOverviewPopup")
            {
                popup = new SearchOverviewPopup(new PopupViewModel());
            }
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Shell.Current.CurrentPage.ShowPopup(popup);
            });
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in MainViewModel.NavigateTo => ",
                ex.Message, "Cancel");
        }
    }
}
