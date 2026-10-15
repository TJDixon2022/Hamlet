using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Hamlet.App.Settings;
using Hamlet.RadioEngine.Cw;

namespace Hamlet.App.ViewModels;

/// <summary>
/// **WEAK STATIONS** (work instruction 571, HM-DEC-275): the scan's settings popover's last row, whether the lane bank hands a
/// weak station the grid cannot stand to the printer, and the story line that says once when one prints.
/// </summary>
/// <remarks>
/// On by default. It takes effect at once on the detector listening now and on every one built after, and it is kept across
/// restarts. The capture sheet and the `cw_listen` row carry it beside the AGC. Listen-only: nothing here keys or writes to
/// the radio.
/// </remarks>
public sealed partial class MainWindowViewModel
{
    /// <summary>The choices, in the popover's order.</summary>
    public static IReadOnlyList<string> WeakStationsChoices { get; } = ["on", "off"];

    /// <summary>The choice shown, one of <see cref="WeakStationsChoices"/>; kept across restarts.</summary>
    [ObservableProperty]
    private string _weakStations = "on";

    /// <summary>Whether the lane bank hands a weak station on.</summary>
    internal bool WeakStationsOn => WeakStations != "off";

    // While the setting is being read in at start, a change is not a change to save.
    private bool _loadingWeakStations;

    // The weak station the story line last named, so it is said once; NaN while none has been.
    private double _weakStationSaid = double.NaN;

    private void LoadWeakStations()
    {
        _loadingWeakStations = true;

        try
        {
            WeakStations = _settings.WeakStations == "off" ? "off" : "on";
        }
        finally
        {
            _loadingWeakStations = false;
        }
    }

    partial void OnWeakStationsChanged(string value)
    {
        if (_envelope is { } envelope)
        {
            envelope.LaneHandover = WeakStationsOn;
        }

        if (_loadingWeakStations)
        {
            return;
        }

        _settings.WeakStations = WeakStationsOn ? "on" : "off";
        SettingsStore.Save(_settings);
    }

    /// <summary>The capture sheet's line, beside the AGC's.</summary>
    private string WeakStationsSheetLine()
        => $"weak       {(WeakStationsOn ? "on" : "off")}  (whether the lane bank hands a weak station to the printer)";

    /// <summary>The capture sheet's weak-stations line, for a test.</summary>
    internal string WeakStationsSheetLineForTests() => WeakStationsSheetLine();

    /// <summary>
    /// Says once, when a lane's station prints, `Weak station at <c>pitch</c> Hz`: the lane holds a station and the printed
    /// sender is at its pitch. Said again only for another station, or once the lane has let the last one go.
    /// </summary>
    /// <param name="printedPitchHz">The printed sender's pitch, or nought where none prints.</param>
    private void SayWeakStation(double printedPitchHz)
    {
        var weak = _envelope?.WeakStationPitchHz ?? double.NaN;

        if (double.IsNaN(weak))
        {
            _weakStationSaid = double.NaN;
            return;
        }

        if (!(printedPitchHz > 0)
            || Math.Abs(printedPitchHz - weak) > CwEnvelopeDetector.HandoverHz
            || (!double.IsNaN(_weakStationSaid) && Math.Abs(_weakStationSaid - weak) <= CwEnvelopeDetector.HandoverHz))
        {
            return;
        }

        _weakStationSaid = weak;
        Narrate(string.Create(CultureInfo.InvariantCulture, $"Weak station at {weak:0} Hz"));
    }
}
