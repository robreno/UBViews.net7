namespace UBViews.Helpers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using UBViews.Models.Notes;
//using UBViews.Models.Ubml;
using UBViews.Services;

public partial class NoteService : INoteService
{
    /// <summary>
    /// CultureInfo
    /// </summary>
    public CultureInfo cultureInfo;

    /// <summary>
    /// ContentPage
    /// </summary>
    public ContentPage contentPage;

    IFileService fileService;

    readonly string _class = nameof(NoteService);

    public NoteService(IFileService fileService)
    {
        this.fileService = fileService;
    }
    public async Task<NoteLocationsDto> GetNoteLocationsDtoAsync()
    {
        string _method = nameof(GetNoteLocationsDtoAsync);
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
                newNoteEntry.NoteEntries.Add(noteText);
                dto.NoteLocations.Add(newNoteEntry);
            }
            return dto;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }

    public async Task<Border> CreateNoteBorderAsync(NoteEntry note)
    {
        string _method = nameof(CreateNoteBorderAsync);
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
        string _method = nameof(LoadNotesAsync);
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
                newNoteEntry.NoteEntries.Add(noteText);
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

    private async Task<string> LoadResourceContentAsync(string fileName)
    {
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
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }
}
