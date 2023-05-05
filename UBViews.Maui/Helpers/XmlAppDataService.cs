using System.Xml.Linq;
using UBViews.Models;
using UBViews.Models.Query;
using UBViews.Services;

namespace UBViews.Helpers;

public class XmlAppDataService : IAppDataService
{
    /// <summary>
    /// 
    /// </summary>
    IFileService fileService;

    public XmlAppDataService(IFileService fileService)
    {
        this.fileService = fileService;
    }
    public async Task<string> LoadAppDataAsync(string filename)
    {
        try
        {
            string appDir = FileSystem.Current.AppDataDirectory;
            string targetFile = System.IO.Path.Combine(appDir, filename);
            using FileStream inputStream = System.IO.File.OpenRead(targetFile);
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
    public async Task SaveAppDataAsync(string filename, string content)
    {
        try
        {
            string targetFile = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, filename);
            using var stream = System.IO.File.OpenWrite(targetFile);
            using StreamWriter writer = new StreamWriter(stream);
            await writer.WriteAsync(content);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
        }
    }
    public async Task<List<AppFile>> GetAppFilesAsync()
    {
        try
        {
            string mainDir = FileSystem.Current.AppDataDirectory;
            string[] files = Directory.GetFiles(mainDir);

            List<AppFile> appFiles = new List<AppFile>();

            int count = 0;
            foreach (string file in files)
            {
                var fileName = file.Substring(mainDir.Length + 1);
                var newFile = new AppFile { Id = ++count, FilePath = file, FileName = fileName };
                appFiles.Add(newFile);
            }

            return appFiles;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }
    public async Task<(bool, int)> QueryResultExistsAsync(string query)
    {
        XDocument xdoc = null;
        bool queryExists = false;
        int queryId = 0;
        try
        {

            var qryCmds = await LoadAppDataAsync("QueryCommands.xml");
            xdoc = XDocument.Parse(qryCmds);
            var root = xdoc.Root;
            var qryCmd = root.Descendants("QueryString").Where(e => e.Value == query)
                                                        .FirstOrDefault();
            var qryResult = qryCmd == null ? null : qryCmd.Parent;
            if (qryResult != null)
            {
                queryId = Int32.Parse(qryResult.Attribute("id").Value);
                queryExists = true;
            }
            return (queryExists, queryId);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in QueryInputViewModel.LoadQueryResult => ",
                ex.Message, "Ok");
            return (false, -1);
        }
        finally
        {
            xdoc = null;
        }
    }
    public async Task<QueryResult> GetQueryResultByIdAsync(int queryId)
    {
        XDocument xdoc = null;
        QueryResult queryResultDto = null;
        try
        {
            var queryHistory = await LoadAppDataAsync("QueryHistory.xml");
            xdoc = XDocument.Parse(queryHistory);
            var root = xdoc.Root;
            var qryId = queryId.ToString("0");
            var queryResult = root.Descendants("QueryResult").Where(e => e.Attribute("id").Value == qryId)
                                                             .FirstOrDefault();
            if (queryResult != null)
            {
                // Load Dto mind and ship filterby parid
                string id = queryResult.Attribute("id").Value;
                string type = queryResult.Attribute("type").Value;
                string terms = queryResult.Attribute("terms").Value;
                string proximity = queryResult.Attribute("proximity").Value;
                string filterId = queryResult.Attribute("filterId").Value;

                XElement queryStringElm = queryResult.Descendants("QueryString").FirstOrDefault();
                string queryString = queryStringElm.Value;
                XElement queryExpressionElm = queryResult.Descendants("QueryExpression").FirstOrDefault();
                string queryExpression = queryExpressionElm.Value;
                XElement queryLocationsElm = queryResult.Descendants("QueryLocations").FirstOrDefault();

                queryResultDto = new QueryResult()
                {
                    Id = Int32.Parse(id),
                    Type = type,
                    Terms = terms,
                    Proximity = proximity,
                    QueryString = queryString,
                    QueryExpression = queryExpression,
                    QueryLocations = new List<QueryLocation>()
                };

                var locs = queryLocationsElm.Descendants("QueryLocation");
                QueryLocation qlocDto;
                foreach (XElement loc in locs)
                {
                    var locId = loc.Attribute("id").Value;
                    var pid = loc.Attribute("pid").Value;
                    qlocDto = new QueryLocation()
                    {
                        Id = locId,
                        Pid = pid,
                        TermOccurrences = new List<TermOccurence>()
                    };
                    queryResultDto.QueryLocations.Add(qlocDto);

                    var occLst = loc.Descendants("TermOccurrence");
                    TermOccurence occDto;
                    foreach (XElement occ in occLst)
                    {
                        var term = occ.Attribute("term").Value;
                        var docId = occ.Attribute("docId").Value;
                        var seqId = occ.Attribute("seqId").Value;
                        var dpoId = occ.Attribute("dpoId").Value;
                        var tpoId = occ.Attribute("tpoId").Value;
                        var len = occ.Attribute("len").Value;
                        occDto = new TermOccurence()
                        {
                            Term = term,
                            DocId = Int32.Parse(docId),
                            SeqId = Int32.Parse(seqId),
                            DpoId = Int32.Parse(dpoId),
                            TpoId = Int32.Parse(tpoId),
                            Len = Int32.Parse(len)
                        };
                        qlocDto.TermOccurrences.Add(occDto);
                    }
                }
            }
            return queryResultDto;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in QueryInputViewModel.LoadQueryResult => ",
                ex.Message, "Ok");
            return null;
        }
        finally
        {
            xdoc = null;
            queryResultDto = null;
        }
    }
    public async Task<QueryResult> GetQueryResultAsync(string query)
    {
        XDocument xdoc = null;
        QueryResult queryResultDto = null;

        try
        {
            var userQueries = await LoadAppDataAsync("QueryHistory.xml");
            xdoc = XDocument.Parse(userQueries);
            var root = xdoc.Root;
            var qry = root.Descendants("QueryString").Where(e => e.Value == query)
                                                     .FirstOrDefault();
            var queryResult = qry == null ? null : qry.Parent;
            if (queryResult != null)
            {
                // Load Dto mind and ship filterby parid
                string id = queryResult.Attribute("id").Value;
                string type = queryResult.Attribute("type").Value;
                string terms = queryResult.Attribute("terms").Value;
                string proximity = queryResult.Attribute("proximity").Value;
                string filterId = queryResult.Attribute("filterId").Value;

                XElement queryStringElm = queryResult.Descendants("QueryString").FirstOrDefault();
                string queryString = queryStringElm.Value;
                XElement queryExpressionElm = queryResult.Descendants("QueryExpression").FirstOrDefault();
                string queryExpression = queryExpressionElm.Value;
                XElement queryLocationsElm = queryResult.Descendants("QueryLocations").FirstOrDefault();

                queryResultDto = new QueryResult()
                {
                    Id = Int32.Parse(id),
                    Type = type,
                    Terms = terms,
                    Proximity = proximity,
                    QueryString = queryString,
                    QueryExpression = queryExpression,
                    QueryLocations = new List<QueryLocation>()
                };

                var locs = queryLocationsElm.Descendants("QueryLocation");
                QueryLocation qlocDto;
                foreach (XElement loc in locs)
                {
                    var locId = loc.Attribute("id").Value;
                    var pid = loc.Attribute("pid").Value;
                    qlocDto = new QueryLocation()
                    {
                        Id = locId,
                        Pid = pid,
                        TermOccurrences = new List<TermOccurence>()
                    };
                    queryResultDto.QueryLocations.Add(qlocDto);

                    var occLst = loc.Descendants("TermOccurrence");
                    TermOccurence occDto;
                    foreach (XElement occ in occLst)
                    {
                        var term = occ.Attribute("term").Value;
                        var docId = occ.Attribute("docId").Value;
                        var seqId = occ.Attribute("seqId").Value;
                        var dpoId = occ.Attribute("dpoId").Value;
                        var tpoId = occ.Attribute("tpoId").Value;
                        var len = occ.Attribute("len").Value;
                        occDto = new TermOccurence()
                        {
                            Term = term,
                            DocId = Int32.Parse(docId),
                            SeqId = Int32.Parse(seqId),
                            DpoId = Int32.Parse(dpoId),
                            TpoId = Int32.Parse(tpoId),
                            Len = Int32.Parse(len)
                        };
                        qlocDto.TermOccurrences.Add(occDto);
                    }
                }
            }
            return queryResultDto;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in QueryInputViewModel.LoadQueryResult => ",
                ex.Message, "Ok");
            return null;
        }
        finally
        {
            xdoc = null;
            queryResultDto = null;
        }
    }
    public async Task<List<QueryCommand>> GetQueryCommandsAsync()
    {
        XDocument xdoc = null;
        List<QueryCommand> queryCommands = null; ;

        try
        {
            var queryHistory = await LoadAppDataAsync("QueryCommands.xml");
            xdoc = XDocument.Parse(queryHistory);
            var root = xdoc.Root;
            var qryCmds = root.Descendants("QueryCmd");
            queryCommands = new List<QueryCommand>();
            foreach (var cmd in qryCmds)
            {
                var id = Int32.Parse(cmd.Attribute("id").Value);
                var type = cmd.Attribute("type").Value;
                var query = cmd.Descendants("QueryString").FirstOrDefault().Value;
                QueryCommand dto = new QueryCommand()
                {
                    Id = id,
                    Type = type,
                    Query = query
                };
                queryCommands.Add(dto);
            }
            return queryCommands;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in QueryInputViewModel.LoadQueryResult => ",
                ex.Message, "Ok");
            return null;
        }
        finally
        {
            xdoc = null;
            queryCommands = null;
        }
    }
}
