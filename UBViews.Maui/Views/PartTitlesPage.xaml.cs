using UBViews.ViewModels;
using UBViews.Models;

namespace UBViews.Views;

public partial class PartTitlesPage : ContentPage
{
	public PartTitlesPage(PartTitlesViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
		vm.rootPage = this;
	}
}