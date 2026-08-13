using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HamManager.RadioEngine.Rig;
using HamManager.RadioEngine.Transport;

namespace HamManager.App.ViewModels;

/// <summary>
/// Shell ViewModel. Phase 1 plumbing milestone: pick a port (or the
/// simulated rig), connect, see the VFO frequency live — including the
/// operator's knob turns arriving as unsolicited CI-V transceive frames.
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    /// <summary>The no-hardware entry in the port list.</summary>
    public const string SimulatedRig = "Simulated rig (no hardware)";

    private IRig? _rig;

    [ObservableProperty]
    private string _frequencyDisplay = "--.---.---";

    [ObservableProperty]
    private string _statusText = "Pick a port and connect";

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private string _connectButtonText = "Connect";

    [ObservableProperty]
    private string _selectedPort = SimulatedRig;

    /// <summary>The simulated rig plus every serial port on this machine.</summary>
    public ObservableCollection<string> AvailablePorts { get; }

    /// <summary>Runtime constructor.</summary>
    public MainWindowViewModel()
    {
        AvailablePorts = new ObservableCollection<string> { SimulatedRig };
        foreach (var name in SafePortNames())
        {
            AvailablePorts.Add(name);
        }
    }

    [RelayCommand]
    private async Task ToggleConnectAsync()
    {
        if (IsConnected)
        {
            await TearDownRigAsync();
            StatusText = "Disconnected";
            return;
        }

        var rig = CreateRig(SelectedPort);
        StatusText = $"Connecting to {SelectedPort}…";

        if (!await rig.ConnectAsync())
        {
            (rig as IDisposable)?.Dispose();
            StatusText = $"No answer on {SelectedPort} — check cable, baud and "
                       + "CI-V address (HM-OPEN-003)";
            return;
        }

        _rig = rig;
        rig.FrequencyChanged += OnRigFrequencyChanged;
        IsConnected = true;
        ConnectButtonText = "Disconnect";

        var hz = await rig.GetFrequencyHzAsync();
        FrequencyDisplay = FormatHz(hz);
        StatusText = SelectedPort == SimulatedRig
            ? "Connected — simulated rig (no hardware attached)"
            : $"Connected — IC-7300 on {SelectedPort} · CI-V bytes unverified until "
              + "HM-OPEN-002 closes";
    }

    private void OnRigFrequencyChanged(object? sender, FrequencyChangedEventArgs e)
    {
        // Engine events arrive on the read-loop thread; bindings update on
        // the UI thread only.
        Dispatcher.UIThread.Post(() => FrequencyDisplay = FormatHz(e.FrequencyHz));
    }

    private async Task TearDownRigAsync()
    {
        if (_rig is not null)
        {
            _rig.FrequencyChanged -= OnRigFrequencyChanged;
            await _rig.DisconnectAsync();
            (_rig as IDisposable)?.Dispose();
            _rig = null;
        }

        IsConnected = false;
        ConnectButtonText = "Connect";
        FrequencyDisplay = "--.---.---";
    }

    private static IRig CreateRig(string selection)
        => selection == SimulatedRig
            ? new FakeRig()
            : new Ic7300Rig(new SystemSerialPort(selection));

    private static IReadOnlyList<string> SafePortNames()
    {
        try
        {
            return SystemSerialPort.GetPortNames();
        }
        catch (Exception)
        {
            // No serial subsystem (some Linux containers): the simulated rig
            // still works, and the list stays honest rather than invented.
            return Array.Empty<string>();
        }
    }

    /// <summary>7030000 → "7.030.000" — the radio-face convention.</summary>
    internal static string FormatHz(long hz)
        => hz.ToString("N0", System.Globalization.CultureInfo.InvariantCulture)
             .Replace(",", ".");
}
