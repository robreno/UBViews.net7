using SQLite;
using UBViews.Repositories.Models;

namespace UBViews.Repositories
{
    public static class PostingRepository
    {
        #region Database Paths and Data Members 
        private static SQLiteAsyncConnection _databaseConn;
        private static string _databasePathLocalState = Path.Combine(FileSystem.AppDataDirectory, "postingLists.db3");
        private static string _databasePathLocalCache =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "postingLists.db3");
        private static bool _dbExists = false;
        public enum CountOpts { PLst, TOcc, TStem };
        #endregion

        #region Database Initialization
        public static async Task InitializeDatabase()
        {
            await Task.Run(() =>
            {
                _databaseConn = new SQLiteAsyncConnection(_databasePathLocalState);
                _dbExists = true;
            });

            //var result = await _databaseConn.CreateTableAsync<PostingList>();
            //result = await _databaseConn.CreateTableAsync<TokenOccurrence>();
            //result = await _databaseConn.CreateTableAsync<TokenStem>();
        }
        public static async Task<bool> DatabaseExistsAsync(string dbPath)
        {

            return await Task.FromResult(_dbExists);
        }
        public static async Task<int> CountAsync(CountOpts ops)
        {
            int recordCount = -1;
            bool _exists = await Task.FromResult(_dbExists);
            if (!_exists)
                return recordCount;

            switch (ops)
            {
                case CountOpts.PLst:
                    recordCount = await _databaseConn.Table<PostingList>().CountAsync();
                    break;
                case CountOpts.TOcc:
                    recordCount = await _databaseConn.Table<TokenOccurrence>().CountAsync();
                    break;
                case CountOpts.TStem:
                    recordCount = await _databaseConn.Table<TokenStem>().CountAsync();
                    break; ;
                    break;
            }
            return recordCount;
        }
        public static async Task<int> PostingListCountAsync()
        {
            return await _databaseConn.Table<PostingList>().CountAsync();
        }
        public static async Task<int> TokenOccurrenceCountAsync()
        {
            return await _databaseConn.Table<TokenOccurrence>().CountAsync();
        }
        public static async Task<int> TokenStemCountAsync()
        {
            return await _databaseConn.Table<TokenStem>().CountAsync();
        }
        #endregion

        #region Posting Repository Api
        public static async Task<int> SavePostingAsync(PostingList posting)
        {
            return await _databaseConn.InsertAsync(posting);
        }
        public static async Task<List<PostingList>> GetPostingsAsync()
        {
            return await _databaseConn.Table<PostingList>().ToListAsync();
        }
        public static async Task<PostingList> GetPostingByLexemeAsync(string lexeme)
        {
            try
            {
                return await _databaseConn.Table<PostingList>().Where(p => p.Lexeme == lexeme).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                return null;
            }
        }
        public static async Task<int> SaveTokenOccurenceAsync(TokenOccurrence occurrence)
        {
            return await _databaseConn.InsertAsync(occurrence);
        }
        public static async Task<int> SaveTokenOccurrencesAsync(IEnumerable<TokenOccurrence> occurrences)
        {
            return await _databaseConn.InsertAllAsync(occurrences);
        }
        public static async Task<List<TokenOccurrence>> GetTokenOccurrencesAsync(int postingId)
        {
            return await _databaseConn.Table<TokenOccurrence>().Where(o => o.PostingId == postingId).ToListAsync();
        }
        public static async Task<int> SaveTokenStem(TokenStem stem) 
        { 
            return await _databaseConn.InsertAsync(stem);
        }
        public static async Task<TokenStem> GetTokenStemAsync(string lexeme)
        {
            try
            {
                return await _databaseConn.Table<TokenStem>().Where(s => s.Lexeme == lexeme).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised in PostingRepository.GetTokenStemAsync => ",
                   ex.Message, "Ok");
                return null;
            }
        }
        public static async Task<List<TokenStem>> GetTokenStemsAsync()
        {
            return await _databaseConn.Table<TokenStem>().ToListAsync();
        }
        #endregion
    }
}
