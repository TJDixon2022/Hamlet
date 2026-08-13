using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamManager.RadioEngine.Rig;

namespace HamManager.App.ViewModels;

/// <summary>
/// Shell ViewModel. Look-and-feel milestone only: shows the frequency the
/// rig reports and a connect toggle. Real state flow, echo suppression and
/// the child VMs arrive with phase 1 plumbing.
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IRig _rig;

    [ObservableProperty]
    private string _frequencyDisplay = "--.---.---";

    [ObservableProperty]
    private string _statusText = "No radio connected — FakeRig standing by";

    [ObservableProperty]
    private bool _isConnected;

    public MainWindowViewModel(IRig rig)
    {
        _rig = rig;
    }

    /// <summary>Parameterless ctor for the XAML previewer only.</summary>
    public MainWindowViewModel() : this(new FakeRig())
    {
    }

    [RelayCommand]
    private async Task ToggleConnectAsync()
    {
        if (IsConnected)
        {
            await _rig.DisconnectAsync();
            IsConnected = false;
            StatusText = "Disconnected";
            FrequencyDisplay = "--.---.---";
            return;
        }

        IsConnected = await _rig.ConnectAsync();
        if (!IsConnected)
        {
            StatusText = "Connect failed — check HM-OPEN-003 station config";
            return;
        }

        var hz = await _rig.GetFrequencyHzAsync();
        FrequencyDisplay = FormatHz(hz);
        StatusText = "Connected — FakeRig (no hardware attached)";
    }

    /// <summary>7030000 → "7.030.000" — the radio-face convention.</summary>
    internal static string FormatHz(long hz)
        => hz.ToString("N0", System.Globalization.CultureInfo.InvariantCulture)
             .Replace(",", ".");
}
