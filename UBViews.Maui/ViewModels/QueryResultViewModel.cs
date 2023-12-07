namespace UBViews.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Input;

using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics.Text;

using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Platform;

using UBViews.Models;
using UBViews.Models.Ubml;
using UBViews.Models.Query;
using UBViews.Services;
using UBViews.Views;

[QueryProperty(nameof(QueryLocations), nameof(QueryLocations))]
public partial class QueryResultViewModel : BaseViewModel
{
    public ContentPage contentPage;

    public ObservableCollection<QueryLocationDto> QueryLocationsDto { get; } = new();
    public ObservableCollection<QueryHit> Hits { get; set; } = new();
    public ObservableCollection<Paragraph> Paragraphs { get; set; } = new();
    public ObservableCollection<Border> Borders { get; set; } = new();

    IFileService fileService;
    IEmailService emailService;
    IQueryProcessingService queryProcessingService;
    IAppSettingsService appSettingsService;

    readonly string _class = "QueryResultViewModel";

    public QueryResultViewModel(IFileService fileService, IQueryProcessingService queryProcessingService, IEmailService emailService, IAppSettingsService appSettingsService)
    {
        this.fileService = fileService;
        this.emailService = emailService;
        this.queryProcessingService = queryProcessingService;
        this.appSettingsService = appSettingsService;
    }

    [ObservableProperty]
    string pageTitle;

    [ObservableProperty]
    bool isRefreshing;

    [ObservableProperty]
    QueryResultLocationsDto queryLocations;

    [ObservableProperty]
    string queryString;

    [ObservableProperty]
    string queryInputString;

    [ObservableProperty]
    string previousQueryInputString;

    [ObservableProperty]
    string queryExpression;

    [ObservableProperty]
    List<string> termList;

