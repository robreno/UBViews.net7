namespace UBViews.Controls.Help;

using CommunityToolkit.Maui.Views;
using UBViews.ViewModels;
public partial class AudioOverviewPopup : Popup
{
	public AudioOverviewPopup(PopupViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
		vm.popupPage = this;
	}
}