using UBViews.ViewModels;
namespace UBViews.Views;

public partial class AppDataPage : ContentPage
{
	public AppDataPage(AppDataViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}