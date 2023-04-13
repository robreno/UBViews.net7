using UBViews.Helpers;
using UBViews.ViewModels;

namespace UBViews.Views;

public partial class QueryInputPage : ContentPage
{
	public QueryInputPage(QueryInputViewModel vm) 
	{ 
		InitializeComponent();
		BindingContext = vm;
	}

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }
}