using UBViews.ViewModels;

namespace UBViews.Views;

public partial class PaperTitlesPage : ContentPage
{
	public PaperTitlesPage(PaperTitlesViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
        vm.Title = "Titles of the Papers";
		vm.contentPage = this;
    }
}