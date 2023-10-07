namespace UBViews.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using Microsoft.Maui.Controls;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Platform;

using UBViews.Models;
using UBViews.Models.Query;
using UBViews.Models.Ubml;
using UBViews.Services;


[QueryProperty(nameof(LocationsDto), "LocationsDto")]
public partial class QueryResultViewModel : BaseViewModel
{
    public ContentPage contentPage;
    public ObservableCollection<QueryLocationDto> QueryLocations { get; } = new();

    Dictionary<string, Span> _spans = new Dictionary<string, Span>();

    IFileService fileService;

    public QueryResultViewModel(IFileService fileService)
    {
        this.fileService = fileService;
    }

    [ObservableProperty]
    string pageTitle;

    [ObservableProperty]
    bool isRefreshing;

    [ObservableProperty]
    QueryResultLocationsDto locationsDto;

    [ObservableProperty]
    string queryString;

    [ObservableProperty]
    int queryHits;

    [ObservableProperty]
    bool showReferencePids;

    [ObservableProperty]
    bool isScrollToLabel;

    [ObservableProperty]
    string scrollToLabelName;

    [RelayCommand]
    async Task QueryResultAppearing(QueryResultLocationsDto dto)
    {
        string methodName = "QueryResultAppearing";
        try
        {
            if (dto == null)
            {
                return;
            }

            QueryString = dto.QueryString;
            QueryHits = dto.Hits;

            string titleMessage = $"Query Result {queryHits} hits ...";
            Title = titleMessage;

            var locations = dto.QueryLocations;
            foreach (var location in locations)
            {
                QueryLocations.Add(location);
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task QueryResultLoaded()
    {
        string methodName = "QueryResultLoaded";
        try
        {
            IsBusy = true;

            var locations = QueryLocations;
            if (locations == null)
            {
                return;
            }
            await LoadXamlEx();
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
            return;
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    async Task TappedGesture(string lableName)
    {
        try
        {
            IsBusy = true;

            var arry = lableName.Split('_', StringSplitOptions.RemoveEmptyEntries);
            var paperId = Int32.Parse(arry[0]);
            var seqId = Int32.Parse(arry[1]);
            Paragraph paragraph = await fileService.GetParagraphAsync(paperId, seqId);
            PaperDto paperDto = await fileService.GetPaperDtoAsync(paperId);
            var uid = paragraph.Uid;
            var pid = paragraph.Pid;
            paperDto.ScrollTo = true;
            paperDto.SeqId = seqId;
            paperDto.Pid = pid;
            paperDto.Uid = uid;
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

    #region Helper Methods
    private async Task LoadXaml()
    {
        try
        {
            // See: https://learn.microsoft.com/en-us/dotnet/maui/xaml/runtime-load
            /*
            string navigationButtonXAML = "<Button Text=\"Navigate\" />";
            Button navigationButton = new Button().LoadFromXaml(navigationButtonXAML);
            stackLayout.Add(navigationButton); ...
            */

            var contentScrollView = contentPage.FindByName("queryResultScrollView") as ScrollView;
            var contentVSL = contentPage.FindByName("contentVerticalStackLayout") as VerticalStackLayout;
            var locations = QueryLocations;
            foreach (var location in locations)
            {
                var id = location.Id;
                var pid = location.Pid;
                var arry = id.Split('.');
                var paperId = Int32.Parse(arry[0]);
                var seqId = Int32.Parse(arry[1]);
                var paragraph = await fileService.GetParagraphAsync(paperId, seqId);
                var text = paragraph.Text;
                var paraStyle = paragraph.ParaStyle;

                // Study the .NET Maui ScrollView examples

                var labelName = "_" + paperId.ToString("000") + "_" + seqId.ToString("000");

                // See: https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/label
                FormattedString fs = await CreateFormattedString(paragraph);

                Label label = new Label { FormattedText = fs };
                TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.SetBinding(TapGestureRecognizer.CommandProperty, "TappedGestureCommand");
                tapGestureRecognizer.CommandParameter = $"{labelName}";
                tapGestureRecognizer.NumberOfTapsRequired = 1;
                label.GestureRecognizers.Add(tapGestureRecognizer);

                label.SetValue(ToolTipProperties.TextProperty, "Tap to go to paragraph.");

                Border newBorder = new Border()
                {
                    Stroke = Colors.Blue,
                    Padding = new Thickness(10),
                    Margin = new Thickness(.5),
                    Content = label
                };
                contentVSL.Add(newBorder);
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in QueryInputViewModel.LoadXaml => ",
                ex.Message, "Ok");
        }
    }
    private async Task<FormattedString> CreateFormattedString(Paragraph paragraph)
    {
        // See: https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/label
        try
        {
            FormattedString formattedString = new FormattedString();
            await Task.Run(() =>
            {
                var paperId = paragraph.Id;
                var seqId = paragraph.SeqId;
                var pid = paragraph.Pid;
                var labelName = "_" + paperId.ToString("000") + "_" + seqId.ToString("000");

                Span tabSpan = new Span() { Style = (Style)App.Current.Resources["TabsSpan"] };
                Span pidSpan = new Span() { Style = (Style)App.Current.Resources["PID"], StyleId = labelName, Text = pid };
                Span spaceSpan = new Span() { Style = (Style)App.Current.Resources["RegularSpaceSpan"] };

                var runs = paragraph.Runs;
                Span newSpan = null;

                formattedString.Spans.Add(tabSpan);
                formattedString.Spans.Add(pidSpan);
                formattedString.Spans.Add(spaceSpan);
                foreach (var run in runs)
                {
                    newSpan = new Span() { Style = (Style)App.Current.Resources["RegularSpan"], Text = run.Text };
                    formattedString.Spans.Add(newSpan);
                }
            });
            return formattedString;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in QueryInputViewModel.LoadXaml => ",
                ex.Message, "Ok");
            return null;
        }
    }
    private async Task LoadXamlEx()
    {
        try
        {
            // See: https://learn.microsoft.com/en-us/dotnet/maui/xaml/runtime-load
            /*
            string navigationButtonXAML = "<Button Text=\"Navigate\" />";
            Button navigationButton = new Button().LoadFromXaml(navigationButtonXAML);
            stackLayout.Add(navigationButton); ...
            */

            var contentScrollView = contentPage.FindByName("queryResultScrollView") as ScrollView;
            var contentVSL = contentPage.FindByName("contentVerticalStackLayout") as VerticalStackLayout;

            var locations = QueryLocations;
            int hit = 0;
            foreach (var location in locations)
            {
                hit++;
                var id = location.Id;
                var pid = location.Pid;
                var arry = id.Split('.');
                var paperId = Int32.Parse(arry[0]);
                var seqId = Int32.Parse(arry[1]);
                var paragraph = await fileService.GetParagraphAsync(paperId, seqId);
                var paraStyle = paragraph.ParaStyle;
                var labelName = "_" + paperId.ToString("000") + "_" + seqId.ToString("000");

                string text = paragraph.Text;
                List<string> termList = new List<string>();
                foreach (var termOcc in location.TermOccurrences)
                {
                    var position = termOcc.TextPosition;
                    var length = termOcc.TextLength;
                    var term = paragraph.Text.Substring(position, length);
                    var termReplacement = "_{" + term + "}_";
                    if (termList.Contains(term))
                        continue;
                    var rgx = new Regex(term);
                    text = Regex.Replace(text, "\\b" + term + "\\b", termReplacement);
                    termList.Add(term);
                }
                var txtArray = text.Split('_');
                FormattedString fs = await CreateFormattedStringEx(paragraph, txtArray, hit);

                Label label = new Label { FormattedText = fs };
                TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.SetBinding(TapGestureRecognizer.CommandProperty, "TappedGestureCommand");
                tapGestureRecognizer.CommandParameter = $"{labelName}";
                tapGestureRecognizer.NumberOfTapsRequired = 1;
                label.GestureRecognizers.Add(tapGestureRecognizer);

                label.SetValue(ToolTipProperties.TextProperty, $"Tap to go to paragraph {pid}");

                Border newBorder = new Border()
                {
                    Stroke = Colors.Blue,
                    Padding = new Thickness(10),
                    Margin = new Thickness(.5),
                    Content = label
                };
                contentVSL.Add(newBorder);
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in QueryInputViewModel.LoadXaml => ",
                ex.Message, "Ok");
        }
    }
    private async Task<FormattedString> CreateFormattedStringEx(Paragraph paragraph, string[] textArray, int hit)
    {
        // See: https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/label
        try
        {
            FormattedString formattedString = new FormattedString();
            await Task.Run(() =>
            {
                var paperId = paragraph.Id;
                var seqId = paragraph.SeqId;
                var pid = paragraph.Pid;
                var labelName = "_" + paperId.ToString("000") + "_" + seqId.ToString("000");

                Span tabSpan = new Span() { Style = (Style)App.Current.Resources["TabsSpan"] };
                Span pidSpan = new Span() { Style = (Style)App.Current.Resources["PID"], StyleId = labelName, Text = pid };
                Span spaceSpan = new Span() { Style = (Style)App.Current.Resources["RegularSpaceSpan"] };
                Span hitsSpan = new Span() { Style = (Style)App.Current.Resources["HID"], StyleId = labelName, Text = $"[hit {hit}]" };

                var runs = paragraph.Runs;
                Span newSpan = null;

                formattedString.Spans.Add(hitsSpan);
                formattedString.Spans.Add(tabSpan);
                formattedString.Spans.Add(pidSpan);
                formattedString.Spans.Add(spaceSpan);

                foreach (var textRun in textArray)
                {
                    if (textRun.Contains('{'))
                        newSpan = new Span() { Style = (Style)App.Current.Resources["HighlightSpan"], Text = textRun };
                    else
                        newSpan = new Span() { Style = (Style)App.Current.Resources["RegularSpan"], Text = textRun };
                    formattedString.Spans.Add(newSpan);
                }

                //formattedString.Spans.Add(spaceSpan);
                //formattedString.Spans.Add(hitsSpan);

            });
            return formattedString;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in QueryInputViewModel.LoadXaml => ",
                ex.Message, "Ok");
            return null;
        }
    }

    #endregion
}
