using UBViews.ViewModels;
using UBViews.Models.AppData;

namespace UBViews.Views;

public partial class AddContactsPage : ContentPage
{
	public AddContactsPage(AddContactsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
		vm.contentPage = this;
		vm.contentView = this.contactControl;
	}
}