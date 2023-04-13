using CommunityToolkit.Mvvm.ComponentModel;

namespace UBViews.ViewModels;

public partial class BaseViewModel : ObservableObject
{

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    bool isBusy;

    [ObservableProperty]
    string title;

    [ObservableProperty]
    string partTitle;

    [ObservableProperty]
    string paperTitle;

    [ObservableProperty]
    string paperAuthor;
    public bool IsNotBusy => !IsBusy;
}
