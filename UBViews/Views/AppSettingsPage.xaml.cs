namespace UBViews.Views;

using UBViews.Services;
using UBViews.ViewModels;

/// <summary>
/// 
/// </summary>
public partial class AppSettingsPage : ContentPage
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="vm"></param>
    public AppSettingsPage(XmlAppSettingsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        vm.contentPage = this;
    }
}