using UBViews.Helpers;
using UBViews.ViewModels;

namespace UBViews.Views;

public partial class QueryInputPage : ContentPage
{
	public QueryInputPage(QueryInputViewModel vm) 
	{ 
		InitializeComponent();
		BindingContext = vm;
		vm.contentPage = this;
	}

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }

    private void queryResultScrollView_Scrolled(object sender, ScrolledEventArgs e)
    {

    }
}