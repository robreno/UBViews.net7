using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UBViews.Services;
using UBViews.Models;
using UBViews.Views;

namespace UBViews.ViewModels;

public partial class PartsViewModel : BaseViewModel
{
    #region   Private Data Members
    /// <summary>
    /// 
    /// </summary>
    public ContentPage contentPage;

    /// <summary>
    /// 
    /// </summary>
    IFileService fileService;

    /// <summary>
    /// 
    /// </summary>
    IAppSettingsService settingsService;

    /// <summary>
    /// 
    /// </summary>
    IAudioService audioService;

    readonly string _class = "PartsViewModel";
    #endregion

    #region   Constructor
    public PartsViewModel(IFileService fileService, IAppSettingsService settingsService, IAudioService audioService)
    {
        this.fileService = fileService;
        this.settingsService = settingsService;
        this.audioService = audioService;
    }
    #endregion

    #region  Observable Properties
    [ObservableProperty]
    bool isRefreshing;

    [ObservableProperty]
    bool showPaperContents;
    #endregion

    #region  Relay Commands
    [RelayCommand]
    async Task PartsPageAppearing()
    {
        string _method = "PartsPageAppearing";
        try
        {
            if (contentPage == null)
            {
                return;
            }

            //mediaElement.Source = MediaSource.FromResource("BookIntro.mp3");
            await audioService.SetContentPageAsync(contentPage);

            string titleMessage = $"Parts of Book";
            Title = titleMessage;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
        }
    }

    [RelayCommand]
    async Task PartsPageDisappearing()
    {
        string _method = "PartsPageDisappearing";
        try
        {

        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
        }
    }

    [RelayCommand]
    async Task PartsPageUnloaded()
    {
        string _method = "PartsPageUnloaded";
        try
        {

        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
        }
    }

    [RelayCommand]
    async Task TappedGesture(string action)
    {
        string _method = "TappedGesture";
        try
        {
            IsBusy = true;

            if (contentPage == null)
            {
                return;
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    async Task NavigateTo(string id)
    {
        if (IsBusy)
            return;

        string _method = "NavigateTo";
        try
        {
            base.IsBusy = true;

            ShowPaperContents = await settingsService.Get("show_paper_contents", false);

            int partId = Int32.Parse(id);
            string targetName = string.Empty;
            if (partId == 0)
            {
                if (ShowPaperContents)
                {
                    targetName = nameof(ContentTitlesPage);
                }
                else
                {
                    targetName = nameof(_000);
                }
            }
            else
            {
                targetName = nameof(PartTitlesPage);
            }

            int pid = 0;
            switch (partId)
            {
                case 0:
                    pid = 0;
                    break;
                case 1:
                    pid = 1;
                    break;
                case 2:
                    pid = 32;
                    break;
                case 3:
                    pid = 57;
                    break;
                case 4:
                    pid = 120;
                    break;
            }

            PaperDto paperDto = await fileService.GetPaperDtoAsync(pid);

            await Shell.Current.GoToAsync(targetName, new Dictionary<string, object>()
            {
                {"PaperDto", paperDto }
            });
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }
    #endregion
}
