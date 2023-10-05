namespace UBViews.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Platform;

using UBViews.Models;
using UBViews.Models.Query;

[QueryProperty(nameof(QueryResultLocationsDto), "LocationsDto")]
public partial class QueryResultViewModel : BaseViewModel
{
    Dictionary<string, Span> _spans = new Dictionary<string, Span>();
    //private bool targetRefId = false;

    /// <summary>
    /// 
    /// </summary>
    public ContentPage contentPage;

    /// <summary>
    /// 
    /// </summary>
    public ObservableCollection<QueryResultLocationsDto> QueryResultsCol { get; } = new();

    public QueryResultViewModel()
    {
    }

    [ObservableProperty]
    QueryResultLocationsDto locationsDto;

    [ObservableProperty]
    bool showReferencePids;

    [ObservableProperty]
    bool isScrollToLabel;

    [ObservableProperty]
    string scrollToLabelName;

    [RelayCommand]
    async Task QueryResultAppearing(QueryResultLocationsDto dto)
    {
        string methodName = "QueryResultAppearing";
        try
        {
            if (dto == null)
            {
                return;
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task QueryResultLoaded(QueryResultLocationsDto dto)
    {
        string methodName = "QueryResultLoaded";
        try
        {
            if (dto == null)
            {
                return;
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
            return;
        }
    }
}
