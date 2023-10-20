using CommunityToolkit.Maui.Alerts;
using mauiCore = CommunityToolkit.Maui.Core;

using UBViews.Models;
using UBViews.Services;
using UBViews.Views;

namespace UBViews.ViewModels
{
    public partial class PaperTitlesViewModel : BaseViewModel
    {
        IFileService fileService;
        IAppSettingsService settingsService;
        public ContentPage contentPage { get; set; }
        public PaperTitlesViewModel(IFileService fileService, IAppSettingsService settingsService)
        {
            this.fileService = fileService;
            this.settingsService = settingsService;
        }

        [ObservableProperty]
        bool isRefreshing;

        [ObservableProperty]
        bool showPaperContents;

        [RelayCommand]
        async Task PaperTitlesPageAppearing()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                IsRefreshing = true;

                Title = "Titles of the Papers";
                ShowPaperContents = await settingsService.Get("show_paper_contents", false);

            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        async Task TappedGesture(string id)
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                IsRefreshing = true;

                string className = id;
                string[] arr = id.Split('_', StringSplitOptions.RemoveEmptyEntries);
                int paperId = Int32.Parse(arr.ElementAt(0));
                PaperDto paperDto = await fileService.GetPaperDtoAsync(paperId);

                await GoToDetails(paperDto);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
                return;
            }
            finally 
            { 
                IsBusy = false; 
                IsRefreshing = false; 
            }
        }

        [RelayCommand]
        async Task GoToDetails(PaperDto dto)
        {
            if (IsBusy)
                return;

            try
            {
                if (dto == null)
                    return;

                string className;
                if (ShowPaperContents)
                {
                    className = nameof(ContentTitlesPage);
                }
                else
                {
                    className = "_" + dto.Id.ToString("000");
                }

                await Shell.Current.GoToAsync(className, true, new Dictionary<string, object>
                {
                    {"PaperDto", dto }
                });
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
                return;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
