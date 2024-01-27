namespace UBViews.Helpers;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

using UBViews.Models;
using UBViews.Models.Notes;
using UBViews.Models.Ubml;
using UBViews.Services;

public partial class NoteService : INoteService
{
    #region Private Data Members
    /// <summary>
    /// CultureInfo
    /// </summary>
    public CultureInfo cultureInfo;

    /// <summary>
    /// ContentPage
    /// </summary>
    public ContentPage contentPage;

    ObservableCollection<NoteEntry> Notes = new();

    IFileService fileService;

    private const string _notesFileName = "Notes.xml";
    private string _appDir = null;
    private string _contents = null;
    private XDocument _notes = null;
    private XElement _notesRoot = null;
    private int _lastNoteId = 0;

    private bool _initialized = false;
    private bool _dataInitialized = false;
    private bool _cacheDirty = false;

    readonly string _xmlData = "XmlData";
    readonly string _mauiUbml = "MauiUbml";
    readonly string _class = "NotesService";
    #endregion

    #region Constructor
    public NoteService(IFileService fileService)
    {
        this.fileService = fileService;
    }
    #endregion

    #region Private Initialization Methods
    private async Task InitializeDataAsync()
    {
        string _method = "InitializeDataAsync";
        try
        {
            // Sets _initialzed to true if successful
            var loadContentResult = await LoadContentAsync();
            if (!loadContentResult)
            {
                throw new Exception("Initialization Exception: Settings file failed to load.");
            }
            // Sets _dataInitialized to true if successful
            var loadDataResult = await InitializeSettingsAsync();
            if (!loadDataResult)
            {
                throw new Exception("Initialization Exception: Loading date failed.");
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }
    private async Task<bool> InitializeSettingsAsync()
    {
        string _method = "InitializeSettingsAsync";
        try
        {
            if (!_initialized)
            {
                throw new Exception("Initialization Exception.");
            }

            _lastNoteId = Int32.Parse(_notesRoot.Attribute("lastId").Value);
            var notes = _notesRoot.Descendants("Note");
            foreach (var note in notes)
            {
                NoteEntry newNote = new NoteEntry();
                var noteId = note.Attribute("id").Value;
                var noteType = note.Attribute("type").Value;
                var noteStyle = note.Attribute("style").Value;
                var paperId = note.Attribute("paperId").Value;
                var seqId = note.Attribute("seqId").Value;
                var locationId = note.Attribute("locationId").Value;
                var pid = note.Attribute("pid").Value;
                var createDate = note.Attribute("created").Value;
                var createTime = note.Attribute("createdTime").Value;
                var editDate = note.Attribute("edited").Value;
                var editTime = note.Attribute("editedTime").Value;

                var author = note.Descendants("Author").FirstOrDefault();
                var subject = note.Descendants("Subject").FirstOrDefault();
                var text = note.Descendants("Text").FirstOrDefault();
                var runs = note.Descendants("Runs").FirstOrDefault(); // <Runs count="0" />
                var count = runs.Attribute("count").Value;
                var _count = Int32.Parse(count);

                List<NoteRun> _newRuns = await MakeRunsAsync(runs);

                var _author = author == null ? "Uknown Author" : author.Value;
                var _subject = subject == null ? "Uknown Subject" : subject.Value;
                var _text = text == null ? "Uknown Text" : text.Value;
                var _runs = _newRuns;

                var createdDate = await CreateDateTimeAsync(createDate, createTime);
                var editedDate = await CreateDateTimeAsync(editDate, editTime);

                newNote.Id = Int32.Parse(noteId);
                newNote.Type = noteType;
                newNote.Style = noteStyle;
                newNote.PaperId = Int32.Parse(paperId);
                newNote.SequenceId = Int32.Parse(seqId);
                newNote.LocationId = locationId;
                newNote.Pid = pid;
                newNote.DateCreated = createdDate;
                newNote.DateEdited = editedDate;
                newNote.Author = _author;
                newNote.Subject = _subject;
                newNote.Text = _text;
                newNote.NoteRuns = _runs;
                Notes.Add(newNote);
            }
            _dataInitialized = true;
            return _dataInitialized;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return false;
        }
    }
    #endregion

    #region Private Methods
    private async Task<int> GetCurrentCountAsync()
    {
        string _method = "GetCurrentCountAsync";

        try
        {
            var _count = Int32.Parse(_notesRoot.Attribute("count").Value);
            return _count;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return 0;
        }
    }
    private async Task<int> IncrementCountAsync()
    {
        string _method = "IncrementCountAsync";

        try
        {
            var _count = Int32.Parse(_notesRoot.Attribute("count").Value);
            _notesRoot.SetAttributeValue("count", ++_count);
            _cacheDirty = true;
            return _count;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return 0;
        }
    }
    private async Task<int> DecrementCountAsync()
    {
        string _method = "DecrementCountAsync";

        try
        {
            var _count = Int32.Parse(_notesRoot.Attribute("count").Value);
            _notesRoot.SetAttributeValue("count", --_count);
            _cacheDirty = true;
            return _count;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return 0;
        }
    }
    private async Task<int> GetLastNoteIdAsync()
    {
        string _method = "GetLastContactId";

        try
        {
            var _lastId = Int32.Parse(_notesRoot.Attribute("lastId").Value);
            return _lastId;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return 0;
        }
    }
    private async Task<int> ResetLastNoteIdAsync()
    {
        string _method = "ResetContactIdAsync";

        try
        {
            _notesRoot.SetAttributeValue("lastId", 0);
            _cacheDirty = true;
            return 0;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return 0;
        }
    }
    private async Task<int> IncrementContactIdAsync()
    {
        string _method = "GetLastContactIdAsync";

        try
        {
            var _lastId = Int32.Parse(_notesRoot.Attribute("lastId").Value);
            _notesRoot.SetAttributeValue("lastId", ++_lastId);
            _cacheDirty = true;
            return _lastId;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return 0;
        }
    }

    // TODO: Make Into Public Methods
    public async Task<NoteEntry> GetNoteByIdAsync(string id)
    {
        string _method = "GetByIdAsyNotenc";

        try
        {
            NoteEntry _note = null;

            var noteElm = _notesRoot.Descendants("Note")
                                    .Where(c => c.Attribute("id").Value == id)
                                    .FirstOrDefault();
            if (noteElm != null)
            {
                var _id = Int32.Parse(noteElm.Attribute("id").Value);
                var type = noteElm.Attribute("type").Value;
                var style = noteElm.Attribute("style").Value;
                var paperId = noteElm.Attribute("paperId").Value;
                var seqId = noteElm.Attribute("seqId").Value;
                var locationId = noteElm.Attribute("locationId").Value;
                var pid = noteElm.Attribute("pid").Value;
                var created = noteElm.Attribute("created").Value;
                var edited = noteElm.Attribute("edited").Value;
                var createdTime = noteElm.Attribute("createdTime").Value;
                var editedTime = noteElm.Attribute("editedTime").Value;
                var author = noteElm.XPathSelectElement("Author").Value;
                var subject = noteElm.XPathSelectElement("Subject").Value;
                var text = noteElm.XPathSelectElement("Text").Value;
                var runs = noteElm.Descendants("Runs").FirstOrDefault();

                _note = new NoteEntry
                {
                    Id = _id,
                    Type = type,
                    Style = style,
                    PaperId = Int32.Parse(paperId),
                    SequenceId = Int32.Parse(seqId),
                    LocationId = locationId,
                    Pid = pid,
                    DateCreated = await CreateDateTimeAsync(created, createdTime),
                    DateEdited = await CreateDateTimeAsync(edited, editedTime),
                    Author = author,
                    Subject = subject,
                    Text = text,
                    NoteRuns = await MakeRunsAsync(runs)
                };
            }
            return _note;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task<XElement> GetNoteElementByIdAsync(string id)
    {
        string _method = "GetSettingElmByIdAsync";

        try
        {
            var _noteElm = _notesRoot.Descendants("Note")
                                     .Where(c => c.Attribute("id").Value == id)
                                     .FirstOrDefault();
            return _noteElm;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task<bool> EqualToAsync(XElement oldNote, NoteEntry newNote)
    {
        string _method = "EqualToAsync";
        try
        {
            var isEqual = false;

            if (oldNote != null)
            {
                var id = Int32.Parse(oldNote.Attribute("id").Value);

                var _oldNote = new NoteEntry
                {
                    Id = id,
                };

                isEqual = _oldNote.EqualTo(newNote);
            }
            return isEqual;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return false;
        }
    }
    public async Task<int> UpdateNoteAsync(NoteEntry note)
    {
        string _method = "UpdateNoteAsync";

        try
        {
            string message = string.Empty;
            XElement newNote = new XElement("Note",
                                new XAttribute("id", note.Id),
                                new XAttribute("type", note.Type),
                                new XAttribute("style", note.Style),
                                new XAttribute("paperId", note.PaperId));

            // update setting here 
            var targets = _notesRoot.Descendants("Note");
            XElement target = null;
            if (targets != null)
            {
                target = targets.Where(e => e.Attribute("id").Value == note.Id.ToString("0"))
                                .FirstOrDefault();
                target.XPathSelectElement("Text").Value = note.Text;
            }
            await SaveNotesAsync();

            message = "Successfully Saved!";
            await App.Current.MainPage.DisplayAlert($"Save Setting", message, "Ok");

            return note.Id;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return 0;
        }
    }
    public async Task<int> DeleteContactAsync(NoteEntry note)
    {
        string _method = "DeleteNoteAsync";
        try
        {
            string message = string.Empty;

            int noteId = note.Id;
            var id = note.Id.ToString("0");

            var oldNote = _notesRoot.Descendants("Note")
                                    .Where(c => c.Attribute("id").Value == id)
                                    .FirstOrDefault();

            oldNote.Remove();

            int _count = await DecrementCountAsync();
            if (_count == 0)
            {
                var _lastId = await ResetLastNoteIdAsync();
                await SaveNotesAsync();
            }
            else
            {
                await SaveNotesAsync();
            }

            message = "Successfully Deleted!";
            await App.Current.MainPage.DisplayAlert($"Update Note", message, "Ok");

            return noteId;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return 0;
        }
    }
    private async Task<int> SaveNoteAsync(NoteEntry note)
    {
        string _method = "SaveNoteAsync";
        try
        {
            string message = string.Empty;
            XElement newNote = new XElement("Note",
                                new XAttribute("id", note.Id),
                                new XAttribute("type", note.Type),
                                new XAttribute("style", note.Style),
                                new XAttribute("paperId", note.PaperId));

            // update setting here 
            var targets = _notesRoot.Descendants("Note");
            XElement target = null;
            if (targets != null)
            {
                target = targets.Where(e => e.Attribute("id").Value == note.Id.ToString("0"))
                                .FirstOrDefault();
                target.XPathSelectElement("Text").Value = note.Text;
            }
            await SaveNotesAsync();

            message = "Successfully Saved!";
            await App.Current.MainPage.DisplayAlert($"Save Setting", message, "Ok");

            return note.Id;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return 0;
        }
    }
    private async Task SaveNotesAsync()
    {
        string _method = "SaveNotesAsync";

        try
        {
            if (_cacheDirty)
            {
                string targetFile = System.IO.Path.Combine(_appDir, _notesFileName);
                _notesRoot.Save(targetFile, SaveOptions.None);
                _cacheDirty = false;
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }
    #endregion

    #region  Public Interface Methods
    public async Task<bool> IsDirtyAsync()
    {
        string _method = "IsDirty";
        try
        {
            return _cacheDirty;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return false;
        }
    }
    public async Task<int> LastNoteIdAsync()
    {
        string _method = "GetLastNoteId";
        try
        {
            return await GetLastNoteIdAsync();
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return 0;
        }
    }
    public async Task<List<NoteEntry>> GetNotesAsync()
    {
        string _method = "GetNotesAsync";
        try
        {
            if (!_initialized)
            {
                await InitializeDataAsync();
            }

            var notes = new List<NoteEntry>();
            notes = Notes.ToList();
            return notes;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    public async Task<NoteLocationsDto> GetNoteLocationsDtoAsync()
    {
        string _method = "GetNoteLocationsDtoAsync";
        try
        {
            var dto = new NoteLocationsDto();
            string contents = await LoadResourceContentAsync("Notes.xml");
            XDocument xdoc = XDocument.Parse(contents);
            XElement root = xdoc.Root;
            var notes = root.Descendants("Note");
            foreach (var note in notes)
            {
                var id = note.Attribute("id").Value;
                var type = note.Attribute("type").Value;
                var paperId = note.Attribute("paperId").Value;
                var sequenceId = note.Attribute("seqId").Value;
                var locationId = note.Attribute("locationId").Value;
                var pid = note.Attribute("pid").Value;
                var created = note.Attribute("created").Value;
                var edited = note.Attribute("edited").Value;
                var author = note.Element("Author").Value;
                var subject = note.Element("Subject").Value;

                var runsNode = note.Descendants("Runs").FirstOrDefault();

                var runs = runsNode.Descendants("Run");
                StringBuilder sb = new StringBuilder();
                NoteText noteText = new NoteText();
                noteText.Style = "RegularParagraph";
                foreach (var run in runs)
                {
                    var attribs = run.Attributes();
                    

                    var style = run.Attribute("style").Value;
                    var text = run.Value;
                    NoteRun newRun = new NoteRun() { Text = text };
                    foreach (var att in attribs)
                    {
                        var name = att.Name.LocalName;
                        var value = att.Value;
                        if (name.Equals("style"))
                        {
                            newRun.Style = value;
                        }
                        if (name.Equals("textSize"))
                        {
                            newRun.TextSize = value;
                        }
                        if (name.Equals("fontFamily"))
                        {
                            newRun.FontFamily = value;
                        }
                        if (name.Equals("textColor"))
                        {
                            newRun.TextColor = value;
                        }
                        if (name.Equals("textTransform"))
                        {
                            newRun.TextTransform = value;
                        }
                    }

                    sb.Append(text);
                    noteText.NoteRuns.Add(newRun);
                }
                noteText.Text = sb.ToString();

                var createdArry = created.Split("-");
                var editedArry  = edited.Split("-");

                var newNoteEntry = new NoteEntry()
                {
                    Id = Int32.Parse(id),
                    Type = type,
                    PaperId = Int32.Parse(paperId),
                    SequenceId = Int32.Parse(sequenceId),
                    LocationId = locationId,
                    Pid = pid,
                    DateCreated = new DateTime(Int32.Parse(createdArry[0]),
                                               Int32.Parse(createdArry[1]),
                                               Int32.Parse(createdArry[2])),
                    DateEdited = new DateTime(Int32.Parse(editedArry[0]),
                                              Int32.Parse(editedArry[1]),
                                              Int32.Parse(editedArry[2])),
                    Author = author,
                    Subject = subject
                };
                dto.Notes.Add(newNoteEntry);
            }
            return dto;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    public async Task<List<PaperDto>> GetPaperDtosAsync()
    {
        string _method = "GetPaperDtosAsync";
        try
        {
            List<PaperDto> titleList = new();

            string xml = await LoadAssetAsync(_xmlData, "PaperTitles.xml");
            var xdoc = XDocument.Parse(xml);
            var root = xdoc.Root;
            var titles = root.Descendants("Title");

            foreach (var title in titles)
            {
                PaperDto newTitle = new()
                {
                    Id = Int32.Parse(title.Attribute("paperId").Value),
                    Title = title.Attribute("paperTitle").Value,
                    Author = title.Attribute("paperAuthor").Value,
                    PartId = Int32.Parse(title.Attribute("partId").Value),
                    PartTitle = title.Attribute("partTitle").Value,
                    TimeSpan = title.Attribute("timeSpan").Value
                };
                titleList.Add(newTitle);
            }
            return titleList;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    public async Task<PaperDto> GetPaperDtoAsync(int paperId)
    {
        string _method = "GetPaperDtoAsync";
        try
        {
            var tl = await GetPaperDtosAsync();
            var title = tl.Where(t => t.Id == paperId).FirstOrDefault();
            return title;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    public async Task<Paragraph> GetParagraphAsync(int paperId, int seqId)
    {
        string _method = "GetParagraphAsync";
        try
        {
            if (!_initialized)
            {
                await InitializeDataAsync();
            }

            Paragraph newParagraph = new();
            var locationId = paperId + "." + seqId;

            var paragraphNotesList = Notes.Where(n => n.LocationId == locationId).ToList();

            string paperName = paperId.ToString("000") + ".xml";
            string filePathName = Path.Combine(_mauiUbml, paperName);

            string content = await LoadAssetAsync(_mauiUbml, paperName);
            var xdoc = XDocument.Parse(content);
            var root = xdoc.Root;
            var paragraphs = root.Descendants("Paragraph");
            var paragraph = paragraphs.Where(p => p.Attribute("seqId").Value == seqId.ToString("0"))
                                      .FirstOrDefault();

            var paraStyle = paragraph.Attribute("paraStyle").Value;
            var paraType = paragraph.Attribute("type").Value;
            var startTime = paragraph.Attribute("startTime").Value;
            var endTime = paragraph.Attribute("endTime").Value;

            StringBuilder sb = new StringBuilder();
            List<Run> paragraphRuns = new List<Run>();
            var runs = paragraph.Descendants("Run").ToList();
            foreach (var run in runs)
            {
                var runStyle = run.Attribute("Style").Value;
                string txt = run.Attribute("Text").Value;
                sb.Append(txt);
                var newRun = new Run
                {
                    Text = txt,
                    Style = runStyle,
                };
                paragraphRuns.Add(newRun);
            }

            var uid = paragraph.Attribute("uid").Value;
            var uidArr = uid.Split('.');
            var pid = Int32.Parse(uidArr[1]) +
                      ":" +
                      Int32.Parse(uidArr[2]) +
                      "." +
                      Int32.Parse(uidArr[3]);

            var paperSeqId = paperId + "." + seqId;

            newParagraph = new()
            {
                PaperId = paperId,
                SeqId = seqId,
                PaperSeqId = paperSeqId,
                Uid = uid,
                Pid = pid,
                Type = paraType,
                ParaStyle = paraStyle,
                StartTime = startTime,
                EndTime = endTime,
                Text = sb.ToString(),
                Runs = paragraphRuns,
                Notes = paragraphNotesList
            };
            newParagraph.Runs = paragraphRuns;
            return newParagraph;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    public async Task<Border> CreateNoteBorderAsync(NoteEntry note)
    {
        string _method = "CreateNoteBorderAsync";
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
    public async Task<List<NoteEntry>> LoadNotesAsync()
    {
        string _method = "LoadNotesAsync";
        try
        {
            var notes = new List<NoteEntry>();
            string contents = await LoadResourceContentAsync("Notes.xml");
            XDocument xdoc = XDocument.Parse(contents);
            XElement root = xdoc.Root;
            var noteElms = root.Descendants("Note");
            foreach (var note in noteElms)
            {
                var id = note.Attribute("id").Value;
                var type = note.Attribute("type").Value;
                var paperId = note.Attribute("paperId").Value;
                var sequenceId = note.Attribute("seqId").Value;
                var locationId = note.Attribute("locationId").Value;
                var pid = note.Attribute("pid").Value;
                var created = note.Attribute("created").Value;
                var edited = note.Attribute("edited").Value;
                var author = note.Element("Author").Value;
                var subject = note.Element("Subject").Value;
                var text = note.Element("Text").Value;

                var runsNode = note.Descendants("Runs").FirstOrDefault();

                var runs = runsNode.Descendants("Run");
                StringBuilder sb = new StringBuilder();
                NoteText noteText = new NoteText();
                noteText.Style = "RegularParagraph";
                foreach (var run in runs)
                {
                    var attribs = run.Attributes();


                    var style = run.Attribute("style").Value;
                    var runText = run.Value;
                    NoteRun newRun = new NoteRun() { Text = runText };
                    foreach (var att in attribs)
                    {
                        var name = att.Name.LocalName;
                        var value = att.Value;
                        if (name.Equals("style"))
                        {
                            newRun.Style = value;
                        }
                        if (name.Equals("textSize"))
                        {
                            newRun.TextSize = value;
                        }
                        if (name.Equals("fontFamily"))
                        {
                            newRun.FontFamily = value;
                        }
                        if (name.Equals("textColor"))
                        {
                            newRun.TextColor = value;
                        }
                        if (name.Equals("textTransform"))
                        {
                            newRun.TextTransform = value;
                        }
                    }

                    sb.Append(runText);
                    noteText.NoteRuns.Add(newRun);
                }
                noteText.Text = sb.ToString();

                var createdArry = created.Split("-");
                var editedArry = edited.Split("-");

                var newNoteEntry = new NoteEntry()
                {
                    Id = Int32.Parse(id),
                    Type = type,
                    PaperId = Int32.Parse(paperId),
                    SequenceId = Int32.Parse(sequenceId),
                    LocationId = locationId,
                    Pid = pid,
                    DateCreated = new DateTime(Int32.Parse(createdArry[0]),
                                               Int32.Parse(createdArry[1]),
                                               Int32.Parse(createdArry[2])),
                    DateEdited = new DateTime(Int32.Parse(editedArry[0]),
                                              Int32.Parse(editedArry[1]),
                                              Int32.Parse(editedArry[2])),
                    Author = author,
                    Subject = subject,
                    Text = text
                };
                notes.Add(newNoteEntry);
            }
            return notes;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    #endregion

    #region  Private Helper Methods
    private async Task<List<NoteRun>> MakeRunsAsync(XElement runs)
    {
        string _method = "MakeRunsAsync";
        try
        {
            List<NoteRun> newRuns = new();
            var _runs = runs.Descendants("Run");
            foreach (var _run in _runs)
            {
                // TODO: NotImplemented
                throw new NotImplementedException("Expecting all emtpty for now ..");
            }
            return newRuns;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task<DateTime> CreateDateTimeAsync(string date, string time)
    {
        string _method = "CreateDateTimeAsync";
        try
        {
            char[] delimiterChars = { '/', ':', '.' };
            var dateArr = date.Split(delimiterChars);
            var timeArr = time.ToString().Split(delimiterChars);
            var hour = Int32.Parse(timeArr[0]);
            var minute = Int32.Parse(timeArr[1]);
            var second = Int32.Parse(timeArr[2]);
            var day = Int32.Parse(dateArr[0]);
            var month = Int32.Parse(dateArr[1]);
            var year = Int32.Parse(dateArr[2]);
            DateTime dateTime = new DateTime(year, month, day, hour, minute, second);
            return dateTime;
        }
        catch (Exception ex)
        {
            var exceptionMessage = $"Exception raised in {_class}.{_method} => " + ex.Message;
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            throw new Exception(exceptionMessage);
        }
    }
    private async Task<string> LoadResourceContentAsync(string fileName)
    {
        string _method = " LoadResourceContentAsync";
        try
        {
            string _appDir = FileSystem.AppDataDirectory;
            string targetFilePath = Path.Combine(_appDir, fileName);
            using FileStream inputStream = File.OpenRead(targetFilePath);
            using StreamReader reader = new StreamReader(inputStream);
            string contents = reader.ReadToEnd();
            return contents;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    #endregion

    #region Private Load/Save File Helper Methods
    private async Task<bool> LoadContentAsync()
    {
        string _method = "LoadContentAsync";
        try
        {
            _appDir = FileSystem.AppDataDirectory;
            string targetFilePath = Path.Combine(_appDir, _notesFileName);
            using FileStream inputStream = File.OpenRead(targetFilePath);
            using StreamReader reader = new StreamReader(inputStream);
            _contents = reader.ReadToEnd();
            _notes = XDocument.Parse(_contents);
            _notesRoot = _notes.Root;
            _initialized = true;
            return _initialized;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return false;
        }
    }
    private async Task<string> LoadFileAsync(string fileName)
    {
        string _method = "LoadFileAsync";
        try
        {
            string targetFilePath = Path.Combine(FileSystem.Current.AppDataDirectory, fileName);
            using FileStream inputStream = File.OpenRead(targetFilePath);
            using StreamReader reader = new StreamReader(inputStream);
            string contents = reader.ReadToEnd();
            return contents;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task SaveFileAsync(string fileName)
    {
        string _method = "SaveFileAsync";
        try
        {
            string targetFilePath = Path.Combine(FileSystem.Current.AppDataDirectory, fileName);
            using var stream = File.Create(targetFilePath);
            CancellationToken token = new CancellationToken(false);
            if (_notesRoot != null)
            {
                await _notesRoot.SaveAsync(stream, SaveOptions.None, token);
            }
            else
            {
                throw new Exception("Initialization exception: Root was null.");
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }
    public async Task<string> LoadAssetAsync(string fileName)
    {
        string _method = "LoadAssetAsync(fileName)";
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
            using var reader = new StreamReader(stream);
            string contents = reader.ReadToEnd();
            return contents;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    public async Task<string> LoadAssetAsync(string rootPath, string fileName)
    {
        string _method = "LoadAssetAsync(rootPath, fileName)";
        try
        {
            string dataSource = Path.Combine(rootPath, fileName);
            string contents = await LoadAssetAsync(dataSource);
            return contents;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok"); await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }
    #endregion
}
