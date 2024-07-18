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

    public async void ScrollToLabel(string name)
    {
        Label targetLabel = this.FindByName(name) as Label;
        await contentScrollView.ScrollToAsync(targetLabel, ScrollToPosition.Start, false);
    }

    //private void ContentPage_Loaded(object sender, EventArgs e)
    //{
    //    Label targetLabel = this.FindByName("RID_0_1_0") as Label;
    //    this.contentScrollView.ScrollToAsync(targetLabel, ScrollToPosition.Start, false);
    //}
}