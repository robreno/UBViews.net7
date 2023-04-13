using UBViews.ViewModels;
namespace UBViews.Views;

public partial class PartsPage : ContentPage
{
	public PartsPage(PartsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
		vm.contentPage = this;
	}
}