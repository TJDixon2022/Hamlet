using System.Globalization;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hamlet.App.Settings;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Scan;

namespace Hamlet.App.ViewModels;

/// <summary>
/// **THE SCAN, ON THE CW TAB** (work instruction 540, HM-DEC-244): the <c>Scan</c> button beside Record, Copy and Clear,
/// its three settings, and the line under the header that says what it is doing.
/// </summary>
/// <remarks>
/// <para>**THE OWNER, 2026-10-04:** *"I want a scan, like a radio scan where the radio used to scan when it sensed
/// something, it would stop. I want this to run unattended."* The scan itself is the engine's
/// (<see cref="CwCatchScan"/>); this starts and stops it, hands it the radio, the scope, the audio and Hamlet's data
/// folder, and shows what it says.</para>
/// <para>**WHILE IT RUNS, NOTHING TRANSMITS.** It holds <see cref="ListenOnlyLock"/>, which the CW transmitter, the
/// radio's keyer, the auto-caller's keyer and the push-to-talk sequence all ask before they key, so the send controls go
/// grey with the reason beside them and the code behind them refuses as well.</para>
/// </remarks>
public sealed partial class MainWindowViewModel
{
    /// <summary>The lock every keying path asks; held for as long as a scan runs.</summary>
    private readonly ListenOnlyLock _listenOnly = new();

    private CwCatchScan? _catchScan;
    private CwCatchEar? _catchEar;

    /// <summary>The lock, for the tests and for anything else that builds a keying path.</summary>
    internal ListenOnlyLock ListenOnlyLock => _listenOnly;

    /// <summary>True while a scan runs.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CatchScanLabel))]
    [NotifyPropertyChangedFor(nameof(TransmitIdleForScan))]
    private bool _isCatchScanning;

    /// <summary>The line under the CW terminal's header: what the scan is doing, or the hint where none runs.</summary>
    [ObservableProperty]
    private string _catchScanLine = string.Empty;

    /// <summary>The scan's length, in minutes: thirty by default.</summary>
    [ObservableProperty]
    private int _catchScanMinutes = (int)CwScanSettings.Defaults.Length.TotalMinutes;

    /// <summary>The positive stay, in seconds: ninety by default.</summary>
    [ObservableProperty]
    private int _catchPositiveStaySeconds = (int)CwScanSettings.Defaults.PositiveStay.TotalSeconds;

    /// <summary>The negative stay, in seconds: thirty by default.</summary>
    [ObservableProperty]
    private int _catchNegativeStaySeconds = (int)CwScanSettings.Defaults.NegativeStay.TotalSeconds;

    /// <summary>
    /// How long the scan watches each span before it visits what it saw, in seconds: three by default (work instruction 543).
    /// </summary>
    [ObservableProperty]
    private int _catchSurveySeconds = (int)CwScanSettings.DefaultSurveyTime.TotalSeconds;

    /// <summary>
    /// How many seconds Record keeps, from thirty to five minutes (work instruction 548, task 3): thirty by default. The tap is
    /// sized when Hamlet starts listening, so a change takes effect the next time it does.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CaptureTip))]
    private int _recordSeconds = AudioTap.SecondsKept;

    /// <summary>The button's word: Scan, or Stop while one runs.</summary>
    public string CatchScanLabel => IsCatchScanning ? "Stop" : "Scan";

    /// <summary>False while a scan runs: what the digital send controls are bound to, as a second guard to the lock.</summary>
    public bool TransmitIdleForScan => !IsCatchScanning;

    /// <summary>The button's hover.</summary>
    public const string CatchScanTip =
        "Scans the CW segment of the band the radio is on, unattended. It jumps to whatever the radio's scope shows, stays "
        + "while a shape can be read, up to its positive stay, and listens for its negative stay where none forms, keeping "
        + "the audio and what it read for each. It moves only the dial, never changes band, and never transmits: every send "
        + "is shut until it stops.";

    /// <summary>Where scans are kept: Hamlet's data folder, beside the telemetry.</summary>
    public static string CatchScansFolder => Path.Combine(SettingsStore.DataFolder, "scans");

