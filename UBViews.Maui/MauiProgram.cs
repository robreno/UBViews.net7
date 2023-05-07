using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using UBViews.Helpers;
using UBViews.Services;
using UBViews.ViewModels;
using UBViews.Views;


namespace UBViews
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMediaElement()
                .ConfigureFonts(fonts =>
                {
                    // CaslonPro Main Font
                    fonts.AddFont("ACaslonPro-Regular.otf", "ACaslonProRegular");
                    fonts.AddFont("ACaslonPro-Italic.otf", "ACaslonProItalic");
                    fonts.AddFont("ACaslonPro-Bold.otf", "ACaslonProBold");
                    fonts.AddFont("ACaslonPro-BoldItalic.otf", "ACaslonProBoldItalic");
                    fonts.AddFont("ACaslonPro-Semibold.otf", "ACaslonProSemibold");
                    fonts.AddFont("ACaslonPro-SemiboldItalic.otf", "ACaslonProSemiboldItalic");
                    // Sabon Alt Font
                    fonts.AddFont("SabonLTD-Roman.otf", "SabonLTDRoman");
                    fonts.AddFont("SabonLTStd-Bold.otf", "SabonLTStdBold");
                    fonts.AddFont("SabonLTStd-Italic.otf", "SabonLTStdItalic");
                    fonts.AddFont("SabonLTStd-BoldItalic.otf", "SabonLTStdBoldItalic");
                    // Optima and OldStyle Special Use
                    fonts.AddFont("OptimaLTStd.otf", "OptimaLTStd");
                    fonts.AddFont("OldStyle7Std.otf", "OldStyle7Std");
                    fonts.AddFont("OldStyle7Std-Italic.otf", "OldStyle7StdItalic");
                    // OpenSans Default Project Font
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Services
            builder.Services.AddTransient<IAppDataService>((e) => new XmlAppDataService(new FileService()));
            builder.Services.AddTransient<IAppSettingsService>((e) => new XmlAppSettingsService(new FileService()));
            builder.Services.AddTransient<IAudioService>((e) => new XmlAudioService(new FileService()));
            builder.Services.AddTransient<IFileService>((e) => new FileService());

            // Connectivity Service
            builder.Services.AddSingleton<IConnectivity>((e) => Connectivity.Current);
            builder.Services.AddTransient<ConnectivityViewModel>();

            // ViewModels & Pages
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<MainPage>();

            builder.Services.AddTransient<PartsViewModel>();
            builder.Services.AddTransient<PartsPage>();

            builder.Services.AddTransient<PartTitlesViewModel>();
            builder.Services.AddTransient<PartTitlesPage>();

            builder.Services.AddTransient<PaperTitlesViewModel>();
            builder.Services.AddTransient<PaperTitlesPage>();

            builder.Services.AddTransient<ContentTitlesViewModel>();
            builder.Services.AddTransient<ContentTitlesPage>();

            builder.Services.AddTransient<QueryInputViewModel>();
            builder.Services.AddTransient<QueryInputPage>();
            //builder.Services.AddTransient<QueryResultViewModel>();
            //builder.Services.AddTransient<QueryResultPage>();

            builder.Services.AddTransient<AppDataViewModel>();
            builder.Services.AddTransient<AppDataPage>();

            builder.Services.AddTransient<AppSettingsViewModel>();
            builder.Services.AddTransient<AppSettingsPage>();

            // Xaml Pages ViewModel
            builder.Services.AddTransient<XamlPaperViewModel>();
            // Generated Xaml Pages Foreword
            builder.Services.AddTransient<_000>();
            // Generated Xaml Pages
            // Part I The Central and Superuniverse
            builder.Services.AddTransient<_001>();
            builder.Services.AddTransient<_002>();
            builder.Services.AddTransient<_003>();
            builder.Services.AddTransient<_004>();
            builder.Services.AddTransient<_005>();
            builder.Services.AddTransient<_006>();
            builder.Services.AddTransient<_007>();
            builder.Services.AddTransient<_008>();
            builder.Services.AddTransient<_009>();
            builder.Services.AddTransient<_010>();
            builder.Services.AddTransient<_011>();
            builder.Services.AddTransient<_012>();
            builder.Services.AddTransient<_013>();
            builder.Services.AddTransient<_014>();
            builder.Services.AddTransient<_015>();
            builder.Services.AddTransient<_016>();
            builder.Services.AddTransient<_017>();
            builder.Services.AddTransient<_018>();
            builder.Services.AddTransient<_019>();
            builder.Services.AddTransient<_020>();
            builder.Services.AddTransient<_021>();
            builder.Services.AddTransient<_022>();
            builder.Services.AddTransient<_023>();
            builder.Services.AddTransient<_024>();
            builder.Services.AddTransient<_025>();
            builder.Services.AddTransient<_026>();
            builder.Services.AddTransient<_027>();
            builder.Services.AddTransient<_028>();
            builder.Services.AddTransient<_029>();
            builder.Services.AddTransient<_030>();
            builder.Services.AddTransient<_031>();

            // Part II 32-56
            builder.Services.AddTransient<_032>();
            builder.Services.AddTransient<_033>();
            builder.Services.AddTransient<_034>();
            builder.Services.AddTransient<_035>();
            builder.Services.AddTransient<_036>();
            builder.Services.AddTransient<_037>();
            builder.Services.AddTransient<_038>();
            builder.Services.AddTransient<_039>();
            builder.Services.AddTransient<_040>();
            builder.Services.AddTransient<_041>();
            builder.Services.AddTransient<_042>();
            builder.Services.AddTransient<_043>();
            builder.Services.AddTransient<_044>();
            builder.Services.AddTransient<_045>();
            builder.Services.AddTransient<_046>();
            builder.Services.AddTransient<_047>();
            builder.Services.AddTransient<_048>();
            builder.Services.AddTransient<_049>();
            builder.Services.AddTransient<_050>();
            builder.Services.AddTransient<_051>();
            builder.Services.AddTransient<_052>();
            builder.Services.AddTransient<_053>();
            builder.Services.AddTransient<_054>();
            builder.Services.AddTransient<_055>();
            builder.Services.AddTransient<_056>();

            // Part III 57-118
            builder.Services.AddTransient<_057>();
            builder.Services.AddTransient<_058>();
            builder.Services.AddTransient<_059>();
            builder.Services.AddTransient<_060>();
            builder.Services.AddTransient<_061>();
            builder.Services.AddTransient<_062>();
            builder.Services.AddTransient<_063>();
            builder.Services.AddTransient<_064>();
            builder.Services.AddTransient<_065>();
            builder.Services.AddTransient<_066>();
            builder.Services.AddTransient<_067>();
            builder.Services.AddTransient<_068>();
            builder.Services.AddTransient<_069>();
            builder.Services.AddTransient<_070>();
            builder.Services.AddTransient<_071>();
            builder.Services.AddTransient<_072>();
            builder.Services.AddTransient<_073>();
            builder.Services.AddTransient<_074>();
            builder.Services.AddTransient<_075>();
            builder.Services.AddTransient<_076>();
            builder.Services.AddTransient<_077>();
            builder.Services.AddTransient<_078>();
            builder.Services.AddTransient<_079>();
            builder.Services.AddTransient<_080>();
            builder.Services.AddTransient<_081>();
            builder.Services.AddTransient<_082>();
            builder.Services.AddTransient<_083>();
            builder.Services.AddTransient<_084>();
            builder.Services.AddTransient<_085>();
            builder.Services.AddTransient<_086>();
            builder.Services.AddTransient<_087>();
            builder.Services.AddTransient<_088>();
            builder.Services.AddTransient<_089>();
            builder.Services.AddTransient<_090>();
            builder.Services.AddTransient<_091>();
            builder.Services.AddTransient<_092>();
            builder.Services.AddTransient<_093>();
            builder.Services.AddTransient<_094>();
            builder.Services.AddTransient<_095>();
            builder.Services.AddTransient<_096>();
            builder.Services.AddTransient<_097>();
            builder.Services.AddTransient<_098>();
            builder.Services.AddTransient<_099>();
            builder.Services.AddTransient<_100>();
            builder.Services.AddTransient<_101>();
            builder.Services.AddTransient<_102>();
            builder.Services.AddTransient<_103>();
            builder.Services.AddTransient<_104>();
            builder.Services.AddTransient<_105>();
            builder.Services.AddTransient<_106>();
            builder.Services.AddTransient<_107>();
            builder.Services.AddTransient<_108>();
            builder.Services.AddTransient<_109>();
            builder.Services.AddTransient<_110>();
            builder.Services.AddTransient<_111>();
            builder.Services.AddTransient<_112>();
            builder.Services.AddTransient<_113>();
            builder.Services.AddTransient<_114>();
            builder.Services.AddTransient<_115>();
            builder.Services.AddTransient<_116>();
            builder.Services.AddTransient<_117>();
            builder.Services.AddTransient<_118>();

            // Part IV 118-196
            builder.Services.AddTransient<_119>();
            builder.Services.AddTransient<_120>();
            builder.Services.AddTransient<_121>();
            builder.Services.AddTransient<_122>();
            builder.Services.AddTransient<_123>();
            builder.Services.AddTransient<_124>();
            builder.Services.AddTransient<_125>();
            builder.Services.AddTransient<_126>();
            builder.Services.AddTransient<_127>();
            builder.Services.AddTransient<_128>();
            builder.Services.AddTransient<_129>();
            builder.Services.AddTransient<_130>();
            builder.Services.AddTransient<_131>();
            builder.Services.AddTransient<_132>();
            builder.Services.AddTransient<_133>();
            builder.Services.AddTransient<_134>();
            builder.Services.AddTransient<_135>();
            builder.Services.AddTransient<_136>();
            builder.Services.AddTransient<_137>();
            builder.Services.AddTransient<_138>();
            builder.Services.AddTransient<_139>();
            builder.Services.AddTransient<_140>();
            builder.Services.AddTransient<_141>();
            builder.Services.AddTransient<_142>();
            builder.Services.AddTransient<_143>();
            builder.Services.AddTransient<_144>();
            builder.Services.AddTransient<_145>();
            builder.Services.AddTransient<_146>();
            builder.Services.AddTransient<_147>();
            builder.Services.AddTransient<_148>();
            builder.Services.AddTransient<_149>();
            builder.Services.AddTransient<_150>();
            builder.Services.AddTransient<_151>();
            builder.Services.AddTransient<_152>();
            builder.Services.AddTransient<_153>();
            builder.Services.AddTransient<_154>();
            builder.Services.AddTransient<_155>();
            builder.Services.AddTransient<_156>();
            builder.Services.AddTransient<_157>();
            builder.Services.AddTransient<_158>();
            builder.Services.AddTransient<_159>();
            builder.Services.AddTransient<_160>();
            builder.Services.AddTransient<_161>();
            builder.Services.AddTransient<_162>();
            builder.Services.AddTransient<_163>();
            builder.Services.AddTransient<_164>();
            builder.Services.AddTransient<_165>();
            builder.Services.AddTransient<_166>();
            builder.Services.AddTransient<_167>();
            builder.Services.AddTransient<_168>();
            builder.Services.AddTransient<_169>();
            builder.Services.AddTransient<_170>();
            builder.Services.AddTransient<_171>();
            builder.Services.AddTransient<_172>();
            builder.Services.AddTransient<_173>();
            builder.Services.AddTransient<_174>();
            builder.Services.AddTransient<_175>();
            builder.Services.AddTransient<_176>();
            builder.Services.AddTransient<_177>();
            builder.Services.AddTransient<_178>();
            builder.Services.AddTransient<_179>();
            builder.Services.AddTransient<_180>();
            builder.Services.AddTransient<_181>();
            builder.Services.AddTransient<_182>();
            builder.Services.AddTransient<_183>();
            builder.Services.AddTransient<_184>();
            builder.Services.AddTransient<_185>();
            builder.Services.AddTransient<_186>();
            builder.Services.AddTransient<_187>();
            builder.Services.AddTransient<_188>();
            builder.Services.AddTransient<_189>();
            builder.Services.AddTransient<_190>();
            builder.Services.AddTransient<_191>();
            builder.Services.AddTransient<_192>();
            builder.Services.AddTransient<_193>();
            builder.Services.AddTransient<_194>();
            builder.Services.AddTransient<_195>();
            builder.Services.AddTransient<_196>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}