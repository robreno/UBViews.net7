using System.Xml.Linq;
using System.Xml.XPath;
using System.Text;
using System.Text.Json;

using UBViews.Services;
using UBViews.Models;
using UBViews.Models.Ubml;

namespace UBViews.Helpers;

public class FileService : IFileService
{
    /// <summary>
    /// Private paths
    /// </summary>
    private const string _corpus = "Corpus";
    private const string _query = "Query";
    private const string _xmlData = "XmlData";
    private const string _jsonData = "JsonData";
    private const string _settings = "Settings";
    private const string _mauiUbml = "MauiUbml";

    /// <summary>
    /// CStor
    /// </summary>
    public FileService() { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePathName"></param>
    /// <returns></returns>
    public async Task<string>LoadAsset(string filePathName)
    {
        try 
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(filePathName);
            using var reader = new StreamReader(stream);
            string contents = reader.ReadToEnd();
            return contents;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rootPath"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public async Task<string> LoadAsset(string rootPath, string fileName)
    {
        try
        {
            string dataSource = Path.Combine(rootPath, fileName);
            string contents = await LoadAsset(dataSource);
            return contents;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="filePathName"></param>
    /// <returns></returns>
    public async Task<List<T>> LoadAsset_JsonSerializer<T>(string filePathName)
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(filePathName);
            using var reader = new StreamReader(stream);
            string contents = reader.ReadToEnd();
            var parts = JsonSerializer.Deserialize<List<T>>(contents);
            return parts;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<List<PaperDto>> GetPaperDtosAsync()
    {
        try
        {
            List<PaperDto> titleList = new();

            string xml = await LoadAsset(_xmlData, "PaperTitles.xml");
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
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paperId"></param>
    /// <returns></returns>
    public async Task<PaperDto> GetPaperDtoAsync(int paperId)
    {
        try
        {
            var tl = await GetPaperDtosAsync();
            var title = tl.Where(t => t.Id == paperId).FirstOrDefault();
            return title;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ContentDto> GetContentDtoAsync(int id)
    {
        try
        {
            var fileName = id.ToString("000") + ".xml";

            ContentDto dto = new ContentDto();
            var content = await LoadAsset(_mauiUbml, fileName);
            XDocument xDoc = XDocument.Parse(content);
            XElement root = xDoc.Root;
            var paragraphs = root.Descendants("Paragraph").Where(e => e.Attribute("type").Value == "section");
            int count = paragraphs.Count();

            dto.Id = Int32.Parse(paragraphs.ElementAt(0).Parent.Attribute("paperId").Value);
            dto.Title = paragraphs.ElementAt(0).Parent.Attribute("paperTitle").Value;
            SectionTitleDto titleDto;
            foreach (var paragraph in paragraphs)
            {
                var uid = paragraph.Attribute("uid").Value;
                var run = paragraph.XPathSelectElement("Run");
                var text = run.Attribute("Text").Value;
                var seqId = paragraph.Attribute("seqId").Value;
                if (seqId == "1")
                {
                    titleDto = new SectionTitleDto() { Uid = uid, Prefix = "0", SectionTitle = text };
                }
                else
                {
                    var _prefix = text.Substring(0, 1);
                    var _sectionTitle = text.Substring(2, text.Length - 2).Trim();
                    titleDto = new SectionTitleDto() { Uid = uid, Prefix = _prefix, SectionTitle = _sectionTitle };
                }
                dto.SectionTitles.Add(titleDto);
            }
            return dto;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paperId"></param>
    /// <returns></returns>
    public async Task<List<Paragraph>> GetParagraphsAsync(int paperId)
    {
        try
        {
            List<Paragraph> paragraphList = new();

            string paperName = paperId.ToString("000") + ".xml";
            string filePathName = Path.Combine(_mauiUbml, paperName);
            string xml = await LoadAsset(filePathName);

            var xdoc = XDocument.Parse(xml);
            var root = xdoc.Root;
            var paragraphs = root.Descendants("Paragraph");
            foreach (var paragraph in paragraphs)
            {
                var startTime = paragraph.Attribute("startTime").Value;
                var endTime = paragraph.Attribute("endTime").Value;
                var seqId = Int32.Parse(paragraph.Attribute("seqId").Value);
                var paraType = paragraph.Attribute("type").Value;
                var paraStyle = paragraph.Attribute("paraStyle").Value;
                bool skipAstrisks = (paraStyle == "SponsorAstrisks") ||
                                    (paraStyle == "PoemAstrisks") ||
                                    (paraStyle == "AstrisksParagraph");
                if (skipAstrisks)
                    continue;

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

                Paragraph newParagraph = new()
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
                    Runs = paragraphRuns
                };
                newParagraph.Runs = paragraphRuns;
                paragraphList.Add(newParagraph);
            }
            return paragraphList;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paperId"></param>
    /// <param name="seqId"></param>
    /// <returns></returns>
    public async Task<Paragraph> GetParagraphAsync(int paperId, int seqId)
    {
        try
        {
            string paperName = paperId.ToString("000") + ".xml";
            string filePathName = Path.Combine(_mauiUbml, paperName);
            string xml = await LoadAsset(filePathName);

            var xdoc = XDocument.Parse(xml);
            var root = xdoc.Root;
            var paragraphs = root.Descendants("Paragraph");
            var paragraph = paragraphs.Where(p => p.Attribute("seqId").Value == seqId.ToString("0")).FirstOrDefault();

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

            Paragraph newParagraph = new()
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
                Runs = paragraphRuns
            };
            newParagraph.Runs = paragraphRuns;
            return newParagraph;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }
}
