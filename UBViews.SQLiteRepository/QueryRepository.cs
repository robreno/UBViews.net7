using SQLite;
using UBViews.SQLiteRepository.Dtos;
using UBViews.SQLiteRepository.Models;

using System.Text.Json;

namespace UBViews.SQLiteRepository
{
    public static class QueryRepository
    {
        #region Database Paths and Data Members 
        private static SQLiteAsyncConnection _databaseConn;
        private static string _databasePathLocalState = Path.Combine(FileSystem.AppDataDirectory, "queryResults.db3");
        private static string _databasePathLocalCache =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "queryResults.db3");
        private static bool _dbExists = false;
        #endregion

        #region Database Initialization
        public static async Task<bool> DatabaseExistsAsync(string dbPath)
        {
            return await Task.FromResult(_dbExists);
        }
        public static async Task InitializeDatabase()
        {
            await Task.Run(() =>
            {
                _databaseConn = new SQLiteAsyncConnection(_databasePathLocalState);
                _dbExists = true;
            });
            //result = await _databaseConn.CreateTableAsync<QueryResult>();
        }
        public static async Task<int> QueryResultCountAsync()
        {
            return await _databaseConn.Table<QueryResult>().CountAsync();
        }
        public enum CountOpts { QRes, TOcc };
        #endregion

