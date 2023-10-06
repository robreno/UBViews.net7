using Microsoft.FSharp.Core;
using CommunityToolkit.Maui.Converters;
using System.Xml.Linq;
using System.Linq.Expressions;
using Microsoft.FSharp.Collections;

using QueryEngine;
using static QueryEngine.QueryRepository;
using static QueryEngine.PostingRepository;
using static QueryEngine.Models;
using static QueryEngine.SimpleEnumeratorsEx;
using static UBViews.Query.Ast;

using UBViews.Models.Query;
using UBViews.Services;

namespace UBViews.Helpers
{
    public class FSRepositoryService : IFSRepositoryService
    {
        #region Private Members
        // C:\Users\robre\AppData\Local\Packages\UBViews_1s7hth42e283a\LocalState
        private static string _queriesDbPathLocalState = Path.Combine(FileSystem.AppDataDirectory, "queryResults.db3");
        private static string _queriesDbPathLocalCache =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "queryResults.db3");
        //private static bool _queryDbExists = false;

        private static string _postingDbPathLocalState = Path.Combine(FileSystem.AppDataDirectory, "postingLists.db3");
        private static string _postingDbPathLocalCache =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "postingLists.db3");
        //private static bool _postingDbExists = false;

        //private string queryDB_Path = _queriesDbPathLocalState;
        //private string postingDB_Path = _postingDbPathLocalState;

        private string queryDB_Path = string.Empty;
        private string postingDB_Path = string.Empty;
        QueryService _queryService;
        #endregion

        public FSRepositoryService() 
        {
            queryDB_Path = Preferences.Default.Get("QueryDBPath", "DATABASE_PATH_NOT_SET");
            postingDB_Path = Preferences.Default.Get("PostingDBPath", "DATABASE_PATH_NOT_SET");
            _queryService = new QueryService();
        }

        #region Database Initialization
        public async Task InititializeDatabase(IFSRepositoryService.InitOptions options)
        {
            try
            {
                switch (options)
                {
                    case IFSRepositoryService.InitOptions.QueryDB:
                        //QueryEngine.QueryRepository.connect(_queriesDbPathLocalState);
                        break;
                    case IFSRepositoryService.InitOptions.PostingDB:
                        //QueryEngine.PostingRepository.connect(_postingDbPathLocalState);
                        break;
                    default:
                        //QueryEngine.QueryRepository.connect(_queriesDbPathLocalState);
                        //QueryEngine.PostingRepository.connect(_postingDbPathLocalState);
                        break;
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            }
        }
        #endregion

        #region Database Repository Methods
        /// <summary>
        /// SaveQueryResultAsync
        /// </summary>
        /// <param name="queryResult"></param>
        /// <returns></returns>
        public async Task<int> SaveQueryResultAsync(QueryResult queryResult)
        {
            string methodName = "SaveQueryResultAsync";
            try

            {
                QueryResultObject obj = new QueryResultObject(queryResult.Id,
                                                          queryResult.Hits,
                                                          queryResult.Type,
                                                          queryResult.Terms,
                                                          queryResult.Proximity,
                                                          queryResult.QueryString,
                                                          queryResult.ReverseQueryString,
                                                          queryResult.QueryExpression);

                var result = await QueryEngine.QueryRepository.insertQueryResultAsync(_queriesDbPathLocalState, obj);
                if (result != null)
                { return 1; }
                else
                { return 0; }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return 0;
            }
        }

        /// <summary>
        /// SaveQueryResultAsync
        /// </summary>
        /// <param name="queryResultDto"></param>
        /// <returns>QueryResultObject rowId.</returns>
        public async Task<int> SaveQueryResultAsync(QueryResultDto queryResultDto)
        {
            string methodName = "SaveQueryResultAsync";
            try
            {
                var obj = await MapQueryResultDtoTotObj(queryResultDto);
                var result = await QueryRepository.saveQueryResultAsync(_queriesDbPathLocalState, obj);
                var id = result.Id;
                return id;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryResultElm"></param>
        /// <returns></returns>
        public async Task<int> SaveQueryResultAsync(XElement queryResultElm)
        {
            string methodName = "SaveQueryResultLocationsAsync";
            try
            {
                var locationCount = queryResultElm.Attribute("locationCount").Value;
                var queryStringElm = queryResultElm.Descendants("QueryString").FirstOrDefault();
                var reverseQueryStringElm = queryResultElm.Descendants("ReverseQueryString").FirstOrDefault();
                var queryExpressionElm = queryResultElm.Descendants("QueryExpression").FirstOrDefault();

                var queryType = queryResultElm.Attribute("type").Value;
                var queryProximity = queryResultElm.Attribute("proximity").Value;
                var queryTerms = queryResultElm.Attribute("terms").Value;
                var queryString = queryStringElm.Value;
                var queryExpression = queryExpressionElm.Value;

                var queryLocations = queryResultElm.Descendants("QueryLocation");
                var queryLocationsCount = queryLocations.Count();

                var reverseQueryString = string.Empty;
                if (queryType == "FilterBy+And" || 
                    queryType == "And" && 
                    reverseQueryStringElm != null) 
                {
                    reverseQueryString = reverseQueryStringElm.Value;
                }
                
                var queryResultDto = new QueryResultDto
                {
                    Id = 0,
                    Hits = queryLocationsCount,
                    Type = queryType,
                    Terms = queryTerms,
                    Proximity = queryProximity,
                    QueryString = queryString,
                    ReverseQueryString = reverseQueryString,
                    QueryExpression = queryExpression
                };

                var qrObj = await MapQueryResultDtoTotObj(queryResultDto);
                var qrReturn = await QueryRepository.saveQueryResultAsync(_queriesDbPathLocalState, qrObj);
                int queryResultId = qrReturn.Id;

                foreach (var queryLocation in queryLocations) 
                {
                    var id = queryLocation.Attribute("id").Value;
                    var pid = queryLocation.Attribute("pid").Value;
                    char[] delims = { ':', '.' };
                    var pidArry = pid.Split(delims, StringSplitOptions.RemoveEmptyEntries);
                    var termOccurrences = queryLocation.Descendants("TermOccurrence");


                    var sectionId = Int32.Parse(pidArry[1]);
                    var termOccs = queryLocation.Descendants("TermOccurrenceList").FirstOrDefault();
                    var termOccsCount = termOccs.Attribute("count").Value;
                    var occs = termOccs.Descendants("TermOccurrence");

                    List<TermOccurrenceDto> termOccDtoLst = new List<TermOccurrenceDto>();
                    foreach (var termOccurrence in termOccurrences) 
                    {
                        var term = termOccurrence.Attribute("term").Value;
                        var docId = Int32.Parse(termOccurrence.Attribute("docId").Value);
                        var seqId = Int32.Parse(termOccurrence.Attribute("seqId").Value);
                        var dpoId = Int32.Parse(termOccurrence.Attribute("dpoId").Value);
                        var tpoId = Int32.Parse(termOccurrence.Attribute("tpoId").Value);
                        var len = Int32.Parse(termOccurrence.Attribute("len").Value);

                        TermOccurrenceDto termOccurrenceDto = new TermOccurrenceDto()
                        {
                            QueryResultId = queryResultId,
                            DocumentId = docId,
                            SequenceId = seqId,
                            DocumentPosition = dpoId,
                            TextPosition = tpoId,
                            TextLength = len,
                            ParagraphId = pid,
                            Term = term
                        };

                        var tocObj = await MapTermOccurrenceDtoTotObj(termOccurrenceDto);
                        var tocReturn = await QueryRepository.saveTermOccurrenceAsync(_queriesDbPathLocalState, tocObj);
                    }
                }

                return queryResultId;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return 0;
            }
        }

        /// <summary>
        /// QueryResultExistsAsync
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns>(bool true or false, QueryResultDto or null)</returns>
        public async Task<(bool, QueryResultDto)> QueryResultExistsAsync(string queryString)
        {
            string methodName = "QueryResultExistsAsync";
            try
            {
                var obj = await QueryRepository.getQueryResultByStringAsync(queryDB_Path, queryString);
                if (obj != null)
                {
                    var qr = obj.Value;
                    var dto = new QueryResultDto
                    {
                        Id = qr.Id,
                        Hits = qr.Hits,
                        Type = qr.Type,
                        Terms = qr.Terms,
                        Proximity = qr.Proximity,
                        QueryString = qr.QueryString,
                        ReverseQueryString = qr.ReverseQueryString,
                        QueryExpression = qr.QueryExpression
                    };
                    return (true, dto);
                }
                else
                {
                    return (false, null);
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return (false, null);
            }
        }

        /// <summary>
        /// GetQueryResultsAsync
        /// </summary>
        /// <returns>List of QueryResultDto</returns>
        public async Task<List<QueryResultDto>> GetQueryResultsAsync()
        {
            string methodName = "GetQueryResultsAsync";
            try
            {
                var queryResultList = await QueryEngine.QueryRepository.getQueryResultsAsync(queryDB_Path);
                List<QueryResultDto> queryResultListDto = new List<QueryResultDto>();
                foreach (var qr in queryResultList) 
                {
                    var dto = await MapQueryResultObjTotDto(qr);
                    queryResultListDto.Add(dto);
                }
                return queryResultListDto;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }

        /// <summary>
        /// GetQueryResultByQueryStringAsync
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns>QueryResultLocationsDto</returns>
        public async Task<QueryResultLocationsDto> GetQueryResultByStringAsync(string queryString)
        {
            string methodName = "GetQueryResultByQueryStringAsync";
            try
            {
                QueryResultLocationsDto queryResultLocationsDto = null;
                (bool qryExists, QueryResultDto dto) = await QueryResultExistsAsync(queryString);
                var queryResult = await QueryEngine.QueryRepository.getQueryResultByStringAsync(queryDB_Path, queryString);
                if (queryResult != null)
                {
                    queryResultLocationsDto = await MapQueryResultObjToDto(queryResult.Value);
                }
                return queryResultLocationsDto;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }

        /// <summary>
        /// GetQueryResultByIdAsync
        /// </summary>
        /// <param name="id"></param>
        /// <returns>QueryResultLocationsDto</returns>
        public async Task<QueryResultLocationsDto> GetQueryResultByIdAsync(int id)
        {
            string methodName = "GetQueryResultByQueryIdAsync";
            try
            {
                QueryResultLocationsDto queryResultLocations = null;
                var queryResult = await QueryEngine.QueryRepository.getQueryResultByIdAsync(queryDB_Path, id);
                if (queryResult != null)
                {
                    queryResultLocations = await MapQueryResultObjToDto(queryResult.Value);
                }
                return queryResultLocations;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }

        /// <summary>
        /// GetQueryCommandsAsync
        /// </summary>
        /// <returns>List of QueryCommandDto</returns>
        public async Task<List<QueryCommandDto>> GetQueryCommandsAsync()
        {
            try
            {
                List<QueryCommandDto> queryCommands = new List<QueryCommandDto>();
                var queryResults = await QueryEngine.QueryRepository.getQueryResultsAsync(queryDB_Path);
                foreach (var obj in queryResults)
                {
                    var dto = new QueryCommandDto
                    {
                        Id = obj.Id,
                        Hits = obj.Hits,
                        Type = obj.Type,
                        Terms = obj.Terms,
                        Proximity = obj.Proximity,
                        QueryString = obj.QueryString,
                        QueryExpression = obj.QueryExpression
                    };
                    queryCommands.Add(dto);
                }
                return queryCommands;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
                return null;
            }
        }

        /// <summary>
        /// GetPostingsAsync
        /// </summary>
        /// <returns>List of PostingList</returns>
        public async Task<List<PostingListDto>> GetPostingListsAsync()
        {
            string methodName = "GetPostingsAsync";
            try
            {
                var postingLists = await QueryEngine.PostingRepository.getPostingListsAsync(postingDB_Path);
                List<PostingListDto> newPostingLists = new List<PostingListDto>();
                foreach (var postingList in postingLists)
                {
                    var dto = await MapPostingListObjToDto(postingList);
                    newPostingLists.Add(dto);
                }
                return newPostingLists;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }

        /// <summary>
        /// GetPostingListByLexemeAsync
        /// </summary>
        /// <param name="lexeme"></param>
        /// <returns>PostingList</returns>
        public async Task<PostingListDto> GetPostingListByLexemeAsync(string lexeme)
        {
            string methodName = "GetPostingListByLexemeAsync";
            try
            {
                PostingListDto postingListDto = null;
                var result = await PostingRepository.getPostingListByLexemeAsync(postingDB_Path, lexeme);
                var opt = OptionModule.IsSome(result);
                if (opt)
                {
                    postingListDto = await MapPostingListObjToDto(result.Value);
                }
                return postingListDto;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }

        /// <summary>
        /// GetTermOccurrencesAsync
        /// </summary>
        /// <returns>List of TermOccurrenceDto</returns>
        public async Task<List<TermOccurrenceDto>> GetTermOccurrencesAsync()
        {
            string methodName = "GetTermOccurrencesAsync";
            try
            {
                List<TermOccurrenceDto> dtoList = new List<TermOccurrenceDto>();
                var termOccurrences = await QueryEngine.QueryRepository.getTermOccurrencesAsync(queryDB_Path);
                var isSome = OptionModule.IsSome(termOccurrences);
                if (isSome)
                {
                    var termOccs = termOccurrences.Value;
                    foreach (var termOccurrence in termOccs)
                    {
                        var dto = await MapTermOccurrenceObjToDto(termOccurrence);
                        dtoList.Add(dto);
                    }
                }
                return dtoList;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }

        /// <summary>
        /// GetTermOccurrencesByQueryResultIdAsync
        /// </summary>
        /// <param name="id"></param>
        /// <returns>List of TermOccurrence</returns>
        public async Task<List<TermOccurrenceDto>> GetTermOccurrencesByQueryResultIdAsync(int id)
        {
            string methodName = "GetTermOccurrencesByQueryResultIdAsync";
            try
            {
                List<TermOccurrenceDto> dtoList = new List<TermOccurrenceDto>();
                var termOccurrences = await QueryRepository.getTermOccurrencesByQueryResultIdAsync(queryDB_Path, id);
                var isSome = OptionModule.IsSome(termOccurrences);
                if (isSome) 
                {
                    var termOccs = termOccurrences.Value;
                    foreach (var termOccurrence in termOccs)
                    {
                        var dto = await MapTermOccurrenceObjToDto(termOccurrence);
                        dtoList.Add(dto);
                    }
                }
                return dtoList;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }

        /// <summary>
        /// GetTokenOccurrencesByPostingListIdAsync
        /// </summary>
        /// <param name="postingId"></param>
        /// <returns></returns>
        public async Task<List<TokenOccurrenceDto>> GetTokenOccurrencesByPostingListIdAsync(int postingId)
        {
            string methodName = "GetTokenOccurrencesByPostingListIdAsync";
            try
            {
                List<TokenOccurrenceDto> dtoList = new List<TokenOccurrenceDto>();
                var tokenOccs = await PostingRepository.getTokenOccurrencesByPostingListIdAsync(postingDB_Path, postingId);
                var opt = OptionModule.IsSome(tokenOccs);
                if (opt)
                { 
                    foreach (var tokenOccurrence in tokenOccs.Value)
                    {
                        var dto = await MapTokenOccurrenceObjToDto(tokenOccurrence);
                        dtoList.Add(dto);
                    }
                }
                return dtoList;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }

        /// <summary>
        /// GetTokenStemsAsync
        /// </summary>
        /// <returns>TokenStemDto List</returns>
        public async Task<List<TokenStemDto>> GetTokenStemsAsync()
        {
            string methodName = "GetTokenStemsAsync";
            try
            {
                List<TokenStemDto> dtoList = null; ;
                var tokenStems = await PostingRepository.getTokenStemsAsync(postingDB_Path);
                foreach (var tokenStem in tokenStems)
                {
                    var dto = await MapTokenStemObjToDto(tokenStem);
                    dtoList.Add(dto);
                }
                return dtoList;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }

        /// <summary>
        /// GetTokenStemAsync
        /// </summary>
        /// <param name="lexeme"></param>
        /// <returns>TokenStem</returns>
        public async Task<TokenStemDto> GetTokenStemAsync(string lexeme)
        {
            string methodName = "GetTokenStemAsync";
            try
            {
                TokenStemDto dto = null;
                var tokenStem = await PostingRepository.getTokenStemAsync(postingDB_Path, lexeme);
                var opt = OptionModule.IsSome(tokenStem);
                if (opt)
                {
                    dto = await MapTokenStemObjToDto(tokenStem.Value);
                }
                return dto;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }

        /// <summary>
        /// GetPostingListsByStemAsync
        /// </summary>
        /// <param name="stem"></param>
        /// <returns></returns>
        public async Task<List<PostingListDto>> GetPostingListsByStemAsync(string stem)
        {
            string methodName = "GetPostingListsByStemAsync";
            try
            {
                var postingLists = await QueryEngine.PostingRepository.getPostingListsByStemAsync(postingDB_Path, stem);
                List<PostingListDto> list = new List<PostingListDto>();
                var isSome = OptionModule.IsSome(postingLists);
                if (isSome) 
                {
                    var postingListObjs = postingLists.Value;
                    foreach (var postingList in postingListObjs)
                    {
                        var dto = await MapPostingListObjToDto(postingList);
                        list.Add(dto);
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }

        /// <summary>
        /// GetTokenOccurrencesByPostingListIdAsync
        /// </summary>
        /// <param name="stem"></param>
        /// <returns></returns>
        public async Task<List<TokenOccurrenceDto>> GetTokenOccurrencesByStemAsync(string stem)
        {
            string methodName = "GetTokenOccurrencesByStemAsync";
            try
            {
                var postingLists = await GetPostingListsByStemAsync(stem);
                List<TokenOccurrenceDto> list = new List<TokenOccurrenceDto>();
                foreach (var postingList in postingLists)
                {
                    var postingListId = postingList.Id;
                    var tokenOccurrences = await GetTokenOccurrencesByPostingListIdAsync(postingListId);
                    foreach (var occurrence in tokenOccurrences)
                    {
                        list.Add(occurrence);
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }
        #endregion

        #region Database Helper Methods
        /// <summary>
        /// RunQuery
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public async Task<TokenPostingList> RunQueryAsync(string queryString)
        {
            string methodName = "RunQueryAsync";
            try
            {
                var query = await LexParser.LexParser.parseQueryStringAsync(queryString);
                var retval = _queryService.RunQuery(query.Head);
                return retval;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="query"></param>
        /// <param name="tpl"></param>
        /// <returns></returns>
        public async Task<XElement> ProcessTokenPostingListAsync(string queryString, UBViews.Query.Ast.Query query, TokenPostingList tpl)
        {
            string methodName = "ProcessTokenPostingListAsync";
            try
            {
                var _tpl = (IEnumerable<DataTypesEx.TokenPositionEx>)tpl.BasePostingList.Head;
                XElement queryResult = null;
                queryResult = QueryProcessor.processTokenPostingSequence(queryString, query, _tpl);
                return queryResult;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="query"></param>
        /// <param name="tpl"></param>
        /// <returns></returns>
        public async Task<QueryResultLocationsDto> GetQueryResultLocationsAsync(string queryString, UBViews.Query.Ast.Query query, TokenPostingList tpl)
        {
            string methodName = "ProcessTokenPostingListAsync";
            try
            {
                QueryResultLocationsDto dto = null;
                var _tpl = (IEnumerable<DataTypesEx.TokenPositionEx>)tpl.BasePostingList.Head;
                XElement queryResult = null;
                queryResult = QueryProcessor.processTokenPostingSequence(queryString, query, _tpl);
                dto = await MapQueryResultElmToDto(queryResult);
                return dto;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }
        #endregion

        #region Private Helper Methods
        // obj -> dto
        private async Task<QueryResultDto> MapQueryResultObjTotDto(QueryResultObject obj)
        {
            string methodName = "MapQueryResultObjToDto";
            try
            {
                var queryResultDto = new QueryResultDto()
                {
                    Id = obj.Id,
                    Hits = obj.Hits,
                    Type = obj.Type,
                    Terms = obj.Terms,
                    Proximity = obj.Proximity,
                    QueryString = obj.QueryString,
                    ReverseQueryString = obj.ReverseQueryString,
                    QueryExpression = obj.QueryExpression
                };
                return queryResultDto;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }
        private async Task<QueryResultLocationsDto> MapQueryResultObjToDto(QueryResultObject queryResultObj)
        {
            string methodName = "MapQueryResultObjToDto";
            try
            {
                var queryResultDto = new QueryResultLocationsDto()
                {
                    Id = queryResultObj.Id,
                    Hits = queryResultObj.Hits,
                    Type = queryResultObj.Type,
                    Terms = queryResultObj.Terms,
                    Proximity = queryResultObj.Proximity,
                    QueryString = queryResultObj.QueryString,
                    ReverseQueryString = queryResultObj.ReverseQueryString,
                    QueryExpression = queryResultObj.QueryExpression
                };

                var termOccurrences = await getTermOccurrencesByQueryResultIdAsync(queryDB_Path, queryResultDto.Id);
                var isSome = OptionModule.IsSome(termOccurrences);
                if (isSome)
                {
                    var termOccs = termOccurrences.Value;
                    var queryLocations = termOccs.GroupBy(g => g.ParagraphId);
                    foreach (var locations in queryLocations)
                    {
                        // Note: The separator for id is "docId.seqId" to differentiate from pid.
                        // This is how the QueryResults.xml file handles the id separator. 
                        // This could be changed later if desired as not intrinsic in database.
                        var id = locations.First().DocumentId + "." + locations.First().SequenceId;
                        var queryLocation = new QueryLocationDto() { Id = id, Pid = locations.Key };

                        foreach (var location in locations)
                        {
                            var termLocation = new TermLocationDto()
                            {
                                Id = location.Id,
                                QueryResultId = location.QueryResutlId,
                                DocumentId = location.DocumentId,
                                SequenceId = termOccs.First().SequenceId,
                                DocumentPosition = location.DocumentPosition,
                                TextPosition = location.TextPosition,
                                TextLength = location.TextLength,
                                ParagraphId = location.ParagraphId,
                                Term = location.Term,
                            };
                            queryLocation.TermOccurrences.Add(termLocation);
                        }
                        queryResultDto.QueryLocations.Add(queryLocation);
                    }
                }
                return queryResultDto;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }
        private async Task<PostingListDto> MapPostingListObjToDto(PostingListObject postingListObj)
        {
            string methodName = "MapPostingListObjToDto";
            try
            {
                var postingListDto = new PostingListDto()
                {
                    Id = postingListObj.Id,
                    Lexeme = postingListObj.Lexeme
                };
                return postingListDto;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }
        private async Task<TermOccurrenceDto> MapTermOccurrenceObjToDto(TermOccurrenceObject termOccurrenceObj)
        {
            string methodName = "MapTermOccurrenceObjToDto";
            try
            {
                var dto = new TermOccurrenceDto()
                {
                    Id = termOccurrenceObj.Id,
                    QueryResultId = termOccurrenceObj.QueryResutlId,
                    DocumentId = termOccurrenceObj.DocumentId,
                    SequenceId = termOccurrenceObj.SequenceId,
                    DocumentPosition = termOccurrenceObj.DocumentPosition,
                    TextPosition = termOccurrenceObj.TextPosition,
                    TextLength = termOccurrenceObj.TextLength,
                    ParagraphId = termOccurrenceObj.ParagraphId,
                    Term = termOccurrenceObj.Term,
                };
                return dto;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }
        private async Task<TokenOccurrenceDto> MapTokenOccurrenceObjToDto(TokenOccurrenceObject tokenOccurrenceObj)
        {
            string methodName = "MapTokenOccurrenceObjToDto";
            try
            {
                var dto = new TokenOccurrenceDto()
                {
                    Id = tokenOccurrenceObj.Id,
                    PostingId = tokenOccurrenceObj.PostingId,
                    DocumentId = tokenOccurrenceObj.DocumentId,
                    SequenceId = tokenOccurrenceObj.SequenceId,
                    SectionId = tokenOccurrenceObj.SectionId,
                    DocumentPosition = tokenOccurrenceObj.DocumentPosition,
                    TextPosition = tokenOccurrenceObj.TextPosition,
                    ParagaphId = tokenOccurrenceObj.ParagraphId
                };
                return dto;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }
        private async Task<TokenStemDto> MapTokenStemObjToDto(TokenStemObject tokenStemObj)
        {
            string methodName = "MapTokenStemObjToDto";
            try
            {
                var dto = new TokenStemDto()
                {
                    Id = tokenStemObj.Id,
                    Lexeme = tokenStemObj.Lexeme,
                    Stemmed = tokenStemObj.Stemmed
                };
                return dto;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }
        // dto -> obj
        private async Task<QueryResultObject> MapQueryResultDtoTotObj(QueryResultDto dto)
        {
            string methodName = "MapQueryResultObjTotDto";
            try
            {
                QueryResultObject obj = new QueryResultObject(dto.Id,
                                                              dto.Hits,
                                                              dto.Type,
                                                              dto.Terms,
                                                              dto.Proximity,
                                                              dto.QueryString,
                                                              dto.ReverseQueryString,
                                                              dto.QueryExpression);
                return obj;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }
        private async Task<TermOccurrenceObject> MapTermOccurrenceDtoTotObj(TermOccurrenceDto dto)
        {
            string methodName = "MapTermOccurrenceDtoTotObj";
            try
            {
                TermOccurrenceObject obj = new TermOccurrenceObject(dto.Id,
                                                                    dto.QueryResultId,
                                                                    dto.DocumentId,
                                                                    dto.SequenceId,
                                                                    dto.DocumentPosition,
                                                                    dto.TextPosition,
                                                                    dto.TextLength,
                                                                    dto.ParagraphId,
                                                                    dto.Term);
                return obj;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }
        private async Task<QueryResultLocationsDto> MapQueryResultElmToDto(XElement queryResultElm)
        {
            string methodName = "MapQueryResultElmToDto";
            try
            {
                var queryId = Int32.Parse(queryResultElm.Attribute("id").Value);
                var locationCount = Int32.Parse(queryResultElm.Attribute("locationCount").Value);
                var queryType = queryResultElm.Attribute("type").Value;
                var terms = queryResultElm.Attribute("terms").Value;
                var proximity = queryResultElm.Attribute("proximity").Value;

                var queryStrElm = queryResultElm.Descendants("QueryString").FirstOrDefault();
                var reverseQueryStrElm = queryResultElm.Descendants("ReverseQueryString").FirstOrDefault();
                var defaultQueryStrElm = queryResultElm.Descendants("DefaultQueryString").FirstOrDefault();
                var queryExpressionElm = queryResultElm.Descendants("QueryExpression").FirstOrDefault();

                QueryResultLocationsDto dto = new QueryResultLocationsDto
                {
                    Id = queryId,
                    Hits = locationCount,
                    Type = queryType,
                    Terms = terms,
                    Proximity = proximity,
                    QueryString = queryStrElm.Value,
                    ReverseQueryString = reverseQueryStrElm != null ? reverseQueryStrElm.Value : null,
                    DefaultQueryString = defaultQueryStrElm != null ? defaultQueryStrElm.Value : null,
                    QueryExpression = queryExpressionElm.Value
                };

                var queryLocations = queryResultElm.Descendants("QueryLocation");
                foreach (var location in queryLocations)
                {

                    var id = location.Attribute("id").Value;
                    var pid = location.Attribute("pid").Value;
                    QueryLocationDto ql = new QueryLocationDto { Id = id, Pid = pid };
                    dto.QueryLocations.Add(ql);
                    var termOccurrences = location.Descendants("TermOccurrence");
                    foreach (var occurrence in termOccurrences)
                    {
                        var docId = Int32.Parse(occurrence.Attribute("docId").Value);
                        var seqId = Int32.Parse(occurrence.Attribute("seqId").Value);
                        var dpoId = Int32.Parse(occurrence.Attribute("dpoId").Value);
                        var tpoId = Int32.Parse(occurrence.Attribute("tpoId").Value);
                        var len = Int32.Parse(occurrence.Attribute("len").Value);
                        var term = occurrence.Attribute("term").Value;
                        TermLocationDto loc = new TermLocationDto
                        {
                            Id = 0,
                            QueryResultId = 0,
                            DocumentId = docId,
                            SequenceId = seqId,
                            DocumentPosition = dpoId,
                            TextPosition = tpoId,
                            TextLength = len,
                            ParagraphId = pid,
                            Term = term
                        };
                        ql.TermOccurrences.Add(loc);
                    }
                }
                return dto;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }
        #endregion
    }
}
