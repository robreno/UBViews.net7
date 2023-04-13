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
    /// <summary>
    /// 
    /// </summary>
    public ContentPage contentPage;

    /// <summary>
    /// 
    /// </summary>
    IFileService fileService;

    IAppSettingsService settingsService;
    public PartsViewModel(IFileService fileService, IAppSettingsService settingsService)
    {
        this.fileService = fileService;
        this.settingsService = settingsService;
    }

    [ObservableProperty]
    bool isRefreshing;

    [ObservableProperty]
    bool showPaperContents;

    [RelayCommand]
    async Task NavigateTo(string id)
    {
        if (IsBusy)
            return;

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
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }
}
