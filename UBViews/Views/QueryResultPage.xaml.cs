using UBViews.ViewModels;

namespace UBViews.Views;

public partial class QueryResultPage : ContentPage
{
	public QueryResultPage(QueryResultViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
        vm.contentPage = this;
    }
}