
namespace UBViews
{
    public partial class App
    {
        public enum WindowSize { Large, Medium, Small }

        Dictionary<int, (int, int)> WindowDimensions = new Dictionary<int, (int, int)>()
        {
            { 0, (1080, 920) },
            { 1, (880,720) },
            { 2, (680,520) }
        };

        Dictionary<int, string> NamesSrc = new Dictionary<int, string>()
        {
            { 0, "Templates/Contacts.xml" },
            { 1, "Templates/Notes.xml" },
            { 2, "Templates/Settings.xml" },
            { 3, "Database/queryResults.db3" },
            { 4, "Database/postingLists.db3" },
        };

        Dictionary<int, string> TargetsSrc = new Dictionary<int, string>()
        {
            { 0, "Contacts.xml" },
            { 1, "Notes.xml" },
            { 2, "Settings.xml" },
            { 3, "queryResults.db3" },
            { 4, "postingLists.db3" },
        };

        string[] srcNames = new string[]
        {
            "Templates/Contacts.xml",
            "Templates/Notes.xml",
            "Templates/Settings.xml",
        };

        string[] trgNames = new string[]
        {
            "Contacts.xml",
            "Notes.xml",
            "Settings.xml",
        };

        Dictionary<string, bool> UserFiles = new();
        private string _appDir;
        private bool _appInitialized = false;
        private bool _hasAllUserFiles = false;
        private bool _missingUserFile = false;
        public void AppInitData(bool debug = false)
        {
            bool hasUserData = HasUserData(trgNames);
            if (hasUserData)
                _appInitialized = true;

            if (!_appInitialized)
            {
                var tpl = (0, 0);
                bool success = WindowDimensions.TryGetValue((int)WindowSize.Medium, out tpl);
                int _width = tpl.Item1;
                int _height = tpl.Item2;

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

                Preferences.Default.Set("default_theme", "light");


                Preferences.Default.Set("default_width", _width);
                Preferences.Default.Set("default_height", _height);

                Preferences.Default.Set("small_window", "680,520");
                Preferences.Default.Set("medium_window", "880,720");
                Preferences.Default.Set("large_window", "1080,920");

                Preferences.Default.Set("auto_send_email", false);
                Preferences.Default.Set("stream_audio", false);

                // SetupDefaultData();
                int size = srcNames.Length;
                for (int i = 0; i < size; i++)
                    SetupDefaultData(srcNames[i], trgNames[i]);
                // End
            }
        }
        public bool HasUserData(string[] userFiles)
        {
            // C:\Users\robre\AppData\Local\Packages\UBViews_1s7hth42e283a\LocalState
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
            var tpl = (0, 0);
            bool success = WindowDimensions.TryGetValue((int)WindowSize.Large, out tpl);
            int _width = tpl.Item1;
            int _height = tpl.Item2;
            //var it1, it2 = GetDimensions(WindowSize.Large);
#if WINDOWS
            Task.Run(async () => 
            {
                int defaultWidth = _width;
                int defaultHeight = _height;

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

        protected (int, int) GetDimensions(WindowSize sizeId)
        {
            var tpl = (0, 0);
            int _width  = 0;
            int _height = 0;
            bool success = WindowDimensions.TryGetValue((int)sizeId, out tpl);
            _width = tpl.Item1;
            _height = tpl.Item2;
            return (_width, _height);
        }
    }
}