        #region Query Repository Methods
        public static async Task<int> SaveQueryResultAsync(QueryResult queryResult)
        {
            return await _databaseConn.InsertAsync(queryResult);
        }
        public static async Task<(bool, int)> QueryResultExistsAsync(string queryString)
        {
            var queryResult = await GetQueryResultByQueryStringAsync(queryString);
            if (queryResult != null)
            {
                return (true, queryResult.Id);
            }
            else
            {
                return (false, 0);
            }
        }
        public static async Task<List<QueryResult>> GetQueryResultsAsync()
        {
            return await _databaseConn.Table<QueryResult>().ToListAsync();
        }
        public static async Task<QueryResultLocations> GetQueryResultByQueryStringAsync(string queryString)
        {
            var queryResult = await _databaseConn.Table<QueryResult>()
                                      .Where(qr => qr.QueryString == queryString)
                                      .FirstOrDefaultAsync();
            if (queryResult != null)
            {
                var qryResult = new QueryResultLocations()
                {
                    Id = queryResult.Id,
                    Hits = queryResult.Hits,
                    Type = queryResult.Type,
                    Terms = queryResult.Terms,
                    Proximity = queryResult.Proximity,
                    QueryString = queryResult.QueryString,
                    QueryExpression = queryResult.QueryExpression
                };

                var termOccurrences = await GetTermOccurrencesByQueryResultIdAsync(queryResult.Id);
                var count = termOccurrences.Count();

                var queryLocations = termOccurrences.GroupBy(g => g.ParagraphId);

                foreach (var location in queryLocations)
                {
                    var queryLocation = new QueryLocation();
                    queryLocation.Id = location.First().DocumentId + ":" + location.First().SequenceId;
                    queryLocation.Pid = location.Key;

                    foreach (var termOccurrence in location)
                    {
                        var loc = new TermLocation()
                        {
                            Term = termOccurrence.Term,
                            DocumentId = termOccurrence.DocumentId,
                            SequenceId = termOccurrence.SequenceId,
                            DocumentPosition = termOccurrence.DocumentPosition,
                            TextPosition = termOccurrence.TextPosition,
                            Length = termOccurrence.TextLength
                        };
                        queryLocation.TermOccurrences.Add(loc);
                    }
                    qryResult.QueryLocations.Add(queryLocation);
                }
                return qryResult;
            }
            return null;
        }
        public static async Task<QueryResultLocations> GetQueryResultByIdAsync(int id)
        {
            var queryResult = await _databaseConn.Table<QueryResult>()
                                      .Where(qr => qr.Id == id)
                                      .FirstOrDefaultAsync();
            if (queryResult != null)
            {
                var qryResult = new QueryResultLocations()
                {
                    Id = queryResult.Id,
                    Hits = queryResult.Hits,
                    Type = queryResult.Type,
                    Terms = queryResult.Terms,
                    Proximity = queryResult.Proximity,
                    QueryString = queryResult.QueryString,
                    QueryExpression = queryResult.QueryExpression
                };

                var termOccurrences = await GetTermOccurrencesByQueryResultIdAsync(id);
                int count = termOccurrences.Count();

                var queryLocations = termOccurrences.GroupBy(g => g.ParagraphId);

                foreach (var location in queryLocations)
                {
                    var queryLocation = new QueryLocation();
                    queryLocation.Id = location.First().DocumentId + ":" + location.First().SequenceId;
                    queryLocation.Pid = location.Key;

                    foreach (var termOccurrence in location)
                    {
                        var loc = new TermLocation()
                        {
                            Term = termOccurrence.Term,
                            DocumentId = termOccurrence.DocumentId,
                            SequenceId = termOccurrence.SequenceId,
                            DocumentPosition = termOccurrence.DocumentPosition,
                            TextPosition = termOccurrence.TextPosition,
                            Length = termOccurrence.TextLength
                        };
                        queryLocation.TermOccurrences.Add(loc);
                    }
                    qryResult.QueryLocations.Add(queryLocation);
                }
                return qryResult;
            }
            return null;
        }
        public static async Task<QueryResultLocations> GetQueryResultByIdAsyncEx(int id)
        {
            var queryResult = await _databaseConn.Table<QueryResult>()
                                      .Where(qr => qr.Id == id)
                                      .FirstOrDefaultAsync();
            if (queryResult != null)
            {
                var qryResult = new QueryResultLocations()
                {
                    Id = queryResult.Id,
                    Hits = queryResult.Hits,
                    Type = queryResult.Type,
                    Terms = queryResult.Terms,
                    Proximity = queryResult.Proximity,
                    QueryString = queryResult.QueryString,
                    QueryExpression = queryResult.QueryExpression
                };

                var termOccurrences = await GetTermOccurrencesByQueryResultIdAsync(id);
                int count = termOccurrences.Count;

                var queryLocations = termOccurrences.GroupBy(g => g.ParagraphId);

                foreach (var location in queryLocations)
                {
                    var queryLocation = new QueryLocation();
                    queryLocation.Id = location.First().DocumentId + ":" + location.First().SequenceId;
                    queryLocation.Pid = location.Key;

                    foreach (var termOccurrence in location)
                    {
                        var loc = new TermLocation()
                        {
                            Term = termOccurrence.Term,
                            DocumentId = termOccurrence.DocumentId,
                            SequenceId = termOccurrence.SequenceId,
                            DocumentPosition = termOccurrence.DocumentPosition,
                            TextPosition = termOccurrence.TextPosition,
                            Length = termOccurrence.TextLength
                        };
                        queryLocation.TermOccurrences.Add(loc);
                    }
                    qryResult.QueryLocations.Add(queryLocation);
                }
                return qryResult;
            }
            return null;
        }
        public static async Task<string> GetQueryResultJsonByIdAsync(int id)
        {
            var queryResult = await _databaseConn.Table<Models.QueryResult>()
                                      .Where(qr => qr.Id == id)
                                      .FirstOrDefaultAsync();
            if (queryResult != null)
            {
                var qryResult = new QueryResultLocations()
                {
                    Id = queryResult.Id,
                    Hits = queryResult.Hits,
                    Type = queryResult.Type,
                    Terms = queryResult.Terms,
                    Proximity = queryResult.Proximity,
                    QueryString = queryResult.QueryString,
                    QueryExpression = queryResult.QueryExpression
                };

                var termOccurrences = await GetTermOccurrencesByQueryResultIdAsync(id);
                int count = termOccurrences.Count;

                var queryLocations = termOccurrences.GroupBy(g => g.ParagraphId);

                foreach (var location in queryLocations)
                {
                    var queryLocation = new QueryLocation();
                    queryLocation.Id = location.First().DocumentId + ":" + location.First().SequenceId;
                    queryLocation.Pid = location.Key;

                    foreach (var termOccurrence in location)
                    {
                        var loc = new TermLocation()
                        {
                            Term = termOccurrence.Term,
                            DocumentId = termOccurrence.DocumentId,
                            SequenceId = termOccurrence.SequenceId,
                            DocumentPosition = termOccurrence.DocumentPosition,
                            TextPosition = termOccurrence.TextPosition,
                            Length = termOccurrence.TextLength
                        };
                        queryLocation.TermOccurrences.Add(loc);
                    }
                    qryResult.QueryLocations.Add(queryLocation);
                }

                string jsonString = JsonSerializer.Serialize(qryResult);
                return jsonString;
            }
            return null;
        }
        public static async Task<int> SaveTermOccurrenceAsync(TermOccurrence termOccurrence)
        {
            return await _databaseConn.InsertAsync(termOccurrence);
        }
        public static async Task<List<TermOccurrence>> GetTermOccurrencesAsync()
        {
            return await _databaseConn.Table<TermOccurrence>().ToListAsync();
        }
        public static async Task<List<TermOccurrence>> GetTermOccurrencesByQueryResultIdAsync(int id)
        {
            return await _databaseConn.Table<TermOccurrence>()
                                      .Where(to => to.QueryResultId == id)
                                      .ToListAsync();
        }
        #endregion
    }
}