    [ObservableProperty]
    bool queryResultExists;

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
        string _method = "QueryResultAppearing";
        try
        {
            if (dto == null)
            {
                return;
            }

            if (dto.DefaultQueryString != null)
            {
                QueryInputString = PreviousQueryInputString = dto.DefaultQueryString;
            }
            else
            {
                QueryInputString = PreviousQueryInputString = dto.QueryString;
            }

            QueryString = QueryInputString;

            QueryString = dto.QueryString;
            QueryHits = dto.Hits;
            MaxQueryResults = await appSettingsService.Get("max_query_results", 50);

            string titleMessage = $"Query Result {queryHits} hits ...";
            Title = titleMessage;

            var qlr = dto.QueryLocations.Take(MaxQueryResults).ToList();
            foreach (var location in qlr)
            {
                QueryLocationsDto.Add(location);
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    [RelayCommand]
    async Task QueryResultLoaded()
    {
        string _method = "QueryResultLoaded";
        try
        {
            IsBusy = true;

            var locations = QueryLocationsDto;
            if (locations == null)
            {
                return;
            }
            else
            {
                List<QueryLocationDto> dtos = new List<QueryLocationDto>();
                foreach (var location in QueryLocationsDto)
                {
                    dtos.Add(location);
                }
                await LoadXaml(dtos);
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
    async Task TappedGesture(string lableName)
    {
        string _method = "TappedGesture";
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
    async Task SwipeLeftGesture(string value)
    {
        string _method = "SwipeLeftGesture";
        try
        {
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    [RelayCommand]
    async Task SelectedCheckboxChanged(bool value)
    {
        string _method = "SelectedCheckboxChanged";
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
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    [RelayCommand]
    async Task ParagraphSelected(CheckBox checkbox)
    {
        string _method = "ParagraphSelected";
        try
        {
            string id = checkbox.ClassId;
            bool isSelected = checkbox.IsChecked;
            // Hits
            QueryHit hit = Hits.Where(h => h.Id == id).FirstOrDefault();
            hit.Selected = isSelected;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    [RelayCommand]
    async Task ShareSelected(string value)
    {
        string _method = "ShareSelected";
        try
        {
            var selectedHits = Hits.Where(p => p.Selected == true).ToList();
            List<Paragraph> paragraphs = new();
            foreach (var hit in selectedHits)
            {
                var paragraph = hit.Paragraph;
                paragraphs.Add(paragraph);
            }
#if WINDOWS
            await emailService.EmailParagraphsAsync(paragraphs, IEmailService.EmailType.PlainText, IEmailService.SendMode.AutoSend);
#elif ANDROID
            await emailService.ShareParagraphsAsync(paragraphs);
#endif
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    [RelayCommand]
    async Task SubmitQuery(string queryString)
    {
        string _method = "SubmitQuery";
        try
        {
            IsBusy = true;
            string message = string.Empty;
            bool parsingSuccessful = false;
            bool runPreCheckSilent = await appSettingsService.Get("run_precheck_silent", true);

            QueryInputString = queryString.Trim();
            (bool result, message) = await queryProcessingService.PreCheckQueryAsync(QueryInputString,
                                                                                     runPreCheckSilent);
            if (message.Contains("="))
            {
                return;
            }

            if (QueryInputString == PreviousQueryInputString)
            {
                message = $"The query \"{QueryInputString}\" was same. Try another query?";
                await App.Current.MainPage.DisplayAlert("Query Results", message, "Cancel");
                return;
            }
            else
            {
                PreviousQueryInputString = QueryInputString;
            }

            (parsingSuccessful, message) = await queryProcessingService.ParseQueryAsync(QueryInputString);
            if (parsingSuccessful)
            {
                QueryInputString = await queryProcessingService.GetQueryInputStringAsync();
                QueryExpression = await queryProcessingService.GetQueryExpressionAsync();
                TermList = await queryProcessingService.GetTermListAsync();

                (bool isSuccess, QueryResultExists, QueryLocations) = await queryProcessingService.RunQueryAsync(QueryInputString);
                if (isSuccess)
                {
                    QueryLocationsDto.Clear();
                    foreach (var location in QueryLocations.QueryLocations)
                    {
                        QueryLocationsDto.Add(location);
                    }

                    if (QueryResultExists)
                    {
                        // Query result from history successfully
                        // Navigate to results page
                    }
                    else
                    {
                        // New query run successfully
                        // Navigate to results page
                    }
                    //await NavigateTo("QueryResults");
                    await LoadXaml(QueryLocationsDto.ToList(), true);
                }
                else
                {
                    // Handle unsuccessful query result returned
                    // Return to existing query page and try again
                    //await Shell.Current.GoToAsync("..");
                    message = $"Query unsuccessful for unknown reason.";
                    await App.Current.MainPage.DisplayAlert($"Query in {_method}.{_class} => ", message, "Ok");
                }
            }
            else // Parsing failure
            {
                message = "Unknown Erro, try again?";
                QueryInputString = await App.Current.MainPage.DisplayPromptAsync("Query Parsing Error",
                    message, "Ok", "Cancel", "Retry Query here ..", -1, null, "");
            }
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

    [RelayCommand]
    async Task NavigateTo(string target)
    {
        try
        {
            IsBusy = true;

            string targetName = string.Empty;
            if (target == "QueryResults")
            {
                targetName = nameof(QueryResultPage);
                QueryResultLocationsDto dto = QueryLocations;
                await Shell.Current.GoToAsync(targetName, new Dictionary<string, object>()
                {
                    {"QueryLocations", dto }
                });
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in MainViewModel.NavigateTo => ",
                ex.Message, "Cancel");
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
        string _method = "GoToDetails";
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
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    #region Helper Methods
    private async Task LoadXaml(List<QueryLocationDto> dtos, bool clear = false)
    {
        string _method = "LoadXaml";
        try
        {
            var contentScrollView = contentPage.FindByName("contentScrollView") as ScrollView;
            var contentVSL = contentPage.FindByName("contentVerticalStackLayout") as VerticalStackLayout;
            var locations = dtos;
            int hit = 0;

            if (clear)
            {
                contentScrollView.Content = null;
                contentVSL = new VerticalStackLayout()
                {
                    HorizontalOptions = LayoutOptions.Center
                };
            }

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

                Paragraphs.Add(paragraph);

                QueryHit queryHit = new QueryHit
                {
                    Id = paperId + "." + seqId,
                    Query = QueryString,
                    Hit = hit,
                    PaperId = paperId,
                    SequenceId = seqId,
                    Pid = pid,
                    Selected = false,
                    Paragraph = paragraph,
                };
                Hits.Add(queryHit);

                // Create Span List
                var spansList = await CreateSpansList(location, paragraph);
                // Create FormattedString
                FormattedString fs = await CreateFormattedString(paragraph, spansList, hit);
                // Create Label
                Label label = await CreateLabel(fs, labelName, paperId, seqId, pid);
                // Create Border
                Border newBorder = await CreateBorder(paperId, seqId, label);

                Borders.Add(newBorder);
                contentVSL.Add(newBorder);
                contentScrollView.Content = contentVSL;
            }
            string titleMessage = $"Query Result {hit} hits ...";
            Title = titleMessage;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }
    private async Task<FormattedString> CreateFormattedString(Paragraph paragraph, List<Span> spansList, int hit)
    {
        string _method = "CreateFormattedString";
        try
        {
            FormattedString formattedString = new FormattedString();
            await Task.Run(() =>
            {
                var paperId = paragraph.PaperId;
                var seqId = paragraph.SeqId;
                var pid = paragraph.Pid;
                var labelName = "_" + paperId.ToString("000") + "_" + seqId.ToString("000");

                Span tabSpan = new Span() { Style = (Style)App.Current.Resources["TabsSpan"] };
                Span pidSpan = new Span() { Style = (Style)App.Current.Resources["PID"], StyleId = labelName, Text = pid };
                Span spaceSpan = new Span() { Style = (Style)App.Current.Resources["RegularSpaceSpan"] };
                Span hitsSpan = new Span() { Style = (Style)App.Current.Resources["HID"], StyleId = labelName, Text = $"[hit {hit}]" };

                formattedString.Spans.Add(hitsSpan);
                formattedString.Spans.Add(tabSpan);
                formattedString.Spans.Add(pidSpan);
                formattedString.Spans.Add(spaceSpan);

                foreach (var span in spansList)
                {
                    formattedString.Spans.Add(span);
                }
            });
            return formattedString;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task<Border> CreateBorder(int paperId, int seqId, Label label)
    {
        string _method = "CreateBorder";
        try
        {
            VerticalStackLayout vsl = new VerticalStackLayout();

            CheckBox checkBox = await CreateCheckBox(paperId, seqId);

            vsl.Add(label);
            vsl.Add(checkBox);

            Border newBorder = new Border()
            {
                Style = (Style)App.Current.Resources["QueryResultBorderStyle"],
                Content = vsl
            };
            return newBorder;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task<CheckBox> CreateCheckBox(int paperId, int seqId)
    {
        string _method = "CreateCheckBox";
        try
        {
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
            return checkBox;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task<Label> CreateLabel(FormattedString fs, string labelName, int paperId, int seqId, string pid)
    {
        string _method = "CreateLabel";
        try
        {
            Label label = new Label { FormattedText = fs };
            //label.Style = (Style)App.Current.Resources["HighlightSpan"];
            label.ClassId = paperId + "." + seqId;
            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.SetBinding(TapGestureRecognizer.CommandProperty, "TappedGestureCommand");
            tapGestureRecognizer.CommandParameter = $"{labelName}";
            tapGestureRecognizer.NumberOfTapsRequired = 1;
            label.GestureRecognizers.Add(tapGestureRecognizer);
            label.SetValue(ToolTipProperties.TextProperty, $"Tap to go to paragraph {pid} ...");
            return label;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task<List<Span>> CreateSpansList(QueryLocationDto location, Paragraph paragraph)
    {
        string _method = "GetSpansList";
        try
        {
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
            var spanArray = text.Split('_');

            var txt = string.Empty;
            Span newSpan = null;
            List<Span> spansList = new();
            foreach (var item in spanArray)
            {
                txt = item;
                if (item.Contains('{'))
                {
                    txt = txt.Replace('{', ' ');
                    txt = txt.Replace('}', ' ');
                    txt = txt.Trim();
                    newSpan = new Span() { Style = (Style)App.Current.Resources["HighlightSpan"], Text = txt };
                    spansList.Add(newSpan);
                }
                else
                {
                    newSpan = new Span() { Style = (Style)App.Current.Resources["RegularSpan"], Text = item };
                    spansList.Add(newSpan);
                }
            }
            return spansList;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    #endregion
    private async Task SendToast(string message)
    {
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
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
            return;
        }
    }
}
