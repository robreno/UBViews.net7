namespace UBViews
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            AppInitData(true);

            MainPage = new AppShell();
        }
    }
}