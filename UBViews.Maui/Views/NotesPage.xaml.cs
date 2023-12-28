using UBViews.ViewModels;

namespace UBViews;

public partial class NotesPage : ContentPage
{
	public NotesPage(NotesViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
		vm.contentPage = this;
	}
}