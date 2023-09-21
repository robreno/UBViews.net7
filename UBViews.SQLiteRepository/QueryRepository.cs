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
        /// <summary>
        /// SaveQueryResultAsync
        /// </summary>
        /// <param name="queryResult"></param>
        /// <returns></returns>
        public static async Task<int> SaveQueryResultAsync(QueryResult queryResult)
        {
            try
            {
                return await _databaseConn.InsertAsync(queryResult);
            }
            catch (Exception ex) 
            {
                return 0;
            }
        }

        /// <summary>
        /// QueryResultExistsAsync
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public static async Task<(bool, int)> QueryResultExistsAsync(string queryString)
        {
            try
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
            catch (Exception ex) 
            {
                return (false, -1);
            }
            
        }

        /// <summary>
        /// GetQueryResultsAsync
        /// </summary>
        /// <returns></returns>
        public static async Task<List<QueryResult>> GetQueryResultsAsync()
        {
            // System.IO.FileNotFoundException
            // Message = Could not load file or assembly 'SQLitePCLRaw.provider.dynamic_cdecl, Version=2.0.4.976, Culture=neutral, PublicKeyToken=b68184102cba0b3b' or one of its dependencies.
            // https://stackoverflow.com/questions/56169808/could-not-load-file-or-assembly-sqlitepclraw-core
            // https://stackoverflow.com/questions/72755874/getting-a-filenotfoundexception-when-trying-to-make-a-sqlite-connection-in-net
            try
            {
                return await _databaseConn.Table<QueryResult>().ToListAsync();
            }
            catch (Exception ex) 
            {
                return null;
            }
        }

        /// <summary>
        /// GetQueryResultByQueryStringAsync
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public static async Task<QueryResultLocations> GetQueryResultByQueryStringAsync(string queryString)
        {
            try
            {
                QueryResultLocations queryResultLocations = null;
                var queryResult = await _databaseConn.Table<QueryResult>()
                                          .Where(qr => qr.QueryString == queryString)
                                          .FirstOrDefaultAsync();
                if (queryResult != null)
                {
                    queryResultLocations = new QueryResultLocations()
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
                    var queryLocations = termOccurrences.GroupBy(g => g.ParagraphId);

                    foreach (var locations in queryLocations)
                    {
                        var id = locations.First().DocumentId + ":" + locations.First().SequenceId;
                        var pid = locations.Key;
                        var queryLocation = new QueryLocation() { Id = id, Pid = pid };

                        foreach (var location in locations)
                        {
                            var termLocation = new TermLocation()
                            {
                                Term = location.Term,
                                DocumentId = location.DocumentId,
                                SequenceId = location.SequenceId,
                                DocumentPosition = location.DocumentPosition,
                                TextPosition = location.TextPosition,
                                Length = location.TextLength
                            };
                            queryLocation.TermOccurrences.Add(termLocation);
                        }
                        queryResultLocations.QueryLocations.Add(queryLocation);
                    }
                }
                return queryResultLocations;
            }
            catch (Exception ex) 
            {
                return null;
            }
        }

        /// <summary>
        /// GetQueryResultByIdAsync
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<QueryResultLocations> GetQueryResultByIdAsync(int id)
        {
            try
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
            catch (Exception ex)
            {
                return null;
            }
            
        }

        /// <summary>
        /// GetQueryResultByIdAsyncEx
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// GetQueryResultJsonByIdAsync
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// SaveTermOccurrenceAsync
        /// </summary>
        /// <param name="termOccurrence"></param>
        /// <returns></returns>
        public static async Task<int> SaveTermOccurrenceAsync(TermOccurrence termOccurrence)
        {
            try
            {
                return await _databaseConn.InsertAsync(termOccurrence);
            }
            catch (Exception ex) 
            {
                return -1;
            }
        }

        /// <summary>
        /// GetTermOccurrencesAsync
        /// </summary>
        /// <returns></returns>
        public static async Task<List<TermOccurrence>> GetTermOccurrencesAsync()
        {
            try
            {
                return await _databaseConn.Table<TermOccurrence>().ToListAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// GetTermOccurrencesByQueryResultIdAsync
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<List<TermOccurrence>> GetTermOccurrencesByQueryResultIdAsync(int id)
        {
            try
            {
                return await _databaseConn.Table<TermOccurrence>()
                                            .Where(to => to.QueryResultId == id)
                                            .ToListAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region Private Helper Methods
        //private async Task<QueryResultLocationsDto> MapQueryResultToDto(QueryResult queryResult)
        //{
        //    try
        //    {
        //        QueryResultLocationsDto queryResultDto = new QueryResultLocationsDto()
        //        {
        //            Id = queryResult.Id,
        //            Hits = queryResult.Hits,
        //            Type = queryResult.Type,
        //            Terms = queryResult.Terms,
        //            Proximity = queryResult.Proximity,
        //            QueryString = queryResult.QueryString,
        //            QueryExpression = queryResult.QueryExpression
        //        };

        //        var termOccurrences = await getTermOccurrencesByQueryResultIdAsync(dbPath, queryResult.Id);
        //        var queryLocations = termOccurrences.GroupBy(g => g.ParagraphId);

        //        foreach (var locations in queryLocations)
        //        {
        //            var id = locations.First().DocumentId + ":" + locations.First().SequenceId;
        //            var queryLocation = new QueryLocationDto() { Id = id, Pid = locations.Key };

        //            foreach (var location in locations)
        //            {
        //                var termLocation = new TermLocationDto()
        //                {
        //                    Term = location.Term,
        //                    DocumentId = location.DocumentId,
        //                    SequenceId = termOccurrences.First().SequenceId,
        //                    DocumentPosition = location.DocumentPosition,
        //                    TextPosition = location.TextPosition,
        //                    TextLength = location.TextLength
        //                };
        //                queryLocation.TermOccurrences.Add(termLocation);
        //            }
        //            queryResultDto.QueryLocations.Add(queryLocation);
        //        }
        //        return queryResultDto;
        //    }
        //    catch (Exception ex)
        //    {
        //        await App.Current.MainPage.DisplayAlert("Exception raised => GetQueryResultByQueryStringAsync.", ex.Message, "Cancel");
        //        return null;
        //    }
        //}
        #endregion
    }
}
