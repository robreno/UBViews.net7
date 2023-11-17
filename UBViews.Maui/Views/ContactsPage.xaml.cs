namespace UBViews.Views;

using UBViews.ViewModels;
using UBViews.Models.AppData;

public partial class ContactsPage : ContentPage
{
	public ContactsPage(ContactsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
		vm.contentPage = this;
	}
}