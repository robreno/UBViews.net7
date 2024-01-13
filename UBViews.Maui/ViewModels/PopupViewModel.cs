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
using CommunityToolkit.Maui.Views;

using UBViews.Services;
using UBViews.Helpers;
using UBViews.Models.Notes;
using UBViews.Controls.Help;
using UBViews.Models.Ubml;

public partial class PopupViewModel : BaseViewModel
{
    #region Private Data Members
    public Popup popupPage;

    public VerticalStackLayout vslPopupContent;
    public string UniqueId = null;

    INoteService notesService;
    IFileService fileService;

    public ObservableCollection<NoteEntry> Notes { get; set; } = new();
    public ObservableCollection<NoteHit> Hits { get; set; } = new();
    public ObservableCollection<Paragraph> Paragraphs { get; set; } = new();
    public ObservableCollection<Border> Borders { get; set; } = new();

    public List<NoteEntry> CurrentNotes { get; set; } = new();

    readonly string _class = "PopupViewModel";
    #endregion

    #region Constructor
    public PopupViewModel()
    {
        this.fileService = new FileService();
        this.notesService = new NoteService(fileService);
    }
    #endregion

    #region Observable Properties
    [ObservableProperty]
    bool isInitialized = false;

    [ObservableProperty]
    string noteAuthor = null;

    [ObservableProperty]
    string noteSubject = null;

    [ObservableProperty]
    DateTime noteCreated;

    [ObservableProperty]
    DateTime noteEdited;

    [ObservableProperty]
    string noteText;

    [ObservableProperty]
    NoteLocationsDto noteLocations;

    [ObservableProperty]
    List<NoteEntry> noteList;

    [ObservableProperty]
    string locationId;

    [ObservableProperty]
    string pid;

    [ObservableProperty]
    int noteCount;
    #endregion

    #region Relay Commands

    [RelayCommand]
    async Task Tapped(string url)
    {
        string _method = "Tapped";
        try
        {
            string _url = url;
            await Launcher.OpenAsync(_url);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

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
    #endregion

    #region Public Methods
    public async Task LoadNotesAsync(string uniqueId)
    {
        string _method = " LoadNotesAsync";
        try
        {
            var ary = uniqueId.Split(".", StringSplitOptions.RemoveEmptyEntries);
            var paperId = Int32.Parse(ary[0]).ToString("0");
            var seqId = Int32.Parse(ary[1]).ToString("0");
            var id = paperId + "." + seqId;
            var _notes = await notesService.GetNotesAsync();
            foreach (var note in _notes)
            {
                Notes.Add(note);
            }

            CurrentNotes = _notes.Where(n => n.LocationId == id).ToList();

            NoteAuthor = CurrentNotes[0].Author;
            NoteCreated = CurrentNotes[0].DateCreated;
            NoteEdited = CurrentNotes[0].DateEdited;
            NoteSubject = CurrentNotes[0].Subject;
            LocationId = CurrentNotes[0].LocationId;
            Pid = CurrentNotes[0].Pid;
            NoteText = CurrentNotes[0].Text;

            IsInitialized = true;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in MainViewModel.NavigateTo => ",
                ex.Message, "Cancel");
        }
    }
    public async Task CreateNoteContentAsync()
    {
        string _method = "CreateNoteContentAsync";
        try
        {
            //var contentScrollView = popupPage.FindByName("mainScrollView") as ScrollView;
            // vslPopupContent
            //var contentVSL = contentPage.FindByName("mainSCVSL") as VerticalStackLayout;
            //var notes = CurrentNotes;
            //int hit = 0;
            await LoadXamlAsync(CurrentNotes, true);

        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }
    public async Task ParagraphSelectedAsync(CheckBox checkbox)
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
    public async Task LoadXamlAsync(List<NoteEntry> dtos, bool clear = false)
    {
        string _method = "LoadXamlAsync";
        try
        {
            if (vslPopupContent == null)
            {
                return;
            }

            var contentScrollView = popupPage.FindByName("contentScrollView") as ScrollView;
            var contentVSL = popupPage.FindByName("mainSCVSL") as VerticalStackLayout;
            var notes = CurrentNotes;
            int hit = 0;

            if (clear)
            {
                contentScrollView.Content = null;
                contentVSL = new VerticalStackLayout()
                {
                    HorizontalOptions = LayoutOptions.Center
                };
            }

            foreach (var note in notes)
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
                var uniqueId = paperId.ToString("0") + "." + seqId.ToString("0");
                var paragraph = await fileService.GetParagraphAsync(paperId, seqId);

                paragraph.Notes.Add(note);
                Paragraphs.Add(paragraph);

                //var entries = note.NoteEntries;

                NoteHit noteHit = new NoteHit
                {
                    Id = paperId + "." + seqId,
                    Subject = subject,
                    Hit = hit,
                    PaperId = paperId,
                    SequenceId = seqId,
                    LocationId = paperId + "." + seqId,
                    Pid = pid,
                    Selected = false,
                    IsDirty = false
                };
                Hits.Add(noteHit);

                // Create Span List
                var spans = await GetSpansAsync(note, paragraph);
                // Create FormattedString
                FormattedString fs = await CreateFormattedStringAsync(noteHit, spans, hit);
                // Create Label
                Label label = await CreateLabelAsync(fs, labelName, paperId, seqId, pid);
                // Create Border
                Border newBorder = await CreateBorderAsync(paperId, seqId, label);

                Borders.Add(newBorder);
                contentVSL.Add(newBorder);
                contentScrollView.Content = contentVSL;
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }
    #endregion

    #region Private Methods
    private async Task<List<Span>> GetSpansAsync(NoteEntry note, Paragraph paragraph)
    {
        string _method = "GetSpansListAsync";
        try
        {
            string paragraphText = paragraph.Text;
            List<Span> spans = new();
            var runs = note.NoteRuns;
            Span newSpan = null;
            foreach (var run in runs)
            {
                newSpan = new();
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

                }
                if (runFontFamily != null)
                {

                }
                if (runTextColor != null)
                {
                    newSpan.TextColor = Color.Parse(runTextColor);
                }
                if (runTextTransform != null)
                {

                }
                spans.Add(newSpan);
            }
            return spans;
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
            var locationId = noteHit.LocationId;
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

            Label authorLabel = new Label { Text = $"Author: {NoteAuthor}", Style = style };
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

            //var behavior = new EventToCommandBehavior
            //{
            //    EventName = "CheckedChanged",
            //    Command = ParagraphSelectedCommand,
            //    CommandParameter = checkBox
            //};
            //checkBox.Behaviors.Add(behavior);
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
        string _method = "CreateNoteIconAsyn";
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
