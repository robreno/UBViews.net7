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

using Microsoft.Maui.Graphics.Text;
using Microsoft.Maui.Controls.Platform;

using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls.Shapes;
//using CommunityToolkit.Maui.Views;
//using CommunityToolkit.Common;

using UBViews.Models;
using UBViews.Models.Notes;
using UBViews.Models.Ubml;
using UBViews.Models.Query;
using UBViews.Services;
using UBViews.Views;

using UBViews.Controls;
using UBViews.Controls.Help;
using UBViews.Converters;

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

    //bool _authorValid = false;
    //bool _subjectValid = false;
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
    string noteStyle = null;

    [ObservableProperty]
    string defaultNoteSpanStyle = null;

    [ObservableProperty]
    DateTime noteCreated;

    [ObservableProperty]
    DateTime noteEdited;

    [ObservableProperty]
    List<NoteEntry> noteList;

    [ObservableProperty]
    int noteCount;

    [ObservableProperty]
    string id;

    [ObservableProperty]
    string currentNoteText;

    [ObservableProperty]
    string previousNoteText;

    [ObservableProperty]
    string loadedText;

    [ObservableProperty]
    string currentNoteName;

    [ObservableProperty]
    string previousNoteName;

    [ObservableProperty]
    string currentCheckBoxTooltip;

    [ObservableProperty]
    string currentNoteId;

    [ObservableProperty]
    string previousNoteId;

    [ObservableProperty]
    bool hideUnselected;

    [ObservableProperty]
    string currentAuthorSubject;

    [ObservableProperty]
    string currentCreateEdit;

    [ObservableProperty]
    string currentNoteIds;

    [ObservableProperty]
    string currentTooltip = "Default tooltip ...";

    [ObservableProperty]
    Dictionary<string, string> tooltips = new();

    [ObservableProperty]
    string[] arrayTooltips;

    [ObservableProperty]
    string newNoteAuthor = string.Empty;

    [ObservableProperty]
    string newNoteSubject = string.Empty;

    [ObservableProperty]
    string newNoteType = string.Empty;

    [ObservableProperty]
    int newNotePaperId = 0;

    [ObservableProperty]
    string newNotePaperIdText = "0";

    [ObservableProperty]
    int newNoteSectionId = 0;

    [ObservableProperty]
    string newNoteSectionIdText = "0";

    [ObservableProperty]
    int newNoteParagraphId = 0;

    [ObservableProperty]
    string newNoteParagraphIdText = "0";

    [ObservableProperty]
    double paperIdStepper = 0;

    [ObservableProperty]
    double sectionIdStepper = 0;

    [ObservableProperty]
    int newNoteSequenceId = 1;

    [ObservableProperty]
    double paragraphIdStepper = 0;

    [ObservableProperty]
    string newNoteLocationId = string.Empty;

    [ObservableProperty]
    bool authorIsValid = false;

    [ObservableProperty]
    bool subjectIsValid = false;

    [ObservableProperty]
    bool typeIsValid = false;

    [ObservableProperty]
    bool notesDirty = false;

    [ObservableProperty]
    bool noteSaved = false;

    [ObservableProperty]
    bool editingNewNote = false;

    [ObservableProperty]
    Color defaultColorForMainVSL = Color.Parse("White");

    [ObservableProperty]
    Color defaultColorForSelectionHSL = Color.Parse("White");

    [ObservableProperty]
    Color defaultColorForScrollView = Color.Parse("White");

    [ObservableProperty]
    Color defaultColorForContentVSL = Color.Parse("White");

    [ObservableProperty]
    Color defaultColorForBorder = Color.Parse("LightBlue");

    [ObservableProperty]
    Color defaultColorForEditor = Color.Parse("WhiteSmoke");

    [ObservableProperty]
    Color defaultColorForHSL = Color.Parse("LightBlue");
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

            var notes = await notesService.GetNotesAsync();

            foreach (var note in notes)
            {
                var paperId = note.PaperId;
                var seqId = note.SequenceId;
                var locationId = note.LocationId;
                Notes.Add(note);
            }

            NoteCount = Notes.Count();
            ArrayTooltips = new string[NoteCount];
            foreach (var note in Notes)
            {
                await SetTooltip(note.Id.ToString("0"));
            }

            string titleMessage = $"Notes {NoteCount}";
            Title = titleMessage;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised {_method}.{_class} => ", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task NotesPageLoaded()
    {
        string _method = "NotesPageLoaded";
        try
        {
            if (contentPage == null)
            {
                return;
            }

            foreach (var location in Notes)
            {
                var paperId = location.PaperId;
                var seqId = location.SequenceId;
                var locationId = location.LocationId;
                var paragraph = Paragraphs.Where(p => p.PaperSeqId == locationId).FirstOrDefault();
                if (paragraph == null)
                {
                    paragraph = await notesService.GetParagraphAsync(paperId, seqId);
                    Paragraphs.Add(paragraph);
                }
            }
            var _notes = Notes.ToList();
            await LoadXamlAsync(_notes);

            var ids = await LoadUniqueIdsForPaper(NewNotePaperId);
            Uids.Clear();
            foreach (var id in ids)
            {
                Uids.Add(id);
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised {_method}.{_class} => ", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task EditorLoaded(string id)
    {
        // How to get Sender
        // see: https://stackoverflow.com/questions/76613536/maui-event-to-command-behavior-how-to-get-sender-and-args-inside-command
        string _method = "EditorLoaded";
        try
        {
            if (contentPage == null)
            {
                return;
            }

            Id = id;

            var button1 = await GetButtonAsync("Delete", id);
            var button2 = await GetButtonAsync("Share", id);
            var button3 = await GetButtonAsync("Save", id);
  
            Buttons.Add(button1);
            Buttons.Add(button2);
            Buttons.Add(button3);

            Editor editor = await GetEditorAsync(Id);
            if (editor != null) 
            {
                Editors.Add(editor);
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised {_method}.{_class} => ", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task EditorFocused(object obj)
    {
        string _method = "EditorFocused(";
        try
        {
            if (contentPage == null)
            {
                return;
            }

            var editor = (Editor)obj;
            string name = editor.ClassId;
            LoadedText = CurrentNoteText = PreviousNoteText = editor.Text;
            CurrentNoteName = name;
            CurrentNoteId = name.Split("_")[1];
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised {_method}.{_class} => ", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task EditorUnfocused(object obj)
    {
        string _method = "EditorUnfocused";
        try
        {
            if (contentPage == null)
            {
                return;
            }

            var editor = (Editor)obj;
            string name = editor.ClassId;
            string id = name.Replace("Editor_", "");

            PreviousNoteName = name;
            PreviousNoteId = name.Split("_")[1];
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised {_method}.{_class} => ", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task EditorTextChanged(Editor editor)
    {
        string _method = "EditorTextChanged";
        try
        {
            if (contentPage == null)
            {
                return;
            }

            var classId = editor.ClassId;
            var classIdArray = classId.Split("_");
            var name = classIdArray[0];
            var id = classIdArray[1];
            var _id = Int32.Parse(id);
            var newText = editor.Text;
            var oldText = CurrentNoteText;
            PreviousNoteText = CurrentNoteText;
            CurrentNoteText = newText.Trim();

            Button saveButton = await GetButtonAsync("Save", id);
            Button deleteButton = await GetButtonAsync("Delete", id);
            Button shareButton = await GetButtonAsync("Share", id);

            if (CurrentNoteText == LoadedText)
            {
                var hit = Hits[_id - 1];
                hit.IsDirty = false;
                NotesDirty = false;
                saveButton.IsVisible = false;
                deleteButton.IsVisible = false;
                shareButton.IsVisible = false;
            }
            else
            {
                NotesDirty = true;
                var hit = Hits[_id - 1];
                hit.IsDirty = true;
                saveButton.IsEnabled = true;
                deleteButton.IsEnabled = true;
                saveButton.IsVisible = true;
                deleteButton.IsVisible = true;
                shareButton.IsVisible = true;
                shareButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised {_method}.{_class} => ", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task EditorCompleted(Editor editor)
    {
        string _method = "EditorCompleted";
        try
        {
            if (contentPage == null)
            {
                return;
            }

            var classId = editor.ClassId;
            var classIdArray = classId.Split("_");
            var name = classIdArray[0];
            var id = classIdArray[1];
            var _id = Int32.Parse(id);
            var newText = editor.Text;
            var oldText = CurrentNoteText;
            PreviousNoteText = CurrentNoteText;
            CurrentNoteText = newText.Trim();

            var hit = Hits[_id - 1];
            // Update datastore
            var note = Notes.Where(n => n.Id == _id).FirstOrDefault();
            note.Text = newText;
            if (NotesDirty && !NoteSaved)
            {
                // Save Notes
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised {_method}.{_class} => ", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task EditorCheckedChanged(CheckBox checkbox)
    {
        string _method = "EditorCheckedChanged";
        try
        {
            if (contentPage == null)
            {
                return;
            }

            string name = checkbox.ClassId;
            int hitId = Int32.Parse(name.Replace("CheckBox_", ""));
            bool isSelected = checkbox.IsChecked;
            NoteHit hit = Hits.Where(h => h.Hit == hitId).FirstOrDefault();
            hit.Selected = isSelected;

        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised {_method}.{_class} => ", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task AuthorCompleted(string authorName)
    {
        string _method = "AuthorCompleted";
        try
        {
            if (contentPage == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(authorName))
            {
                //NewNoteAuthor = NoteAuthor;
                AuthorIsValid = false;
            }
            else
            {
                NewNoteAuthor = authorName;
                AuthorIsValid = true;
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised {_method}.{_class} => ", ex.Message, "Cancel");
            return;
        }
    }
    [RelayCommand]
    async Task SubjectCompleted(string subject)
    {
        string _method = "SubjectCompleted";
        try
        {
            if (contentPage == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(subject))
            {
                //NewNoteSubject = NoteSubject;
                SubjectIsValid = false;
            }
            else
            {
                NewNoteSubject = subject;
                SubjectIsValid = true;
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised {_method}.{_class} => ", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task RadioCheckedChanged(RadioButton radioButton)
    {
        string _method = "RadioCheckedChanged";
        try
        {
            if (contentPage == null)
            {
                return;
            }

            var name = radioButton.StyleId;
            var content = radioButton.Content;
            var isChecked = radioButton.IsChecked;

            //if (string.IsNullOrEmpty(type)) 
            //{
            //    // Error
            //    return;
            //}
            //NewNoteType = type;
            //TypeIsValid = true;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised {_method}.{_class} => ", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task NewNoteTextChanged(Editor entry)
    {
        string _method = "PaperIdTextChanged";
        try
        {
            // Do Nothing
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised {_method}.{_class} => ", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task NewNoteUnfocused(Editor entry)
    {
        string _method = "NewNoteUnfocused";
        try
        {
            string msg = $"";
            string digits = string.Empty;
            string range = string.Empty;

            var name = entry.StyleId;
            var _name = name.Replace("Editor", "");
            var value = entry.Text;

            var isSuccess = Int32.TryParse(value, out var result);
            if (!isSuccess)
            {
                NewNotePaperIdText = NewNotePaperId.ToString("0");
                switch (_name)
                {
                    case "paperId":
                        digits = "3";
                        range = "0 to 196";
                        break;
                    case "sectionId":
                        digits = "2";
                        range = "0 to 14";
                        break;
                    case "paragraphId":
                        digits = "2";
                        range = "0 to 45";
                        break;
                    default:
                        break;
                }
                string message = $"Only {digits} digit numeric types are allowed. " +
                                 $"Please enter a valid {range} digit number.";
                await App.Current.MainPage.DisplayAlert("Invalid Entry", message, "Ok");
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised {_method}.{_class} => ", ex.Message, "Cancel");
            return;
        }
    }
    [RelayCommand]
    async Task NewNoteCompleted(Editor editor)
    {
        string _method = "NewNoteCompleted";
        try
        {
            string styleId = editor.StyleId;
            string name = styleId.Replace("Editor", "");
            var value = editor.Text;
            var currentPid = string.Empty;
            bool isValid = false;

            switch (name)
            {
                case "paperId":
                    int _paperId = Int32.Parse(value);
                    isValid = NewNotePaperId == _paperId;
                    NewNotePaperId = _paperId;
                    currentPid = NewNotePaperId + ":" + NewNoteSectionId + "." + NewNoteParagraphId;
                    if (!isValid)
                    {
                        await UpdateUids(NewNotePaperId);
                        bool isAllZeros = NewNoteSectionId == 0 && NewNoteParagraphId == 0;
                        if (!isAllZeros)
                        {
                            // set vm fields for new note
                        }
                        // otherwise ignore
                    }
                    break;
                case "sectionId":
                    break;
                case "paragraphId":
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised {_method}.{_class} => ", ex.Message, "Cancel");
            return;
        }
    }

    //[RelayCommand]
    //async Task PaperIdEditorCompleted(Editor entry)
    //{
    //    string _method = "PaperIdEditorCompleted";
    //    try
    //    {
    //        var value = entry.Text;
    //        if (!value.IsNumeric())
    //        {
    //            entry.Text = "0";
    //            string message = $"Only three digit numeric types are allowed. " +
    //                             $"Please enter a valid 1 to 3 digit number.";
    //            await App.Current.MainPage.DisplayAlert("Invalid Entry", message, "Ok");
    //        }
    //        else
    //        {
    //            int newInt = -1;
    //            var result = Int32.TryParse(value, out newInt);
    //            if (result)
    //            {
    //                NewNotePaperId = newInt;
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        await App.Current.MainPage.DisplayAlert($"Exception raised {_method}.{_class} => ", ex.Message, "Cancel");
    //        return;
    //    }
    //}

    [RelayCommand]
    async Task PaperIdValueChanged(double paperId)
    {
        string _method = "PaperIdValueChanged";
        try
        {
            if (contentPage == null)
            {
                return;
            }

            PaperIdStepper = paperId;
            NewNotePaperId = Convert.ToInt32(paperId);
            NewNotePaperIdText = NewNotePaperId.ToString("0");
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised {_method}.{_class} => ", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task SectionIdValueChanged(double sectionId)
    {
        string _method = "PaperIdCompleted";
        try
        {
            if (contentPage == null)
            {
                return;
            }

            SectionIdStepper = sectionId;
            NewNoteSectionId = Convert.ToInt32(sectionId);

            var currentPid = NewNotePaperId + ":" + NewNoteSectionId + "." + NewNoteParagraphId;
            var uid = await GetUid(currentPid);
            if (uid != null)
            {
                NewNoteSequenceId = Int32.Parse(uid.SequenceId);
            }
            NewNoteLocationId = NewNotePaperId + "." + NewNoteSequenceId;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised {_method}.{_class} => ", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task ParagraphIdValueChanged(double paragraphId)
    {
        string _method = "ParagraphIdCompleted";
        try
        {
            if (contentPage == null)
            {
                return;
            }
            ParagraphIdStepper = paragraphId;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised {_method}.{_class} => ", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task ButtonLoaded(Button button)
    {
        string _method = "ButtonsLoaded";
        try
        {
            var styleId = button.StyleId;
            var text = button.Text;
            
            if (text.Equals("Cancel"))
            {
                await SendToastAsync("Cancel Note Loaded ... ");
            }
            if (text.Equals("Clear"))
            {
                await SendToastAsync("Clear Note Loaded ... ");
            }
            if (text.Equals("Save"))
            {
                await SendToastAsync("Save Note Loaded ... ");
            }
            button.IsEnabled = true;
            button.IsVisible = true;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised {_method}.{_class} => ", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task ButtonClicked(Button button)
    {
        string _method = "ButtonsClicked";
        try
        {
            var styleId = button.StyleId;
            var text = button.Text;

            if (text.Equals("Cancel"))
            {
                // Call CancelNote
                await CancelNote();
            }
            if (text.Equals("Clear"))
            {
                this.NotesDirty = false;
                this.EditingNewNote = false;
                this.NewNoteAuthor = string.Empty;
                this.AuthorIsValid = false;
                this.NewNoteSubject = string.Empty;
                this.SubjectIsValid = false;
                this.NewNoteType = string.Empty;
                this.TypeIsValid = false;
                this.NewNotePaperId = 0;
                this.NewNoteSectionId = 0;
                this.NewNoteParagraphId = 0;
                await SendToastAsync("Button Clear clicked ...");
            }
            if (text.Equals("Save"))
            {
                // Create note
                await CreateNewNoteAsync();
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised {_method}.{_class} => ", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task<string> GetTooltip(string id)
    {
        string _method = "GetTooltip";
        try
        {
            string tooltip = string.Empty;
            var result = Tooltips.TryGetValue("1", out tooltip);
            return tooltip;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return string.Empty;
        }
    }

    [RelayCommand]
    async Task SetTooltip(string id)
    {
        string _method = "SetTooltip";
        try
        {
            var _id = Int32.Parse(id);
            var note = Notes.Where(n => n.Id == _id).FirstOrDefault();
            var auth = note.Author;
            var subj = note.Subject;
            var created = note.DateCreated;
            var edited = note.DateEdited;
            var paperId = note.PaperId;
            var seqId = note.SequenceId;
            var locId = note.LocationId;
            var pid = note.Pid;

            var _CurrentAuthorSubject = $"Author: {auth}\rSubject: {subj}\r";
            var _CurrentCreateEdit = $"Created: {created}\rEdited: {edited}\r";
            var _CurrentNoteIds = $"Location: {locId}\rPid: {pid}";
            string tooltip = _CurrentAuthorSubject + _CurrentCreateEdit + _CurrentNoteIds;
            Tooltips.Add(id, tooltip);
            ArrayTooltips[_id - 1] = tooltip;

            // https://stackoverflow.com/questions/41386047/wpf-xaml-binding-array-with-variable-index
            //var _labelTooltip = contentPage.FindByName(name) as Label;
            //if (_labelTooltip != null)
            //{
            //    await MainThread.InvokeOnMainThreadAsync(() =>
            //    {
            //        _labelTooltip.Text = tooltip;
            //    });
            //}
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
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
                var edt = (Editor)visualTree[1];
                var hsl = (HorizontalStackLayout)visualTree[2];
                var lbl = (Label)visualTree[3];
                var chk = (CheckBox)visualTree[4];
                var btn = (Button)visualTree[5];
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
    async Task SaveNote(int editorId)
    {
        string _method = "SaveNote";
        try
        {
            var id = editorId;
            var _id = id - 1;
            var editor = Editors[_id];
            var text = editor.Text;
            // Notes
            // NoteLocations - remove not needed, load in VM
            // NoteList - null, not needed 
            // Paragraphs - get paragraphs when needed, remove
            // Hits - remove paragraph, not needed
            NotesDirty = false;
            Buttons[2].IsEnabled = false;
            await SendToastAsync("Save Note Feature Not Implemented Yet ...");
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    [RelayCommand]
    async Task DeleteNote(int editorId)
    {
        string _method = "DeleteNote";
        try
        {
            var id = editorId;
            var _id = id - 1;
            var editor = Editors[_id];
            var text = editor.Text;
            // Notes
            // NoteLocations - remove not needed, load in VM
            // NoteList - null, not needed 
            // Paragraphs - get paragraphs when needed, remove
            // Hits - remove paragraph, not needed
            await SendToastAsync("Delete Note Feature Not Implemented Yet ...");

        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    [RelayCommand]
    async Task ShareNote(int editorId)
    {
        string _method = "ShareNote";
        try
        {
            var id = editorId;
            var _id = id - 1;
            var editor = Editors[_id];
            var text = editor.Text;
            // Notes
            // NoteLocations - remove not needed, load in VM
            // NoteList - null, not needed 
            // Paragraphs - get paragraphs when needed, remove
            // Hits - remove paragraph, not needed
            await SendToastAsync("Share Note Feature Not Implemented Yet ...");

        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    [RelayCommand]
    async Task NewNote(Button button)
    {
        string _method = "NewNote";
        try
        {
            var name = button.StyleId;
            var text = button.Text;
            EditingNewNote = true;

            string promptMessage = string.Empty;
            var newNoteControl = contentPage.FindByName("newNoteControl") as NewNoteControl;
            if (newNoteControl != null)
            {
                var vsl = newNoteControl.FindByName("mainVSL") as VerticalStackLayout;
                if (vsl != null) 
                {
                    vsl.IsVisible = true;
                    var hsl = vsl.FindByName("buttonsHSL") as HorizontalStackLayout;
                    var btn = hsl.FindByName("saveNoteButton") as Button;
                    if (btn != null)
                    {
                        btn.IsEnabled = true;
                        //await MainThread.InvokeOnMainThreadAsync(() =>
                        //{
                        //    btn.IsEnabled = true;
                        //    button.IsEnabled = false;
                        //});
                    }
                    //await CreateNewNoteAsync();
                    //await SendToastAsync("New Note Feature Not Implemented Yet ...");
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
    async Task CreateNote(int editorId)
    {
        string _method = "CreateNote";
        try
        {
            var id = editorId;
            var _id = id - 1;
            var editor = Editors[_id];
            var text = editor.Text;

            NotesDirty = false;
            await SendToastAsync("Create Note Feature Not Implemented Yet ...");
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    [RelayCommand]
    async Task CancelNote()
    {
        string _method = "CancelNote";
        try
        {
            string promptMessage = string.Empty;
            var newNoteControl = contentPage.FindByName("newNoteControl") as NewNoteControl;
            if (newNoteControl != null)
            {
                var vsl = newNoteControl.FindByName("mainVSL") as VerticalStackLayout;
                vsl.IsVisible = false;
                this.NotesDirty = false;
                this.EditingNewNote = false;
                this.NewNoteAuthor = string.Empty;
                this.AuthorIsValid = false;
                this.NewNoteSubject = string.Empty;
                this.SubjectIsValid = false;
                this.NewNoteType = string.Empty;
                this.TypeIsValid = false;
                this.NewNotePaperId = 0;
                this.NewNoteSectionId = 0;
                this.NewNoteParagraphId = 0;
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    [RelayCommand]
    async Task ShareSelected(Button button)
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
            //await emailService.EmailParagraphsAsync(paragraphs, IEmailService.EmailType.PlainText, IEmailService.SendMode.AutoSend);
#elif ANDROID
            //await emailService.ShareParagraphsAsync(paragraphs);
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
    async Task FlyoutMenu(string actionId)
    {
        string _method = "FlyoutMenu";
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
    private async Task<UniqueId> GetUid(string pid)
    {
        string _method = "UpdateUids";
        try
        {
            UniqueId value = null;
            value = Uids.Where(u => u.Pid == pid).FirstOrDefault();
            return value;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised {_method}.{_class} => ", ex.Message, "Cancel");
            return null;
        }
    }
    private async Task<string> GetSequenceId(string pid)
    {
        string _method = "UpdateUids";
        try
        {
            UniqueId value = null;
            value = Uids.Where(u => u.Pid == pid).FirstOrDefault();
            return value.SequenceId;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised {_method}.{_class} => ", ex.Message, "Cancel");
            return null;
        }
    }
    private async Task UpdateUids(int paperId)
    {
        string _method = "UpdateUids";
        try
        {
            var ids = await LoadUniqueIdsForPaper(paperId);
            Uids.Clear();
            foreach (var id in ids)
            {
                Uids.Add(id);
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised {_method}.{_class} => ", ex.Message, "Cancel");
            return;
        }
    }
    private async Task<List<UniqueId>> LoadUniqueIdsForPaper(int paperId)
    {
        string _method = "LoadUniqueIdsForPaper";
        try
        {
            char[] delimiterChars = { '/', ':', '.' };
            string fileName = paperId.ToString("000") + ".pids.xml";
            var content = await fileService.LoadAsset("Lookup", fileName);
            var _doc = XDocument.Parse(content);
            var _root = _doc.Root;

            List<UniqueId> _uniqueIds = new();

            var uniqueIds = _root.Descendants("Uid");
            foreach (var id in uniqueIds) 
            {
                var _pid = id.Attribute("pid").Value;
                var _sequenceId = id.Attribute("seqId").Value;

                var _pidArr = _pid.Split(delimiterChars);
                var _paperId = _pidArr[0];
                var _sectionId = _pidArr[1];
                var _paragraphId = _pidArr[2];
                var newUID = new UniqueId()
                {
                    Id = 0,
                    Pid = _pid,
                    PaperId = _paperId,
                    SectionId = _sectionId,
                    ParagraphId = _paragraphId,
                    SequenceId = _sequenceId,
                    LocationId = _paperId + "." + _sequenceId
                };
                _uniqueIds.Add(newUID);
            }
            return _uniqueIds;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task LoadXamlAsync(List<NoteEntry> dtos, bool plainText = true, bool clear = false)
    {
        string _method = "LoadXamlAsync";
        try
        {
            var contentScrollView = contentPage.FindByName("contentScrollView") as ScrollView;
            var contentVSL = contentPage.FindByName("contentVerticalStackLayout") as VerticalStackLayout;
            var notes = dtos;
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
                var style = note.Style;
                var text = note.Text;

                string _noteStyle = string.Empty;
                switch(style)
                {
                    case "regular":
                        _noteStyle = "RegularParagraph";
                        break;
                    default:
                        _noteStyle = "RegularParagraph";
                        break;
                }
                NoteStyle = _noteStyle;

                DefaultNoteSpanStyle = _noteStyle.Replace("Paragraph", "Span");

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
                    LocationId = paperId + "." + seqId,
                    Pid = pid,
                    Selected = false,
                    IsDirty = false
                };
                Hits.Add(noteHit);

                // Create Span List
                var spans = await GetSpansAsync(note, paragraph, plainText);
                // Create FormattedString
                FormattedString fs = await CreateFormattedStringAsync(note, noteHit, spans, hit);
                // Create Editor
                Editor editor = await CreateEditorAsync(note, fs, labelName, paperId, seqId, pid);
                // Create Border
                Border newBorder = await CreateBorderAsync(note, editor, paperId, seqId);

                contentVSL.Add(newBorder);
            }
            contentScrollView.Content = contentVSL;

            string titleMessage = string.Empty;
            titleMessage = $"Notes ({hit} hits)";
            Title = titleMessage;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }
    private async Task CreateNewNoteAsync(bool plainText = true)
    {
        string _method = "CreateNewNoteAsync";
        try
        {
            //string action = await App.Current.MainPage.DisplayActionSheet("Note Type?", "Cancel", 
            //       null, "Regular", "Paragraph");

            //var contentScrollView = contentPage.FindByName("contentScrollView") as ScrollView;
            //var contentVSL = contentPage.FindByName("contentVerticalStackLayout") as VerticalStackLayout;

            //NoteHit noteHit = new NoteHit()
            //{
            //    Id = "0.0",
            //    Subject = "[Subject]",
            //    Hit = Hits.Count + 1,
            //    PaperId = 0,
            //    SequenceId = 0,
            //    LocationId = "0.0",
            //    Pid = "0:0.0",
            //    Selected = false,
            //    IsDirty = true
            //};

            //DefaultNoteSpanStyle = "RegularSpan";

            //Span newSpan = new Span();
            //newSpan.Text = "Enter text here ...";
            //newSpan.Style = (Style)App.Current.Resources["RegularSpan"];
            //FormattedString formattedString = new FormattedString();
            //formattedString.Spans.Add(newSpan);

            //var lastId = await notesService.LastNoteId();
            //var noteId = ++lastId;
            //var locationId = 0 + "." + 0;

            //NoteEntry note = new NoteEntry();
            //note.Id = noteId;
            //note.Type = "General Note";
            //note.Subject = "New Note";
            //note.Author = "Note Author";
            //note.PaperId = noteHit.PaperId;
            //note.SequenceId = noteHit.SequenceId;
            //note.LocationId = noteHit.LocationId;
            //note.Pid = noteHit.Pid;
            //note.DateCreated = DateTime.Now;
            //note.DateEdited = DateTime.Now;
            //note.Style = "RegularParagaph";
            //note.Text = "Enter note text here ...";


            //Editor editor = new Editor()
            //{
            //    ClassId = "Editor_" + noteId,
            //    IsVisible = true,
            //    IsEnabled = true,
            //    IsTextPredictionEnabled = true,
            //    IsSpellCheckEnabled = true,
            //    AutoSize = EditorAutoSizeOption.TextChanges,
            //    MinimumHeightRequest = 15,
            //    MaximumHeightRequest = 300,
            //    MinimumWidthRequest = 750,
            //    MaximumWidthRequest = 750,
            //    Text = "Enter text here ..."
            //};

            //var loadedBehavior = new EventToCommandBehavior
            //{
            //    EventName = "Loaded",
            //    Command = EditorLoadedCommand,
            //    CommandParameter = noteId
            //};

            //var focusedBehavior = new EventToCommandBehavior
            //{
            //    EventName = "Focused",
            //    Command = EditorFocusedCommand,
            //    CommandParameter = editor
            //};

            //var unfocusedBehavior = new EventToCommandBehavior
            //{
            //    EventName = "Unfocused",
            //    Command = EditorUnfocusedCommand,
            //    CommandParameter = editor
            //};

            //var textChangedBehavior = new EventToCommandBehavior
            //{
            //    EventName = "TextChanged",
            //    Command = EditorTextChangedCommand,
            //    CommandParameter = editor
            //};

            //var completedBehavior = new EventToCommandBehavior
            //{
            //    EventName = "Completed",
            //    Command = EditorCompletedCommand,
            //    CommandParameter = editor
            //};
            //editor.Behaviors.Add(loadedBehavior);
            //editor.Behaviors.Add(focusedBehavior);
            //editor.Behaviors.Add(unfocusedBehavior);
            //editor.Behaviors.Add(textChangedBehavior);
            //editor.Behaviors.Add(completedBehavior);

            //var viewModelDefaultBackgroundColorBindng = new Binding();
            //viewModelDefaultBackgroundColorBindng.Source = this.defaultEditorColor;
            //editor.SetBinding(Editor.BackgroundColorProperty, viewModelDefaultBackgroundColorBindng);

            //VerticalStackLayout vsl = new VerticalStackLayout()
            //{
            //    ClassId = "VSL_" + noteId,
            //    Padding = new Thickness(0),
            //    HorizontalOptions = LayoutOptions.Start, // Obsolte, use Grid
            //    VerticalOptions = LayoutOptions.Start    // Obsolete, use Grid
            //};

            //Label paragraphLabel = new Label()
            //{
            //    ClassId = "paragraphLabel" + noteId,
            //    Text = "General Note",
            //    Margin = new Thickness(0, 0, 10, 0),
            //    WidthRequest = 110,
            //    HorizontalOptions = LayoutOptions.Start,
            //    VerticalOptions = LayoutOptions.Center,
            //    VerticalTextAlignment = TextAlignment.Center
            //};
            //var tooltipPara = "Default tooltip ... ";
            //paragraphLabel.SetValue(ToolTipProperties.TextProperty, tooltipPara);

            //var width = editor.MinimumWidthRequest;
            //HorizontalStackLayout hsl = new HorizontalStackLayout()
            //{
            //    ClassId = "HSL_" + noteId,
            //    StyleId = "HSL_" + noteId,
            //    Margin = new Thickness(10, 5, 6, 0),
            //    HorizontalOptions = LayoutOptions.End,
            //};
            //hsl.Add(paragraphLabel);

            //var editorWidthPropertyBinding = new Binding();
            //editorWidthPropertyBinding.Source = editor;
            //editorWidthPropertyBinding.Path = "WidthRequest";

            //hsl.BindingContext = editor;
            //hsl.SetBinding(HorizontalStackLayout.WidthProperty, editorWidthPropertyBinding);

            //var viewModelHSLBackgroundColorBindng = new Binding();
            //viewModelHSLBackgroundColorBindng.Source = this.DefaultHSLColor;
            //hsl.SetBinding(Border.BackgroundColorProperty, viewModelHSLBackgroundColorBindng);

            //CheckBox checkBox = await CreateCheckBoxAsync(note, locationId);
            //Button saveButton = await CreateButtonAsync("Save", true, note, locationId);
            //Button deleteButton = await CreateButtonAsync("Delete", true, note, locationId);
            //Button shareButton = await CreateButtonAsync("Share", true, note, locationId);

            //hsl.Add(checkBox);
            //hsl.Add(deleteButton);
            //hsl.Add(shareButton);
            //hsl.Add(saveButton);

            //vsl.Add(editor);
            //vsl.Add(hsl);

            //RoundRectangle rectangle = new RoundRectangle()
            //{
            //    CornerRadius = 5,
            //};

            //Border newBorder = new Border()
            //{
            //    ClassId = "Border_" + noteId,
            //    Stroke = Colors.Blue,
            //    StrokeShape = rectangle,
            //    StrokeThickness = 1,
            //    Margin = new Thickness(10, 5, 10, 5),
            //    HorizontalOptions = LayoutOptions.Start,
            //    Content = vsl
            //};

            //string auth = "Robert Reno";
            //string subj = "General Note";
            //string created = note.DateCreated.ToString();
            //string edited = note.DateEdited.ToString();
            //string locId = note.LocationId;
            //string pid = note.Pid;
            //var _CurrentAuthorSubject = $"Author: {auth}\rSubject: {subj}\r";
            //var _CurrentCreateEdit = $"Created: {created}\rEdited: {edited}\r";
            //var _CurrentNoteIds = $"Location: {locId}\rPid: {pid}";
            //string tooltip = _CurrentAuthorSubject + _CurrentCreateEdit + _CurrentNoteIds;

            ////Tooltips.Add(noteId.ToString("0"), tooltip);
            ////ArrayTooltips[lastId - 1] = tooltip;

            ////int _id = noteId - 1;
            ////var tooltipBorder = ArrayTooltips[_id];
            ////newBorder.SetValue(ToolTipProperties.TextProperty, tooltipBorder);

            //var style = (Style)App.Current.Resources["NoteBorderStyle"];
            //newBorder.SetValue(Border.StrokeShapeProperty, rectangle);
            //newBorder.SetValue(Border.StyleProperty, style);

            //var viewModelBorderBackgroundColorBindng = new Binding();
            //viewModelBorderBackgroundColorBindng.Source = this.defaultBorderColor;
            //newBorder.SetBinding(Border.BackgroundColorProperty, viewModelBorderBackgroundColorBindng);

            //Borders.Add(newBorder);
            //contentVSL.Add(newBorder);

            //contentScrollView.Content = contentVSL;

            await SendToastAsync("New Note Create/Save note impl ...");
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }
    private async Task<List<Span>> GetSpansAsync(NoteEntry note, Paragraph paragraph, bool plainText = true)
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
        string _method = "CreateNewSpanAsync";
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
    private async Task<FormattedString> CreateFormattedStringAsync(NoteEntry note, NoteHit noteHit, List<Span> spans, int hit)
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

            //Span tabSpan = new Span() { Style = (Style)App.Current.Resources["TabsSpan"] };
            //Span pidSpan = new Span() { Style = (Style)App.Current.Resources["PID"], StyleId = labelName, Text = pid };
            //Span spaceSpan = new Span() { Style = (Style)App.Current.Resources["RegularSpaceSpan"] };
            //Span hitsSpan = new Span() { Style = (Style)App.Current.Resources["HID"], StyleId = labelName, Text = $"[hit {hit}]" };

            //formattedString.Spans.Add(hitsSpan);
            //formattedString.Spans.Add(tabSpan);
            //formattedString.Spans.Add(pidSpan);
            //formattedString.Spans.Add(spaceSpan);

            foreach (var span in spans)
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
    private async Task<Editor> CreateEditorAsync(NoteEntry note, FormattedString fs, string labelName, int paperId, int seqId, string pid)
    {
        string _method = "CreateEditorAsync";
        try
        {
            var id = note.Id;
            var locationId = paperId + "." + seqId;
            Editor editor = new Editor() 
            { 
                ClassId = "Editor_" + note.Id,
                IsVisible = true,
                IsEnabled = true,
                IsTextPredictionEnabled = true,
                IsSpellCheckEnabled = true,
                AutoSize = EditorAutoSizeOption.TextChanges,
                MinimumHeightRequest = 15,
                MaximumHeightRequest = 300,
                MinimumWidthRequest = 750,
                MaximumWidthRequest = 750,
                Text = note.Text
            };
            int _id = id - 1;
            var tooltip = ArrayTooltips[id - 1];
            Tooltips.TryGetValue(id.ToString("0"), out tooltip);

            var loadedBehavior = new EventToCommandBehavior
            { 
                EventName = "Loaded",
                Command = EditorLoadedCommand,
                CommandParameter = note.Id.ToString("0")
            };

            var focusedBehavior = new EventToCommandBehavior
            {
                EventName = "Focused",
                Command = EditorFocusedCommand,
                CommandParameter = editor
            };

            var unfocusedBehavior = new EventToCommandBehavior
            {
                EventName = "Unfocused",
                Command = EditorUnfocusedCommand,
                CommandParameter = editor
            };

            var textChangedBehavior = new EventToCommandBehavior
            {
                EventName = "TextChanged",
                Command = EditorTextChangedCommand,
                CommandParameter = editor
            };

            var completedBehavior = new EventToCommandBehavior
            {
                EventName = "Completed",
                Command = EditorCompletedCommand,
                CommandParameter = editor
            };
            editor.Behaviors.Add(loadedBehavior);
            editor.Behaviors.Add(focusedBehavior);
            editor.Behaviors.Add(unfocusedBehavior);
            editor.Behaviors.Add(textChangedBehavior);
            editor.Behaviors.Add(completedBehavior);

            var viewModelDefaultBackgroundColorBindng = new Binding();
            viewModelDefaultBackgroundColorBindng.Source = DefaultColorForEditor;
            editor.SetBinding(Editor.BackgroundColorProperty, viewModelDefaultBackgroundColorBindng);

            return editor;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class} . {_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task<Border> CreateBorderAsync(NoteEntry note, Editor editor, int paperId, int seqId)
    {
        string _method = "CreateBorderAsync";
        try
        {
            var id = note.Id;
            var locationId = paperId + "." + seqId;
            VerticalStackLayout vsl = new VerticalStackLayout()
            {
                ClassId = "VSL_" + note.Id,
                Padding = new Thickness(0),
                HorizontalOptions = LayoutOptions.Start, // Obsolte, use Grid
                VerticalOptions = LayoutOptions.Start    // Obsolete, use Grid
            };

            var type = note.Type;
            switch (type)
            {
                case "paragraph":
                    type = "Paragraph Note";
                    break;
                case "section":
                    type = "Section Note";
                    break;
                case "paper":
                    type = "Paper Note";
                    break;
                default:
                    type = "General Note";
                    break;
            }

            Label paragraphLabel = new Label() 
            {
                ClassId = "paragraphLabel" + note.Id,
                Text = type,
                Margin = new Thickness(0, 0, 10, 0),
                WidthRequest = 110,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
            var tooltipPara = "Default tooltip ... ";
            paragraphLabel.SetValue(ToolTipProperties.TextProperty, tooltipPara);

            var width = editor.MinimumWidthRequest;
            HorizontalStackLayout hsl = new HorizontalStackLayout()
            {
                ClassId = "HSL_" + note.Id,
                StyleId = "HSL_" + note.Id,
                Margin = new Thickness(10, 5, 6, 0),
                HorizontalOptions = LayoutOptions.End,
            };
            hsl.Add(paragraphLabel);

            var editorWidthPropertyBinding = new Binding();
            editorWidthPropertyBinding.Source = editor;
            editorWidthPropertyBinding.Path = "WidthRequest";

            hsl.BindingContext = editor;
            hsl.SetBinding(HorizontalStackLayout.WidthProperty, editorWidthPropertyBinding);

            var viewModelHSLBackgroundColorBindng = new Binding();
            viewModelHSLBackgroundColorBindng.Source = DefaultColorForHSL;
            hsl.SetBinding(Border.BackgroundColorProperty, viewModelHSLBackgroundColorBindng);

            // TODO: How to create FlyoutMenu for hsl control.
            MenuFlyout menuFlyout = new MenuFlyout();
            MenuFlyoutItem flyoutItem1 = new MenuFlyoutItem()
            {
                Text = "Email",
                Command = this.FlyoutMenuCommand,
                CommandParameter = "Email_" + note.Id,
            };
            MenuFlyoutItem flyoutItem2 = new MenuFlyoutItem()
            {
                Text = "Share",
                Command = this.FlyoutMenuCommand,
                CommandParameter = "Share_" + note.Id,
            };
            menuFlyout.Add(flyoutItem1);
            menuFlyout.Add(flyoutItem2);

            //hsl.SetBinding(HorizontalStackLayout.BehaviorsProperty, ?)

            CheckBox checkBox = await CreateCheckBoxAsync(note, locationId);
            Button saveButton = await CreateButtonAsync("Save", true, note, locationId);
            Button deleteButton = await CreateButtonAsync("Delete", true, note, locationId);
            Button shareButton = await CreateButtonAsync("Share", true, note, locationId);

            hsl.Add(checkBox);
            hsl.Add(deleteButton);
            hsl.Add(shareButton);
            hsl.Add(saveButton);
            
            vsl.Add(editor);
            vsl.Add(hsl);

            RoundRectangle rectangle = new RoundRectangle()
            { 
                CornerRadius = 5,
            };

            Border newBorder = new Border()
            {
                ClassId = "Border_" + note.Id,
                Stroke = Colors.Blue,
                StrokeShape = rectangle,
                StrokeThickness = 1,
                Margin = new Thickness(10, 5, 10, 5),
                HorizontalOptions = LayoutOptions.Start,
                Content = vsl
            };
            int _id = id - 1;
            var tooltipBorder = ArrayTooltips[_id];
            var style = (Style)App.Current.Resources["NoteBorderStyle"];
            newBorder.SetValue(Border.StrokeShapeProperty, rectangle);
            newBorder.SetValue(Border.StyleProperty, style);
            newBorder.SetValue(ToolTipProperties.TextProperty, tooltipBorder);

            var viewModelDefaultBackgroundColorBindng = new Binding();
            viewModelDefaultBackgroundColorBindng.Source = this.DefaultColorForBorder;
            newBorder.SetBinding(Border.BackgroundColorProperty, viewModelDefaultBackgroundColorBindng);

            return newBorder;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}  .  {_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task<CheckBox> CreateCheckBoxAsync(NoteEntry note, string locationId)
    {
        string _method = "CreateCheckBoxEx";
        try
        {
            CheckBox checkBox = new CheckBox()
            {
                ClassId = "CheckBox_" + note.Id,
                IsChecked = false,
                IsVisible = true,
                Margin = new Thickness(0, 0, 10, 0),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,

            };
            checkBox.SetValue(ToolTipProperties.TextProperty, $"Check to share note ...");

            var behavior = new EventToCommandBehavior
            {
                EventName = "CheckedChanged",
                Command = EditorCheckedChangedCommand,
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
    private async Task<Button> CreateButtonAsync(string action, bool isVisible, NoteEntry note, string locationId)
    {
        string _method = "CreateButton";
        try
        {
            var id = note.Id;
            var _id = id.ToString("0");
            Button button = new Button()
            {
                ClassId = action + "_" + "Button_" + note.Id,
                StyleId = action + "_" + "Button_" + note.Id,
                Margin = new Thickness(0, 0, 2, 0),
                WidthRequest = 70,
                HeightRequest = 10,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                IsEnabled = true,
                IsVisible = false, // testing with true
                Text = action
            };

            var viewModelNotesDirtyBindng = new Binding() { Mode = BindingMode.TwoWay };
            viewModelNotesDirtyBindng.Source = this.NotesDirty;
            button.SetBinding(Button.IsEnabledProperty, viewModelNotesDirtyBindng);

            var cmd = action == "Save" ? SaveNoteCommand : DeleteNoteCommand;
            var behavior = new EventToCommandBehavior
            {
                EventName = "Clicked",
                Command = cmd,
                CommandParameter = id
            };
            button.Behaviors.Add(behavior);

            return button;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class} . {_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task<Label> CreateTypeLabelAsync(NoteEntry note, FormattedString fs, string labelName, int paperId, int seqId, string pid)
    {
        string _method = "CreateTypeLabel";
        try
        {
            Label label = new Label { FormattedText = fs };
            label.Style = (Style)App.Current.Resources["NoteLabelStyle"];
            label.ClassId = "Label_" + note.Id;
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
    private async Task<Label> MakeHeaderLabelAsync(Paragraph paragraph, int noteId)
    {
        string _method = "MakeHeaderLabel";
        try
        {
            FormattedString formattedString = new FormattedString();
            var paperId = paragraph.PaperId;
            var seqId = paragraph.SeqId;
            var pid = paragraph.Pid;
            var labelName = "_" + paperId.ToString("000") + "_" + seqId.ToString("000");

            Span tabSpan = new Span() { Style = (Style)App.Current.Resources["TabsSpan"] };
            Span pidSpan = new Span() { Style = (Style)App.Current.Resources["PID"], StyleId = labelName, Text = pid };
            Span spaceSpan = new Span() { Style = (Style)App.Current.Resources["RegularSpaceSpan"] };
            Span noteIdSpan = new Span() { Style = (Style)App.Current.Resources["HID"], StyleId = labelName, Text = $"[Id {noteId}]" };

            formattedString.Spans.Add(noteIdSpan);
            formattedString.Spans.Add(tabSpan);
            formattedString.Spans.Add(pidSpan);
            formattedString.Spans.Add(spaceSpan);

            Label label = new Label { FormattedText = formattedString };
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
    private async Task<Border> GetBorderAsync(string editorId)
    {
        string _method = "GetBorderAsync";
        try
        {
            string borderName = "Border_" + editorId;
            Border _border = new Border();

            var contentVSL = contentPage.FindByName("contentVerticalStackLayout") as VerticalStackLayout;
            var children = contentVSL.Children;

            var borders = children.Where(c => c.GetType().Name == "Border");
            foreach (var border in borders)
            {
                var bdr = (Border)border;
                if (bdr.ClassId.Equals(borderName))
                {
                    _border = bdr;
                }
            }
            return _border;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task<Editor> GetEditorAsync(string editorId)
    {
        string _method = "GetEditorAsync";
        try
        {
            string editorName = "Editor_" + editorId;
            Editor _editor = new Editor();

            var contentVSL = contentPage.FindByName("contentVerticalStackLayout") as VerticalStackLayout;
            var children = contentVSL.Children;

            var borders = children.Where(c => c.GetType().Name == "Border");
            foreach (var border in borders) 
            {
                var vtd = ((Border)border).Content.GetVisualTreeDescendants();
                var editor = vtd.Where(v => v.GetType().Name == "Editor").FirstOrDefault() as Editor;
                if (editor.ClassId.Equals(editorName)) 
                {
                    _editor = editor;
                }
            }
            return _editor;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task<Editor> GetVSLAsync(string editorId)
    {
        string _method = "GetVSLAsync";
        try
        {
            string editorName = "VSL_" + editorId;
            Editor _editor = new Editor();

            var contentVSL = contentPage.FindByName("contentVerticalStackLayout") as VerticalStackLayout;
            var children = contentVSL.Children;

            var borders = children.Where(c => c.GetType().Name == "Border");
            foreach (var border in borders)
            {
                var vtd = ((Border)border).Content.GetVisualTreeDescendants();
                var editor = vtd.Where(v => v.GetType().Name == "Editor").FirstOrDefault() as Editor;
                if (editor.ClassId.Equals(editorName))
                {
                    _editor = editor;
                }
            }
            return _editor;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task<Button> GetButtonAsync(string action, string buttonId)
    {
        string _method = "GetButtonAsync";
        try
        {
            string buttonName = action + "_Button_" + buttonId;
            Button _btn = new Button();

            var contentVSL = contentPage.FindByName("contentVerticalStackLayout") as VerticalStackLayout;
            var children = contentVSL.Children;

            var borders = children.Where(c => c.GetType().Name == "Border");
            foreach (var border in borders)
            {
                var vtd = ((Border)border).Content.GetVisualTreeDescendants();
                var btns = vtd.Where(v => v.GetType().Name == "Button");
                foreach (Button btn in btns)
                {
                    if (btn.ClassId.Equals(buttonName))
                    {
                        _btn = btn;
                    }
                }
            }
            return _btn;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task<CheckBox> GetCheckBoxAsync(string checkBoxId)
    {
        string _method = "GetCheckBoxAsync";
        try
        {
            string checkBoxName = "CheckBox_" + checkBoxId;
            CheckBox _cbx = null;

            var contentVSL = contentPage.FindByName("contentVerticalStackLayout") as VerticalStackLayout;
            var children = contentVSL.Children;

            var borders = children.Where(c => c.GetType().Name == "Border");
            foreach (var border in borders)
            {
                var vtd = ((Border)border).Content.GetVisualTreeDescendants();
                var cbx = vtd.Where(v => v.GetType().Name == "CheckBox").FirstOrDefault() as CheckBox;
                if (cbx.ClassId.Equals(checkBoxName))
                {
                    _cbx = cbx;
                }
            }

            return _cbx;
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
