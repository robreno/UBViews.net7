using System.Reflection;
using System.Xml.Linq;
using UBViews.Helpers;
using UBViews.Services;

//using UBViews.SQLiteRepository;

// See: https://learn.microsoft.com/en-us/dotnet/maui/data-cloud/database-sqlite

namespace UBViews
{
    public partial class AppShell
    {
        #region Database Paths and Data Members 
        // C:\Users\robre\AppData\Local\Packages\UBViews_1s7hth42e283a\LocalState
        private string _appLocalState = FileSystem.Current.AppDataDirectory;
        // C:\Users\robre\AppData\Local\Packages\UBViews_1s7hth42e283a\LocalCache
        private string _appLocalCache = FileSystem.Current.CacheDirectory;

        private string _plDatabaseName = "postingLists.db3";
        private string _postingsPathName = Path.Combine(FileSystem.Current.AppDataDirectory, "postingLists.db3");

        private string _qrDatabaseName = "queryResults.db3";
        private string _queriesPathName = Path.Combine(FileSystem.Current.AppDataDirectory, "queryResults.db3");
        #endregion

        #region Database Initialization and Table Generation Methods
        public async Task InitializeData()
        {
            // Copy Posting Lists Database
            if (!File.Exists(_postingsPathName))
                await CopyDatabase(_plDatabaseName, _postingsPathName);
            // Copy Query Results Database
            if (!File.Exists(_queriesPathName))
                await CopyDatabase(_qrDatabaseName, _queriesPathName);

            return;
        }
        public async Task CopyDatabase(string databaseName, string targetPath)
        {
            try
            {
                var assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
                var manfests = assembly.GetManifestResourceNames();
                string rootPath = "UBViews.Resources.Raw.Database.";
                using (Stream stream = assembly.GetManifestResourceStream(rootPath + databaseName))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        File.WriteAllBytes(targetPath, ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised in AppInit.CopyDatabase => ",
                    ex.Message, "Cancel");
            }
            return;
        }
        //public async Task CreateQueryResultsDB()
        //{
        //    await QueryRepository.InitializeDatabase();
        //}
        
        //public async Task CreatePostingListsDB()
        //{
        //    await PostingRepository.InitializeDatabase();
        //}
        #endregion
    }
}
