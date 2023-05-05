using UBViews.ViewModels;

namespace UBViews.Views;

public partial class ContentTitlesPage : ContentPage
{
	public ContentTitlesPage(ContentTitlesViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
#if ANDROID
		this.innerContentGrid.SetValue(StyleProperty, "AndroidPaperContentTitlesVSL");
#endif
#if WINDOWS
        this.innerContentGrid.SetValue(StyleProperty, "WindowsPaperContentTitlesVSL");
#endif
    }
}