    /// <summary>Start a scan, or stop the one running.</summary>
    [RelayCommand]
    private void ToggleCatchScan()
    {
        if (_catchScan is { } running)
        {
            running.Stop();
            return;
        }

        if (!IsCwMode)
        {
            CatchScanLine = "scan not started · the scan runs on the CW tab";
            return;
        }

        if (_rig is not { IsConnected: true } rig || _rigMonitor is not { } monitor)
        {
            CatchScanLine = "scan not started · no radio is connected";
            return;
        }

        if (_audioInput is not { } audio || !IsDecoding)
        {
            CatchScanLine = "scan not started · Hamlet is not listening to the radio yet";
            return;
        }

        if (SpectrumSource is not { } scope)
        {
            CatchScanLine = "scan not started · the radio's scope is not reaching Hamlet";
            return;
        }

        var width = monitor.State[RigField.FilterBandwidth] is { IsKnown: true, Number: { } w } && w > 0 ? w : 500;
        var ear = new CwCatchEar(audio, _settings.CwPitchHz, width, () => monitor.State);
        var settings = new CwScanSettings(
            TimeSpan.FromMinutes(Math.Max(1, CatchScanMinutes)),
            TimeSpan.FromSeconds(Math.Max(5, CatchPositiveStaySeconds)),
            TimeSpan.FromSeconds(Math.Max(5, CatchNegativeStaySeconds)))
        {
            SurveyTime = TimeSpan.FromSeconds(Math.Clamp(CatchSurveySeconds, 1, 30)),
        };
        var scan = new CwCatchScan(
            rig, monitor, scope, ear, _listenOnly, new FileScanHome(SettingsStore.ScanHomePath), CatchScansFolder, settings);

        scan.Said += line => Dispatcher.UIThread.Post(() => CatchScanLine = line);

        _catchScan = scan;
        _catchEar = ear;
        IsCatchScanning = true;
        CatchScanLine = "scanning";

        _ = Task.Run(async () =>
        {
            CwScanSummary? summary = null;

            try
            {
                summary = await scan.RunAsync().ConfigureAwait(false);
            }
            catch (Exception error)
            {
                // Never-throw discipline (§8): the scan's own code says so, and this is the belt to that.
                Dispatcher.UIThread.Post(() => CatchScanLine = "scan aborted · " + error.Message);
            }
            finally
            {
                Dispatcher.UIThread.Post(() =>
                {
                    ear.Dispose();
                    _catchScan = null;
                    _catchEar = null;
                    IsCatchScanning = false;

                    if (summary is not null)
                    {
                        CatchScanLine = summary.Sentence;
                    }

                    Transmit.Refresh();
                });
            }
        });

        Transmit.Refresh();
    }

    /// <summary>
    /// **LEAVING THE CW TAB STOPS THE SCAN** (work instruction 540): the tab writes the mode, and a scan carried on in
    /// another mode would be scanning for CW with the radio not in CW.
    /// </summary>
    /// <param name="mode">The tab now.</param>
    private void StopCatchScanForTab(string mode)
    {
        if (mode != "CW")
        {
            _catchScan?.StopFor(CwScanEnd.TabChanged);
        }
    }

    /// <summary>The scan's settings in words, for the popover's heading.</summary>
    public string CatchScanSettingsLine => string.Create(
        CultureInfo.InvariantCulture,
        $"runs {CatchScanMinutes} min · watches each span {CatchSurveySeconds} s · stays up to {CatchPositiveStaySeconds} s on a shape · {CatchNegativeStaySeconds} s where none forms · Record keeps {RecordSeconds} s · AGC in CW {AgcInCw}");

    // While the settings are being read in at start, a change is not a change to save.
    private bool _loadingCatchScanSettings;

    /// <summary>
    /// **THE SCAN REMEMBERS ITS SETTINGS** (work instruction 541): its length and two stays are kept in Hamlet's settings, as
    /// the app keeps its others, and read back at start. A value out of the popover's range is taken as the default.
    /// </summary>
    private void LoadCatchScanSettings()
    {
        _loadingCatchScanSettings = true;

        try
        {
            CatchScanMinutes = _settings.ScanMinutes is >= 1 and <= 600 ? _settings.ScanMinutes : 30;
            CatchPositiveStaySeconds = _settings.ScanPositiveStaySeconds is >= 5 and <= 600 ? _settings.ScanPositiveStaySeconds : 90;
            CatchNegativeStaySeconds = _settings.ScanNegativeStaySeconds is >= 5 and <= 600 ? _settings.ScanNegativeStaySeconds : 30;
            CatchSurveySeconds = _settings.ScanSurveySeconds is >= 1 and <= 30 ? _settings.ScanSurveySeconds : 3;
            RecordSeconds = _settings.RecordSeconds is >= AudioTap.SecondsKept and <= AudioTap.MaximumSecondsKept ? _settings.RecordSeconds : AudioTap.SecondsKept;
        }
        finally
        {
            _loadingCatchScanSettings = false;
        }
    }

    // **SAVED THE MOMENT IT CHANGES, NOT AT SHUTDOWN**, as the operating mode is: a preference written only on a clean exit is
    // lost whenever the app is closed the way people close it.
    private void SaveCatchScanSettings()
    {
        OnPropertyChanged(nameof(CatchScanSettingsLine));

        if (_loadingCatchScanSettings)
        {
            return;
        }

        _settings.ScanMinutes = CatchScanMinutes;
        _settings.ScanPositiveStaySeconds = CatchPositiveStaySeconds;
        _settings.ScanNegativeStaySeconds = CatchNegativeStaySeconds;
        _settings.ScanSurveySeconds = CatchSurveySeconds;
        _settings.RecordSeconds = RecordSeconds;
        SettingsStore.Save(_settings);
    }

    partial void OnCatchScanMinutesChanged(int value) => SaveCatchScanSettings();

    partial void OnCatchPositiveStaySecondsChanged(int value) => SaveCatchScanSettings();

    partial void OnCatchNegativeStaySecondsChanged(int value) => SaveCatchScanSettings();

    partial void OnCatchSurveySecondsChanged(int value) => SaveCatchScanSettings();

    partial void OnRecordSecondsChanged(int value) => SaveCatchScanSettings();
}
