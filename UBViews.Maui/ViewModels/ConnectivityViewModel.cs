namespace UBViews.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using UBViews.Helpers;

public partial class ConnectivityViewModel : ObservableObject
{
    IConnectivity connectivity;
    public ConnectivityViewModel(IConnectivity connectivity)
    {
        this.connectivity = connectivity;
    }

    [ObservableProperty]
    bool hasinternet;

    [ObservableProperty]
    bool bluetooth;

    [RelayCommand]
    async Task RequestBluetooth()
    {
        if (DeviceInfo.Platform != DevicePlatform.Android)
            return;

        var status = PermissionStatus.Unknown;

        if (DeviceInfo.Version.Major >= 12)
        {
            status = await Permissions.CheckStatusAsync<MyBluetoothPermission>();

            if (status == PermissionStatus.Granted)
                return;

            if (Permissions.ShouldShowRationale<MyBluetoothPermission>())
            {
                await Shell.Current.DisplayAlert("Needs permissions", "BECAUSE!!!", "OK");
            }

            status = await Permissions.RequestAsync<MyBluetoothPermission>();


        }
        else
        {
            status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Granted)
                return;

            if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
            {
                await Shell.Current.DisplayAlert("Needs permissions", "BECAUSE!!!", "OK");
            }

            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

        }


        if (status != PermissionStatus.Granted)
            await Shell.Current.DisplayAlert("Permission required",
                "Location permission is required for bluetooth scanning. " +
                "We do not store or use your location at all.", "OK");
    }

    internal async Task<bool> CheckInternet()
    {
        //NetworkAccess accessType = connectivity.NetworkAccess;
        var hasInternet = await Task.Run(() => connectivity?.NetworkAccess == NetworkAccess.Internet);
        return hasInternet;
    }
}
