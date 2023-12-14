namespace UBViews.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Views;

public partial class PopupViewModel : BaseViewModel
{
    public Popup popupPage;

    [RelayCommand]
    async Task ClosePopup(object obj)
    {
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
            await App.Current.MainPage.DisplayAlert("Exception raised in MainViewModel.NavigateTo => ",
                ex.Message, "Cancel");
        }
    }
}
