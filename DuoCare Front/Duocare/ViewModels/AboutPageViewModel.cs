using CommunityToolkit.Mvvm.ComponentModel;
using System.Reflection;

namespace Duocare.ViewModels;

public partial class AboutPageViewModel : ObservableObject
{
    public string AppName => "Duocare";

    public string Version =>
        Assembly.GetExecutingAssembly()
                .GetName()
                .Version?
                .ToString() ?? "1.0.0";

    public string BuildDate =>
        File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location)
            .ToString("dd/MM/yyyy HH:mm");

    public string DeviceInfoSummary =>
        $"{DeviceInfo.Manufacturer} {DeviceInfo.Model} — {DeviceInfo.Platform} {DeviceInfo.VersionString}";
}
