namespace UBViews.Views;

using UBViews.ViewModels;
public partial class QueryResultPage2 : ContentPage
{
	public QueryResultPage2(QueryResultViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
        vm.contentPage = this;
	}
}