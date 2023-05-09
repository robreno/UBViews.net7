
namespace UBViews
{
    public partial class App
    {
        public enum WindowSize { Large, Medium, Small }
        public Dictionary<int, string> WindowDimensions = new Dictionary<int, string>()
        {
            { 0, "1080,920" }, 
            { 1, "880,720" },
            { 2, "680,520" }
        };

        Dictionary<int, string> NamesSrc = new Dictionary<int, string>()
        {
            { 0, "Query/Queries.template.xml" },
            { 1, "Query/Queries.xml" },
            { 2, "Query/QueryCmdList.xml" },
            { 3, "Settings/Settings.template.xml" },
        };

        string[] srcNames = new string[]
        {
            "Query/Queries.template.xml",
            "Query/Queries.xml",
            "Query/QueryCmdList.xml",
            "Settings/Settings.template.xml"
        };

        Dictionary<int, string> TargetsSrc = new Dictionary<int, string>()
        {
            { 0, "UserQueries.xml" },
            { 1, "QueryHistory.xml" },
            { 2, "QueryCommands.xml" },
            { 3, "Settings.xml" },
        };
        string[] trgNames = new string[]
        {
            "UserQueries.xml",
            "QueryHistory.xml",
            "QueryCommands.xml",
            "Settings.xml"
        };

        Dictionary<string, bool> UserFiles = new();
        private string _appDir;
        private bool _appInitialized = false;
        private bool _hasAllUserFiles = false;
        private bool _missingUserFile = false;
        public void AppInitData(bool debug = false)
        {
            // C:\Users\robre\AppData\Local\Packages\A5924E32-1AFA-40FB-955D-1C58BE2D2ED5_9zz4h110yvjzm\LocalState

            bool hasUserData = HasUserData(trgNames);
            if (hasUserData)
                _appInitialized = true;

            if (!_appInitialized)
            {
                var v = WindowSize.Large;

                var _size = (int)WindowSize.Large;
                int _width = Int32.Parse(WindowDimensions.ElementAt(0).Value.Split(',')[0]);
                int _height = Int32.Parse(WindowDimensions.ElementAt(0).Value.Split(',')[1]);
                // Set Default Prefs: some move to settings 
                Preferences.Default.Set("culture", "en-US");
                Preferences.Default.Set("userData", true);
                Preferences.Default.Set("has_queries", false);
                Preferences.Default.Set("query_count", 0);
                Preferences.Default.Set("max_query_results", 50);
                Preferences.Default.Set("line_height", 1.0);
                Preferences.Default.Set("show_reference_pids", false);
                Preferences.Default.Set("show_playback_controls", false);
                Preferences.Default.Set("show_paper_contents", false);

                Preferences.Default.Set("default_theme", "Light");
                Preferences.Default.Set("default_width", _width);
                Preferences.Default.Set("default_height", _height);

                Preferences.Default.Set("small_window", "680,520");
                Preferences.Default.Set("medium_window", "880,720");
                Preferences.Default.Set("large_window", "1080,920");

                // SetupDefaultData();
                int size = srcNames.Length;
                for (int i = 0; i < size; i++)
                    SetupDefaultData(srcNames[i], trgNames[i]);
                // End
            }
        }
        public bool HasUserData(string[] userFiles)
        {
            _appDir = FileSystem.Current.AppDataDirectory;
            foreach (var file in userFiles)
            {
                string fullPath = Path.Combine(_appDir, file);
                bool fileExists = System.IO.File.Exists(fullPath);
                if (!fileExists)
                    _missingUserFile = true;
                UserFiles.Add(file, fileExists);
            }

            if (!_missingUserFile)
                _hasAllUserFiles = true;

            return _hasAllUserFiles;
        }
        public void SetupDefaultData(string source, string target)
        {
            Task.Run(async () =>
            {
                try
                {
                    string sourceFile = source;
                    string targetFileName = target;
                    // Read the source file
                    using Stream fileStream = await FileSystem.OpenAppPackageFileAsync(sourceFile);
                    using StreamReader reader = new StreamReader(fileStream);

                    string content = await reader.ReadToEndAsync();

                    // Write the file content to the app data directory
                    string targetFile = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, targetFileName);

                    using FileStream outputStream = System.IO.File.OpenWrite(targetFile);
                    using StreamWriter streamWriter = new StreamWriter(outputStream);

                    await streamWriter.WriteAsync(content);
                }
                catch (Exception ex)
                {
                    await App.Current.MainPage.DisplayAlert("Exception raised in AppInit.SetupDefaultData => ",
                        ex.Message, "Cancel");
                }
            });
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);
            window.Created += Window_Created;
            return window;
        }

        protected void Window_Created(object sender, EventArgs e)
        {
#if WINDOWS
            Task.Run(async () => 
            {
                const int defaultWidth = 1080;
                const int defaultHeight = 920;

                var window = (Window)sender;
                window.Width = defaultWidth;
                window.Height = defaultHeight;
                window.X = -defaultWidth;
                window.Y = -defaultHeight;

                await window.Dispatcher.DispatchAsync(() =>
                {
                    var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
                    window.X = (displayInfo.Width / displayInfo.Density - window.Width) / 2;
                    window.Y = (displayInfo.Height / displayInfo.Density - window.Height) / 2;
                });
            });
#endif
        }
    }
}
