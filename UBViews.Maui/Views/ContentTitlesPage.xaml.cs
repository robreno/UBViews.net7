using UBViews.ViewModels;

namespace UBViews.Views;

public partial class ContentTitlesPage : ContentPage
{
	public ContentTitlesPage(ContentTitlesViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}