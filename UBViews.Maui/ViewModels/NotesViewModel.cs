namespace UBViews.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Collections.ObjectModel;

using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics.Text;
using Microsoft.Maui.Controls.Platform;

using UBViews.Models;
using UBViews.Models.Notes;
using UBViews.Models.Ubml;
using UBViews.Models.Query;
using UBViews.Services;
using UBViews.Views;


//[QueryProperty(nameof(NoteLocations), nameof(NoteLocations))]
public partial class NotesViewModel : BaseViewModel
{
    #region Private Data Members
    public ContentPage contentPage;

    public ObservableCollection<UniqueId> Uids { get; set; } = new();
    public ObservableCollection<Editor> Editors { get; set; } = new();
    public ObservableCollection<Button> Buttons { get; set; } = new();
    public ObservableCollection<NoteEntry> Notes { get; } = new();
    public ObservableCollection<Paragraph> Paragraphs { get; set; } = new();
    public ObservableCollection<NoteHit> Hits { get; set; } = new();

    IFileService fileService;
    IEmailService emailService;
    INoteService notesService;

    readonly string _class = "NotesViewModel";
    #endregion

    #region Constructor
    public NotesViewModel(IFileService fileService, INoteService notesService, IEmailService emailService)
    {
        this.fileService = fileService;
        this.notesService = notesService;
        this.emailService = emailService;
    }
    #endregion

    #region Observable Properties
    [ObservableProperty]
    bool isRefreshing;

    [ObservableProperty]
    bool isInitialized;

    [ObservableProperty]
    string pageTitle;

    [ObservableProperty]
    string noteAuthor = null;

    [ObservableProperty]
    string noteSubject = null;

    [ObservableProperty]
    string defaultNoteSpanStyle = null;

    [ObservableProperty]
    DateTime noteCreated;

    [ObservableProperty]
    DateTime noteEdited;

    [ObservableProperty]
    NoteLocationsDto noteLocations;

    [ObservableProperty]
    List<NoteEntry> noteList;

    [ObservableProperty]
    int noteCount;

    [ObservableProperty]
    bool hideUnselected;
    #endregion

