namespace UBViews.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Input;

using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Platform;

using UBViews.Models;
using UBViews.Models.Ubml;
using UBViews.Models.Query;
using UBViews.Services;


[QueryProperty(nameof(LocationsDto), "LocationsDto")]
public partial class QueryResultViewModel : BaseViewModel
{
    public ContentPage contentPage;
    public ObservableCollection<QueryLocationDto> QueryLocations { get; } = new();
    public ObservableCollection<QueryHit> Paragraphs { get; set; } = new();

    Dictionary<string, Span> _spans = new Dictionary<string, Span>();

    IFileService fileService;
    IAppSettingsService appSettingsService;

    public QueryResultViewModel(IFileService fileService, IAppSettingsService appSettingsService)
    {
        this.fileService = fileService;
        this.appSettingsService = appSettingsService;
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
    int maxQueryResults;

    [ObservableProperty]
    bool showReferencePids;

    [ObservableProperty]
    bool isScrollToLabel;

    [ObservableProperty]
    string scrollToLabelName;

    [ObservableProperty]
    bool hideUnselected;

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
            MaxQueryResults = await appSettingsService.Get("max_query_results", 50);

            string titleMessage = $"Query Result {queryHits} hits ...";
            Title = titleMessage;

            var qlr = dto.QueryLocations.Take(MaxQueryResults).ToList();
            foreach (var location in qlr)
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
    async Task SwipeLeftGesture(string value)
    {
        try
        {
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task SelectedCheckboxChanged(bool value)
    {
        try
        {
            var contentVSL = contentPage.FindByName("contentVerticalStackLayout") as VerticalStackLayout;
            var checkedLabel = contentPage.FindByName("hideUncheckedLabel") as Label;
            if (contentVSL == null)
            {
                return;
            }
            var children = contentVSL.Children;
            foreach (var child in children)
            {
                Border border = (Border)child;
                var content = border.Content;
                var visualTree = content.GetVisualTreeDescendants();
                var vsl = (VerticalStackLayout)visualTree[0];
                var lbl = (Label)visualTree[1];
                var chk = (CheckBox)visualTree[2];
                var isChecked = chk.IsChecked;
                var borderIsVisible = border.IsVisible;
                if (!isChecked && borderIsVisible)
                {
                    border.IsVisible = false;
                    checkedLabel.Text = "Show Unchecked";
                }
                else if (!isChecked && !borderIsVisible)
                {
                    border.IsVisible = true;
                    checkedLabel.Text = "Hide Unchecked";
                }
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task ParagraphSelected(CheckBox checkbox)
    {
        try
        {
            string id = checkbox.ClassId;
            bool isSelected = checkbox.IsChecked;
            QueryHit hit = Paragraphs.Where(h => h.Id == id).FirstOrDefault();
            hit.Selected = isSelected;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task ShareSelected(string value)
    {
        try
        {
            foreach (var paragraph in Paragraphs)
            {

            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return;
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

                label.SetValue(ToolTipProperties.TextProperty, $"Tap to go to paragraph {pid} ...   ");

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

                QueryHit queryHit = new QueryHit
                {
                    Id = paperId + "." + seqId,
                    Query = QueryString,
                    Hit = hit,
                    PaperId = paperId,
                    SequenceId = seqId,
                    Pid = pid,
                    Selected = false,
                    ParagraphHit = paragraph
                };
                Paragraphs.Add(queryHit);

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
                label.ClassId = pid + "_" + seqId;
                TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.SetBinding(TapGestureRecognizer.CommandProperty, "TappedGestureCommand");
                tapGestureRecognizer.CommandParameter = $"{labelName}";
                tapGestureRecognizer.NumberOfTapsRequired = 1;
                label.GestureRecognizers.Add(tapGestureRecognizer);

                label.SetValue(ToolTipProperties.TextProperty, $"Tap to go to paragraph {pid}");

                VerticalStackLayout vsl = new VerticalStackLayout();

                CheckBox checkBox = new CheckBox();
                checkBox.ClassId = paperId + "." + seqId;
                checkBox.HorizontalOptions = LayoutOptions.End;

                var binding = new Binding();
                binding.Source = nameof(QueryResultViewModel);
                binding.Path = "IsChecked";

                var behavior = new EventToCommandBehavior
                {
                    EventName = "CheckedChanged",
                    Command = ParagraphSelectedCommand,
                    CommandParameter = checkBox
                };
                checkBox.Behaviors.Add(behavior);

                vsl.Add(label);
                vsl.Add(checkBox);

                Border newBorder = new Border()
                {
                    Stroke = Colors.Blue,
                    Padding = new Thickness(10),
                    Margin = new Thickness(.5),
                    Content = vsl
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
    private async Task<string> CreateEmailText(string pretext, string postText, string subject, List<string> recipients, EmailBodyFormat bodyFormat = EmailBodyFormat.PlainText)
    {
        try
        {
            string emailText = string.Empty;
            return emailText;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }
    #endregion
}
