using SQLite;
using UBViews.Repositories.Models;

namespace UBViews.Repositories
{
    public static class PostingRepository
    {
        private static SQLiteAsyncConnection _databaseConn;
        private static string _databasePathLocalState = Path.Combine(FileSystem.AppDataDirectory, "postingLists.db3");
        private static string _databasePathLocalCache =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "postingLists.db3");
        private static bool _dbExists = false;

        public static async Task Init()
        {
            _databaseConn = new SQLiteAsyncConnection(_databasePathLocalCache);
            var result = await _databaseConn.CreateTableAsync<PostingList>();
            result = await _databaseConn.CreateTableAsync<TokenOccurrence>();
            result = await _databaseConn.CreateTableAsync<TokenStem>();
            _dbExists = true;

            //await Task.Run(() => 
            //{
            //    _databaseConn = new SQLiteAsyncConnection(_databasePathLocalCache);
            //    var result = _databaseConn.CreateTableAsync<PostingList>();
            //    result = _databaseConn.CreateTableAsync<TokenOccurrence>();
            //    result = _databaseConn.CreateTableAsync<TokenStem>();
            //    _dbExists = true;
            //});
        }

        public static async Task<bool> ExistsAsync(string dbPath) 
        {

            return await Task.FromResult(_dbExists);
        }

        public static async Task<List<PostingList>> GetPostingsAsync()
        {
            return await _databaseConn.Table<PostingList>().ToListAsync();
        }

        public static async Task<PostingList> GetPostingByLexemeAsync(string lexeme)
        {
            return await _databaseConn.Table<PostingList>().Where(p => p.Lexeme == lexeme).FirstOrDefaultAsync();
        }

        public static async Task<int> CountAsync(string dbPath) 
        {
            return await _databaseConn.Table<PostingList>().CountAsync();
        }

        public static async Task<List<TokenOccurrence>> GetTokenOccurrencesAsync(int postingId)
        {
            return await _databaseConn.Table<TokenOccurrence>().Where(o => o.PostingId == postingId).ToListAsync();
        }
        public static async Task<int> SavePostingAsync(PostingList posting)
        {
            return await _databaseConn.InsertAsync(posting);
        }
        public static async Task<int> SaveTokenOccurenceAsync(TokenOccurrence occurrence)
        {
            return await _databaseConn.InsertAsync(occurrence);
        }
        public static async Task<int> SaveTokenOccurrencesAsync(IEnumerable<TokenOccurrence> occurrences)
        {
            return await _databaseConn.InsertAllAsync(occurrences);
        }

        public static async Task<List<TokenStem>> GetTokenStemsAsync()
        {
            return await _databaseConn.Table<TokenStem>().ToListAsync();
        }

        public static async Task<int> CountAsync() 
        {
            return await _databaseConn.Table<TokenStem>().CountAsync();
        }

        public static async Task<TokenStem> GetTokenStemAsync(string lexeme)
        {
            return await _databaseConn.Table<TokenStem>().Where(s => s.Lexeme == lexeme).FirstOrDefaultAsync();
        }

        public static async Task<int> SaveTokenStem(TokenStem stem) 
        { 
            return await _databaseConn.InsertAsync(stem);
        }
    }
}
