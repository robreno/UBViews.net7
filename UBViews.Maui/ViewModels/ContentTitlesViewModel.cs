using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UBViews.Services;
using UBViews.Models;
using UBViews.Views;

namespace UBViews.ViewModels;

[QueryProperty(nameof(PaperDto), "PaperDto")]
public partial class ContentTitlesViewModel : BaseViewModel
{
    public ContentPage contentPage;
    IFileService fileService;

    public ObservableCollection<SectionTitleDto> SectionTitlesDtos { get; } = new();

    public ContentTitlesViewModel(IFileService fileService)
    {
        this.fileService = fileService;
    }

    [ObservableProperty]
    bool isRefreshing;

    [ObservableProperty]
    PaperDto paperDto;

    [ObservableProperty]
    ContentDto contentTitles;

    [ObservableProperty]
    string paperTitle;

    [ObservableProperty]
    string paperAuthor;

    [ObservableProperty]
    string paperNumber;

    [RelayCommand]
    public async Task ContentTitlesPageAppearing(PaperDto dto)
    {
        try
        {
            IsBusy = true;

            if (SectionTitlesDtos.Count != 0)
                return;

            PaperTitle = dto.Title;
            PaperAuthor = dto.Author;
            PaperNumber = dto.Id.ToString("0");
            int partId = dto.PartId;
            string partTitle = dto.PartTitle;
            this.Title = partTitle;

            ContentTitles = await fileService.GetContentDtoAsync(dto.Id);

            foreach (var section in ContentTitles.SectionTitles)
            {
                SectionTitlesDtos.Add(section);
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
    async Task TappedGesture(SectionTitleDto dto)
    {
        try
        {
            string uid = dto.Uid; // 002.000.000.001
            int paperId = Int32.Parse(uid.Substring(4, 3));
            PaperDto paperDto = await fileService.GetPaperDtoAsync(paperId);
            paperDto.ScrollTo = true;
            paperDto.Uid = uid;
            await GoToDetails(paperDto);
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

            string className = "_" + dto.Id.ToString("000");

            await Shell.Current.GoToAsync(className, true, new Dictionary<string, object>
            {
                {"PaperDto", dto }
            });
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
}
