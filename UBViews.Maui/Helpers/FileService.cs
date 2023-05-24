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
    public async Task<List<T>> LoadAsset<T>(string filePathName)
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
                    var arry = text.Split('.');
                    var prefix = arry[0].Trim();
                    var sectionTitle = arry[1].Trim();
                    titleDto = new SectionTitleDto() { Uid = uid, Prefix = prefix, SectionTitle = sectionTitle };
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
                var seqId = Int32.Parse(paragraph.Attribute("seqId").Value);
                var style = paragraph.Attribute("paraStyle").Value;
                bool skipAstrisks = (style == "SponsorAstrisks") ||
                                    (style == "PoemAstrisks") ||
                                    (style == "AstrisksParagraph");
                if (skipAstrisks)
                    continue;

                StringBuilder sb = new StringBuilder();
                var paraType = paragraph.Attribute("type").Value;
                if (paraType == "section")
                {
                    var runs = paragraph.Descendants("Run").ToList();
                    foreach (var run in runs)
                    {
                        string txt = run.Attribute("Text").Value;
                        sb.Append(txt);
                    }
                }
                else
                {
                    var runs = paragraph.Descendants("Run").ToList();
                    foreach (var run in runs)
                    {
                        string txt = run.Attribute("Text").Value;
                        sb.Append(txt);
                    }
                }

                var uid = paragraph.Attribute("uid").Value;
                var uidArr = uid.Split('.');
                var pid = Int32.Parse(uidArr[1]) +
                          ":" +
                          Int32.Parse(uidArr[2]) +
                          "." +
                          Int32.Parse(uidArr[3]);

                Paragraph newParagraph = new()
                {
                    Id = paperId,
                    SeqId = seqId,
                    Uid = uid,
                    Pid = pid,
                    Type = paragraph.Attribute("type").Value,
                    ParaStyle = style,
                    StartTime = paragraph.Attribute("startTime").Value,
                    EndTime = paragraph.Attribute("endTime").Value,
                    Text = sb.ToString()
                };
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
            var para = paragraphs.Where(p => p.Attribute("seqId").Value == seqId.ToString("0")).FirstOrDefault();

            var style = para.Attribute("paraStyle").Value;
            StringBuilder sb = new StringBuilder();
            var paraType = para.Attribute("type").Value;

            if (paraType == "section")
            {
                var runs = para.Descendants("Run").ToList();
                foreach (var run in runs)
                {
                    string txt = run.Attribute("Text").Value;
                    sb.Append(txt);
                }
            }
            else
            {
                var runs = para.Descendants("Run").ToList();
                foreach (var run in runs)
                {
                    string txt = run.Attribute("Text").Value;
                    sb.Append(txt);
                }
            }

            var uid = para.Attribute("uid").Value;
            var uidArr = uid.Split('.');
            var pid = Int32.Parse(uidArr[1]) +
                      ":" +
                      Int32.Parse(uidArr[2]) +
                      "." +
                      Int32.Parse(uidArr[3]);

            Paragraph paragraph = new()
            {
                Id = paperId,
                SeqId = seqId,
                Uid = uid,
                Pid = pid,
                Type = para.Attribute("type").Value,
                ParaStyle = style,
                StartTime = para.Attribute("startTime").Value,
                EndTime = para.Attribute("endTime").Value,
                Text = sb.ToString()
            };

            return paragraph;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }
}
