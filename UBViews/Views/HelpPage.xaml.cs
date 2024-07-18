namespace UBViews.Views;

using UBViews.ViewModels;

public partial class HelpPage : ContentPage
{
	public HelpPage(HelpViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
        vm.contentPage = this;
	}
}