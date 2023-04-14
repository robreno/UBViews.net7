using System.Collections.ObjectModel;
using UBViews.Models;
using UBViews.Services;
using UBViews.State;
using UBViews.Views;

namespace UBViews.ViewModels
{
    [QueryProperty(nameof(PaperDto), "PaperDto")]
    public partial class PartTitlesViewModel : BaseViewModel
    {
        public ContentPage rootPage;

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
        public ObservableCollection<PaperDto> PaperDtos { get; } = new();

        /// <summary>
        /// 
        /// </summary>
        public TitlesFilter FilterEx { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileService"></param>
        /// <param name="settingsService"></param>
        public PartTitlesViewModel(IFileService fileService, IAppSettingsService settingsService)
        {
            this.fileService = fileService;
            this.settingsService = settingsService;
            this.FilterEx = new TitlesFilter();
        }

        [ObservableProperty]
        bool isRefreshing;

        [ObservableProperty]
        PaperDto paperDto;

        [ObservableProperty]
        string partHeading;

        [ObservableProperty]
        bool showPaperContents;

        [RelayCommand]
        async Task PartTitlesPageAppearing(PaperDto dto)
        {
            try
            {
                IsBusy = true;

                if (PaperDtos.Count != 0)
                    return;

                ShowPaperContents = await settingsService.Get("show_paper_contents", false);

                int partId = dto.PartId;
                string partTitle = dto.PartTitle;
                this.Title = partTitle;

                switch (partId)
                {
                    case 1:
                        PartHeading = "Part I";
                        break;
                    case 2:
                        PartHeading = "Part II";
                        break;
                    case 3:
                        PartHeading = "Part III";
                        break;
                    case 4:
                        PartHeading = "Part IV";
                        break;
                };

                await FilterEx.Create();
                foreach (var title in FilterEx.PaperDtos.Where(t => t.PartId == partId))
                {
                    PaperDtos.Add(title);
                }
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
        async Task GetDtos(PaperDto dto)
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                int partId = dto.PartId;
                string partTitle = dto.Title;
                this.Title = partTitle;

                switch (partId)
                {
                    case 1:
                        PartHeading = "Part I";
                        break;
                    case 2:
                        PartHeading = "Part II";
                        break;
                    case 3:
                        PartHeading = "Part III";
                        break;
                    case 4:
                        PartHeading = "Part IV";
                        break;
                };

                foreach (var title in FilterEx.PaperDtos.Where(t => t.PartId == partId))
                {
                    PaperDtos.Add(title);
                }

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
        async Task GoToDetails(PaperDto dto)
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

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
                IsRefreshing = false;
            }
        }
    }
}