    #region Relay Commands
    [RelayCommand]
    async Task NotesPageAppearing()
    {
        string _method = "NotesPageAppearing";
        try
        {
            if (contentPage == null)
            {
                return;
            }

            var locations = await notesService.GetNotesAsync();
            foreach (var location in locations)
            {
                var paperId = location.PaperId;
                var seqId = location.SequenceId;
                var locationId = location.LocationId;
                var paragraph = Paragraphs.Where(p => p.PaperSeqId == locationId).FirstOrDefault();
                if (paragraph == null)
                {
                    paragraph = await fileService.GetParagraphAsync(paperId, seqId);
                    Paragraphs.Add(paragraph);
                }
                paragraph.Notes.Add(location);
                Notes.Add(location);
            }

            NoteCount = Notes.Count();

            string titleMessage = $"UBViews NoteEntries";
            Title = titleMessage;

            var _notes = Notes.ToList();
            await CreateXamlAsync(_notes);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised {_method}.{_class} => ", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task TappedGesture(string lableName)
    {
        string _method = "TappedGesture";
        try
        {
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

            string msg = $"Navigating to {pid}";
            //await App.Current.MainPage.DisplayAlert("Navigate to =>", message, "Cancel");

            // Don't sync with UBViews, this is test code only
            if (paperDto.Id == 0)
            {
                await GoToDetails(paperDto);
            }
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
            NoteHit hit = Hits.Where(h => h.Id == id).FirstOrDefault();
            hit.Selected = isSelected;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    [RelayCommand]
    async Task ShareSelected()
    {
        string _method = "ShareSelected";
        // TODO: unfinished code
        try
        {
            var selectedHits = Hits.Where(p => p.Selected == true).ToList();
            //var _paragraph  = notesService.GetParagraphAsync()
            List<Paragraph> paragraphs = new();
            foreach (var hit in selectedHits)
            {
                // Save Note Here or throw
                var id = hit.Id;
                var paperId = hit.PaperId;
                var seqId = hit.SequenceId;
                // not updates yet ...
                var paragraph = await notesService.GetParagraphAsync(paperId, seqId);
                paragraphs.Add(paragraph);
            }
#if WINDOWS
            await emailService.EmailParagraphsAsync(paragraphs, IEmailService.EmailType.PlainText, IEmailService.SendMode.AutoSend);
#elif ANDROID
            await emailService.ShareParagraphsAsync(paragraphs);
#endif
        }
        catch (FormatException ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
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
    #endregion

    #region Helper Methods
    private async Task CreateXamlAsync(List<NoteEntry> notes, bool plainText = true, bool clear = false)
    {
        string _method = "CreateXamlAsync";
        try
        {
            var _contentScrollView = contentPage.FindByName("contentScrollView") as ScrollView;
            var _contentVSL = contentPage.FindByName("contentVerticalStackLayout") as VerticalStackLayout;
            var _notes = notes;
            int hit = 0;

            if (clear) 
            {
                _contentScrollView.Content = null;
                _contentVSL = new VerticalStackLayout()
                {
                    HorizontalOptions = LayoutOptions.Center
                };
            }

            foreach (var note in _notes)
            {
                hit++;
                var id = note.Id;
                var type = note.Type;
                var paperId = note.PaperId;
                var seqId = note.SequenceId;
                var pid = note.Pid;
                var created = NoteCreated = note.DateCreated;
                var edited = NoteEdited = note.DateEdited;
                var author = NoteAuthor = note.Author;
                var subject = NoteSubject = note.Subject;

                var labelName = "_" + paperId.ToString("000") + "_" + seqId.ToString("000");
                var locationId = paperId.ToString("0") + "." + seqId.ToString("0");
                var paragraph = Paragraphs.Where(p => p.PaperSeqId == locationId).FirstOrDefault();

                NoteHit noteHit = new NoteHit
                {
                    Id = paperId + "." + seqId,
                    Subject = subject,
                    Hit = hit,
                    PaperId = paperId,
                    SequenceId = seqId,
                    Pid = pid,
                    Selected = false,
                    IsDirty = false
                };
                Hits.Add(noteHit);

                // Create Span List
                var spansList = await GetSpansAsync(note, paragraph, plainText);
                // Create FormattedString
                FormattedString fs = await CreateFormattedStringAsync(noteHit, spansList, hit);
                // Create Label
                Label label = await CreateLabelAsync(fs, labelName, paperId, seqId, pid);
                // Create Border
                Border newBorder = await CreateBorderAsync(paperId, seqId, label);

                _contentVSL.Add(newBorder);
            }
            _contentScrollView.Content = _contentVSL;

            string titleMessage = string.Empty;
            titleMessage = $"Notes ({hit} hits)";
            Title = titleMessage;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }
    private async Task<List<Span>> GetSpansAsync(NoteEntry note, Paragraph paragraph, bool plainText)
    {
        string _method = "GetSpansAsync";
        try
        {
            string paragraphText = paragraph.Text;
            List<Span> spans = new();

            Span newSpan = null;
            NoteRun noteRun = null;
            if (plainText)
            {
                noteRun = new NoteRun()
                {
                    Text = note.Text,
                    Style = DefaultNoteSpanStyle,
                    //TextSize = null,
                    //FontFamily = null,
                    //TextColor = null,
                    //TextTransform = null
                };
                newSpan = await CreateNewSpanAsync(noteRun);
                spans.Add(newSpan);
            }
            else
            {
                // Make RichText Runs 
                var noteRuns = note.NoteRuns;
                foreach (var run in noteRuns)
                {
                    newSpan = new Span();

                    var runText = run.Text;
                    var runStyle = run.Style;
                    var runFontSize = run.TextSize;
                    var runFontFamily = run.FontFamily;
                    var runTextColor = run.TextColor;
                    var runTextTransform = run.TextTransform;

                    noteRun = new NoteRun()
                    {
                        Text = note.Text,
                        Style = run.Style,
                        TextSize = run.TextSize,
                        FontFamily = run.FontFamily,
                        TextColor = run.TextColor,
                        TextTransform = run.TextTransform
                    };
                    newSpan = await CreateNewSpanAsync(noteRun);
                    spans.Add(newSpan);
                }
            }
            return spans;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class} . {_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task<Span> CreateNewSpanAsync(NoteRun run)
    {
        string _method = "CreateNewSpan";
        try
        {
            Span newSpan = new Span();

            var runText = run.Text;
            var runStyle = run.Style;
            var runFontSize = run.TextSize;
            var runFontFamily = run.FontFamily;
            var runTextColor = run.TextColor;
            var runTextTransform = run.TextTransform;

            newSpan.Text = runText;
            newSpan.Style = (Style)App.Current.Resources[runStyle];
            if (runFontSize != null)
            {
                var _size = Double.Parse(runFontSize);
            }
            if (runFontFamily != null)
            {
                newSpan.FontFamily = runFontFamily;
            }
            if (runTextColor != null)
            {
                var _color = Color.Parse(runTextColor);
            }
            if (runTextTransform != null)
            {
                TextTransform _transform;
                if (runTextTransform.Equals("None"))
                {
                    _transform = TextTransform.None;
                }
                else if (runTextTransform.Equals("Lowercase"))
                {
                    _transform = TextTransform.Lowercase;
                }
                else if (runTextTransform.Equals("Uppercase"))
                {
                    _transform = TextTransform.Uppercase;
                }
                else
                {
                    _transform = TextTransform.Default;
                }
                newSpan.TextTransform = _transform;
            }
            return newSpan;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class} . {_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task<FormattedString> CreateFormattedStringAsync(NoteHit noteHit, List<Span> spansList, int hit)
    {
        string _method = "CreateFormattedStringAsync";
        try
        {
            FormattedString formattedString = new FormattedString();

            var paperId = noteHit.PaperId;
            var seqId = noteHit.SequenceId;
            var pid = noteHit.Pid;
            var labelName = "_" + paperId.ToString("000") + "_" + seqId.ToString("000");

            Span tabSpan = new Span() { Style = (Style)App.Current.Resources["TabsSpan"] };
            Span pidSpan = new Span() { Style = (Style)App.Current.Resources["PID"], StyleId = labelName, Text = pid };
            Span spaceSpan = new Span() { Style = (Style)App.Current.Resources["RegularSpaceSpan"] };
            Span hitsSpan = new Span() { Style = (Style)App.Current.Resources["HID"], StyleId = labelName, Text = $"[note {hit}]" };

            formattedString.Spans.Add(hitsSpan);
            formattedString.Spans.Add(tabSpan);
            formattedString.Spans.Add(pidSpan);
            formattedString.Spans.Add(spaceSpan);

            foreach (var span in spansList)
            {
                formattedString.Spans.Add(span);
            }

            return formattedString;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task<Label> CreateLabelAsync(FormattedString fs, string labelName, int paperId, int seqId, string pid)
    {
        string _method = "CreateLabelAsync";
        try
        {
            Label label = new Label { FormattedText = fs };
            label.Style = (Style)App.Current.Resources["RegularParagraph"];
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
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class} . {_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task<Border> CreateBorderAsync(int paperId, int seqId, Label label)
    {
        string _method = "CreateBorderAsync";
        try
        {
            VerticalStackLayout vsl = new VerticalStackLayout();

            CheckBox checkBox = await CreateCheckBoxAsync(paperId, seqId);

            Style style = (Style)App.Current.Resources["RegularParagraph"];

            //Label authorLabel  = new Label { Text = $"Author: {NoteAuthor}", Style = style };
            //Label subjectLabel = new Label { Text = $"Subject: {NoteSubject}", Style = style };

            //vsl.Add(authorLabel);
            //vsl.Add(subjectLabel);

            vsl.Add(label);
            vsl.Add(checkBox);

            Border newBorder = new Border()
            {
                Style = (Style)App.Current.Resources["NoteBorderStyle"],
                Content = vsl
            };
            return newBorder;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}  .  {_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task<CheckBox> CreateCheckBoxAsync(int paperId, int seqId)
    {
        string _method = "CreateCheckBoxAsync";
        try
        {
            CheckBox checkBox = new CheckBox();
            checkBox.ClassId = paperId + "." + seqId;
            checkBox.HorizontalOptions = LayoutOptions.End;

            var binding = new Binding();
            binding.Source = nameof(NotesViewModel);
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
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class} . {_method} => ", ex.Message, "Ok");
            return null;
        }
    } 
    private async Task<Border> CreateNoteIconAsync(NoteEntry note)
    {
        string _method = "CreateNoteIconAsync";
        try
        {
            var pid = note.Pid;
            var locationId = note.LocationId;
            var arry = locationId.Split('.');
            var labelName = "_" + Int32.Parse(arry[0]).ToString("000")
                            + "_" + Int32.Parse(arry[1]).ToString("000");

            Image image = new Image()
            {
                Source = "quick_note.png",
                Aspect = Aspect.AspectFit
            };

            Border border = new Border()
            {
                HeightRequest = 15,
                WidthRequest = 15,
                HorizontalOptions = LayoutOptions.Start,

            };
            border.SetValue(ToolTipProperties.TextProperty, $"Tap to open note(s) for {pid} ...");
            border.Content = image;

            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.SetBinding(TapGestureRecognizer.CommandProperty, "OpenPopupNoteCommand");
            tapGestureRecognizer.CommandParameter = $"{labelName}";
            tapGestureRecognizer.NumberOfTapsRequired = 1;
            border.GestureRecognizers.Add(tapGestureRecognizer);

            return border;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task SendToastAsync(string message)
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
    #endregion
}
