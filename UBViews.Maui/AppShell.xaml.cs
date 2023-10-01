using UBViews.Views;

namespace UBViews
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(PartsPage), typeof(PartsPage));
            Routing.RegisterRoute(nameof(PartTitlesPage), typeof(PartTitlesPage));
            Routing.RegisterRoute(nameof(ContentTitlesPage), typeof(ContentTitlesPage));
            Routing.RegisterRoute(nameof(PaperTitlesPage), typeof(PaperTitlesPage));

            Routing.RegisterRoute(nameof(AppDataPage), typeof(AppDataPage));
            Routing.RegisterRoute(nameof(AppSettingsPage), typeof(AppSettingsPage));

            Routing.RegisterRoute(nameof(QueryInputPage), typeof(QueryInputPage));
            Routing.RegisterRoute(nameof(QueryResultPage), typeof(QueryResultPage));

            #region Xaml Papers
            // Generated Xaml Pages
            // Part I The Central and Superuniverse
            Routing.RegisterRoute(nameof(_000), typeof(_000));
            Routing.RegisterRoute(nameof(_001), typeof(_001));
            Routing.RegisterRoute(nameof(_002), typeof(_002));
            Routing.RegisterRoute(nameof(_003), typeof(_003));
            Routing.RegisterRoute(nameof(_004), typeof(_004));
            Routing.RegisterRoute(nameof(_005), typeof(_005));
            Routing.RegisterRoute(nameof(_006), typeof(_006));
            Routing.RegisterRoute(nameof(_007), typeof(_007));
            Routing.RegisterRoute(nameof(_008), typeof(_008));
            Routing.RegisterRoute(nameof(_009), typeof(_009));
            Routing.RegisterRoute(nameof(_010), typeof(_010));
            Routing.RegisterRoute(nameof(_011), typeof(_011));
            Routing.RegisterRoute(nameof(_012), typeof(_012));
            Routing.RegisterRoute(nameof(_013), typeof(_013));
            Routing.RegisterRoute(nameof(_014), typeof(_014));
            Routing.RegisterRoute(nameof(_015), typeof(_015));
            Routing.RegisterRoute(nameof(_016), typeof(_016));
            Routing.RegisterRoute(nameof(_017), typeof(_017));
            Routing.RegisterRoute(nameof(_018), typeof(_018));
            Routing.RegisterRoute(nameof(_019), typeof(_019));
            Routing.RegisterRoute(nameof(_020), typeof(_020));
            Routing.RegisterRoute(nameof(_021), typeof(_021));
            Routing.RegisterRoute(nameof(_022), typeof(_022));
            Routing.RegisterRoute(nameof(_023), typeof(_023));
            Routing.RegisterRoute(nameof(_024), typeof(_024));
            Routing.RegisterRoute(nameof(_025), typeof(_025));
            Routing.RegisterRoute(nameof(_026), typeof(_026));
            Routing.RegisterRoute(nameof(_027), typeof(_027));
            Routing.RegisterRoute(nameof(_028), typeof(_028));
            Routing.RegisterRoute(nameof(_029), typeof(_029));
            Routing.RegisterRoute(nameof(_030), typeof(_030));
            Routing.RegisterRoute(nameof(_031), typeof(_031));

            // Part II The Evolution of Local Universes
            Routing.RegisterRoute(nameof(_032), typeof(_032));
            Routing.RegisterRoute(nameof(_033), typeof(_033));
            Routing.RegisterRoute(nameof(_034), typeof(_034));
            Routing.RegisterRoute(nameof(_035), typeof(_035));
            Routing.RegisterRoute(nameof(_036), typeof(_036));
            Routing.RegisterRoute(nameof(_037), typeof(_037));
            Routing.RegisterRoute(nameof(_038), typeof(_038));
            Routing.RegisterRoute(nameof(_039), typeof(_039));
            Routing.RegisterRoute(nameof(_040), typeof(_040));
            Routing.RegisterRoute(nameof(_041), typeof(_041));
            Routing.RegisterRoute(nameof(_042), typeof(_042));
            Routing.RegisterRoute(nameof(_043), typeof(_043));
            Routing.RegisterRoute(nameof(_044), typeof(_044));
            Routing.RegisterRoute(nameof(_045), typeof(_045));
            Routing.RegisterRoute(nameof(_046), typeof(_046));
            Routing.RegisterRoute(nameof(_047), typeof(_047));
            Routing.RegisterRoute(nameof(_048), typeof(_048));
            Routing.RegisterRoute(nameof(_049), typeof(_049));
            Routing.RegisterRoute(nameof(_050), typeof(_050));
            Routing.RegisterRoute(nameof(_051), typeof(_051));
            Routing.RegisterRoute(nameof(_052), typeof(_052));
            Routing.RegisterRoute(nameof(_053), typeof(_053));
            Routing.RegisterRoute(nameof(_054), typeof(_054));
            Routing.RegisterRoute(nameof(_055), typeof(_055));
            Routing.RegisterRoute(nameof(_056), typeof(_056));

            // Part III The History of Urantia
            Routing.RegisterRoute(nameof(_057), typeof(_057));
            Routing.RegisterRoute(nameof(_058), typeof(_058));
            Routing.RegisterRoute(nameof(_059), typeof(_059));
            Routing.RegisterRoute(nameof(_060), typeof(_060));
            Routing.RegisterRoute(nameof(_061), typeof(_061));
            Routing.RegisterRoute(nameof(_062), typeof(_062));
            Routing.RegisterRoute(nameof(_063), typeof(_063));
            Routing.RegisterRoute(nameof(_064), typeof(_064));
            Routing.RegisterRoute(nameof(_065), typeof(_065));
            Routing.RegisterRoute(nameof(_066), typeof(_066));
            Routing.RegisterRoute(nameof(_067), typeof(_067));
            Routing.RegisterRoute(nameof(_068), typeof(_068));
            Routing.RegisterRoute(nameof(_069), typeof(_069));
            Routing.RegisterRoute(nameof(_070), typeof(_070));
            Routing.RegisterRoute(nameof(_071), typeof(_071));
            Routing.RegisterRoute(nameof(_072), typeof(_072));
            Routing.RegisterRoute(nameof(_073), typeof(_073));
            Routing.RegisterRoute(nameof(_074), typeof(_074));
            Routing.RegisterRoute(nameof(_075), typeof(_075));
            Routing.RegisterRoute(nameof(_076), typeof(_076));
            Routing.RegisterRoute(nameof(_077), typeof(_077));
            Routing.RegisterRoute(nameof(_078), typeof(_078));
            Routing.RegisterRoute(nameof(_079), typeof(_079));
            Routing.RegisterRoute(nameof(_080), typeof(_080));
            Routing.RegisterRoute(nameof(_081), typeof(_081));
            Routing.RegisterRoute(nameof(_082), typeof(_082));
            Routing.RegisterRoute(nameof(_083), typeof(_083));
            Routing.RegisterRoute(nameof(_084), typeof(_084));
            Routing.RegisterRoute(nameof(_085), typeof(_085));
            Routing.RegisterRoute(nameof(_086), typeof(_086));
            Routing.RegisterRoute(nameof(_087), typeof(_087));
            Routing.RegisterRoute(nameof(_088), typeof(_088));
            Routing.RegisterRoute(nameof(_089), typeof(_089));
            Routing.RegisterRoute(nameof(_090), typeof(_090));
            Routing.RegisterRoute(nameof(_091), typeof(_091));
            Routing.RegisterRoute(nameof(_092), typeof(_092));
            Routing.RegisterRoute(nameof(_093), typeof(_093));
            Routing.RegisterRoute(nameof(_094), typeof(_094));
            Routing.RegisterRoute(nameof(_095), typeof(_095));
            Routing.RegisterRoute(nameof(_096), typeof(_096));
            Routing.RegisterRoute(nameof(_097), typeof(_097));
            Routing.RegisterRoute(nameof(_098), typeof(_098));
            Routing.RegisterRoute(nameof(_099), typeof(_099));
            Routing.RegisterRoute(nameof(_100), typeof(_100));
            Routing.RegisterRoute(nameof(_101), typeof(_101));
            Routing.RegisterRoute(nameof(_102), typeof(_102));
            Routing.RegisterRoute(nameof(_103), typeof(_103));
            Routing.RegisterRoute(nameof(_104), typeof(_104));
            Routing.RegisterRoute(nameof(_105), typeof(_105));
            Routing.RegisterRoute(nameof(_106), typeof(_106));
            Routing.RegisterRoute(nameof(_107), typeof(_107));
            Routing.RegisterRoute(nameof(_108), typeof(_108));
            Routing.RegisterRoute(nameof(_109), typeof(_109));
            Routing.RegisterRoute(nameof(_110), typeof(_110));
            Routing.RegisterRoute(nameof(_111), typeof(_111));
            Routing.RegisterRoute(nameof(_112), typeof(_112));
            Routing.RegisterRoute(nameof(_113), typeof(_113));
            Routing.RegisterRoute(nameof(_114), typeof(_114));
            Routing.RegisterRoute(nameof(_115), typeof(_115));
            Routing.RegisterRoute(nameof(_116), typeof(_116));
            Routing.RegisterRoute(nameof(_117), typeof(_117));
            Routing.RegisterRoute(nameof(_118), typeof(_118));

            // Part IV The Life and Teachings of Jesus
            Routing.RegisterRoute(nameof(_119), typeof(_119));
            Routing.RegisterRoute(nameof(_120), typeof(_120));
            Routing.RegisterRoute(nameof(_121), typeof(_121));
            Routing.RegisterRoute(nameof(_122), typeof(_122));
            Routing.RegisterRoute(nameof(_123), typeof(_123));
            Routing.RegisterRoute(nameof(_124), typeof(_124));
            Routing.RegisterRoute(nameof(_125), typeof(_125));
            Routing.RegisterRoute(nameof(_126), typeof(_126));
            Routing.RegisterRoute(nameof(_127), typeof(_127));
            Routing.RegisterRoute(nameof(_128), typeof(_128));
            Routing.RegisterRoute(nameof(_129), typeof(_129));
            Routing.RegisterRoute(nameof(_130), typeof(_130));
            Routing.RegisterRoute(nameof(_131), typeof(_131));
            Routing.RegisterRoute(nameof(_132), typeof(_132));
            Routing.RegisterRoute(nameof(_133), typeof(_133));
            Routing.RegisterRoute(nameof(_134), typeof(_134));
            Routing.RegisterRoute(nameof(_135), typeof(_135));
            Routing.RegisterRoute(nameof(_136), typeof(_136));
            Routing.RegisterRoute(nameof(_137), typeof(_137));
            Routing.RegisterRoute(nameof(_138), typeof(_138));
            Routing.RegisterRoute(nameof(_139), typeof(_139));
            Routing.RegisterRoute(nameof(_140), typeof(_140));
            Routing.RegisterRoute(nameof(_141), typeof(_141));
            Routing.RegisterRoute(nameof(_142), typeof(_142));
            Routing.RegisterRoute(nameof(_143), typeof(_143));
            Routing.RegisterRoute(nameof(_144), typeof(_144));
            Routing.RegisterRoute(nameof(_145), typeof(_145));
            Routing.RegisterRoute(nameof(_146), typeof(_146));
            Routing.RegisterRoute(nameof(_147), typeof(_147));
            Routing.RegisterRoute(nameof(_148), typeof(_148));
            Routing.RegisterRoute(nameof(_149), typeof(_149));
            Routing.RegisterRoute(nameof(_150), typeof(_150));
            Routing.RegisterRoute(nameof(_151), typeof(_151));
            Routing.RegisterRoute(nameof(_152), typeof(_152));
            Routing.RegisterRoute(nameof(_153), typeof(_153));
            Routing.RegisterRoute(nameof(_154), typeof(_154));
            Routing.RegisterRoute(nameof(_155), typeof(_155));
            Routing.RegisterRoute(nameof(_156), typeof(_156));
            Routing.RegisterRoute(nameof(_157), typeof(_157));
            Routing.RegisterRoute(nameof(_158), typeof(_158));
            Routing.RegisterRoute(nameof(_159), typeof(_159));
            Routing.RegisterRoute(nameof(_160), typeof(_160));
            Routing.RegisterRoute(nameof(_161), typeof(_161));
            Routing.RegisterRoute(nameof(_162), typeof(_162));
            Routing.RegisterRoute(nameof(_163), typeof(_163));
            Routing.RegisterRoute(nameof(_164), typeof(_164));
            Routing.RegisterRoute(nameof(_165), typeof(_165));
            Routing.RegisterRoute(nameof(_166), typeof(_166));
            Routing.RegisterRoute(nameof(_167), typeof(_167));
            Routing.RegisterRoute(nameof(_168), typeof(_168));
            Routing.RegisterRoute(nameof(_169), typeof(_169));
            Routing.RegisterRoute(nameof(_170), typeof(_170));
            Routing.RegisterRoute(nameof(_171), typeof(_171));
            Routing.RegisterRoute(nameof(_172), typeof(_172));
            Routing.RegisterRoute(nameof(_173), typeof(_173));
            Routing.RegisterRoute(nameof(_174), typeof(_174));
            Routing.RegisterRoute(nameof(_175), typeof(_175));
            Routing.RegisterRoute(nameof(_176), typeof(_176));
            Routing.RegisterRoute(nameof(_177), typeof(_177));
            Routing.RegisterRoute(nameof(_178), typeof(_178));
            Routing.RegisterRoute(nameof(_179), typeof(_179));
            Routing.RegisterRoute(nameof(_180), typeof(_180));
            Routing.RegisterRoute(nameof(_181), typeof(_181));
            Routing.RegisterRoute(nameof(_182), typeof(_182));
            Routing.RegisterRoute(nameof(_183), typeof(_183));
            Routing.RegisterRoute(nameof(_184), typeof(_184));
            Routing.RegisterRoute(nameof(_185), typeof(_185));
            Routing.RegisterRoute(nameof(_186), typeof(_186));
            Routing.RegisterRoute(nameof(_187), typeof(_187));
            Routing.RegisterRoute(nameof(_188), typeof(_188));
            Routing.RegisterRoute(nameof(_189), typeof(_189));
            Routing.RegisterRoute(nameof(_190), typeof(_190));
            Routing.RegisterRoute(nameof(_191), typeof(_191));
            Routing.RegisterRoute(nameof(_192), typeof(_192));
            Routing.RegisterRoute(nameof(_193), typeof(_193));
            Routing.RegisterRoute(nameof(_194), typeof(_194));
            Routing.RegisterRoute(nameof(_195), typeof(_195));
            Routing.RegisterRoute(nameof(_196), typeof(_196));
            #endregion
        }
        protected override void OnAppearing()
        {
            Task.Run(async () =>
            {
                await InitializeData();
            });
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }
    }
}